using System;
using System.Reflection;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Linq;
using static System.Windows.Forms.LinkLabel;
using System.Threading;

namespace Modbus.Common
{
    [Flags]
    public enum BusState : short
    {
        undefined = 0,
        off = 0x1,
        on = 0x2,
        error = unchecked((short)0x8000),
        errorActive = on | error,
        errorPassive = off | error
    }

    public partial class BaseForm : Form
    {
        private DisplayFormat _displayFormat = DisplayFormat.Integer;
        private CommunicationMode _communicationMode = CommunicationMode.TCP;
        protected Socket _socket;
        
        /** 
         * Helps to protect _registerData.
         * - About master application: all requests are triggered and processed from the main thread, thus no lock is 
         *  required
         * - About slave application: activities are spread between main thread and each worker thread which is raised
         * from an accepted connection
         */
        private object _dataLock = new object();
        private readonly ushort[] _registerData;

        private bool _logPaused = false;

        /** prefix for main window caption */
        private string _wndBaseName;

        /** dirty trick about property changes topic*/
        private bool _isLoaded;

        private AppOptions m_options = new AppOptions();

        /** cache the communication state*/
        private BusState m_connState;
        private object m_stateLock = new object();

        /** 
         * The folder path of the last load/save xml/csv file operation (communication file or data table file).
         * We keep it and do-it-yourself as the native MRU feature is not a "per application" feature but a 
         * "per extension" one, thus it's not very useful but annoying
         */
        private string _lastDataDir;

        /// <summary>
        /// Wrap some instance methods into a delegate
        /// </summary>
        /// <remarks>micro opt w/o doubt: just for avoid to instantiate delegate each time</remarks>
        private class Callers
        {
            public Callers(BaseForm self)
            {
                _appendLog = self.AppendLog;
                _setBusState = self.SetBusState;
                _updateDataTable = self.updateDataTable;
            }
            public readonly Action<string> _appendLog;
            public readonly Action<BusState> _setBusState;
            public readonly Action _updateDataTable;
        }

        private readonly Callers _call;

        #region Form 

        /// <summary>
        /// publish the private components instance, which is managed by the designer (it's wise to avoid to stress the
        /// designer ... and its fucking generated code)
        /// </summary>
        protected IContainer baseComponents => components;

        public BaseForm() : this("{Modbus window}", "{Modbus app}")
        {
            //dumyy ctor dummy for the designer
        }

        protected BaseForm(string wndBaseName, string appName, AppOptions options = null)
        {
            m_options = options ?? new AppOptions();

            InitializeComponent();
            _registerData = new ushort[65600];

            _wndBaseName = wndBaseName;

            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            txtAppVersion.Text = $"{appName} v{version.InformationalVersion}";

            _call = new Callers(this);
        }

        private void BaseFormLoading(object sender, EventArgs e)
        {
            comboBoxBaudRate.SelectedIndex = 4;
            FillRTUDropDownLists();
            CurrentTab.RegisterData = _registerData;
            if (_registerData == null)
            {
                throw new ApplicationException("Failed to allocate 128k block");
            }
            LoadUserData();
            CurrentTab.DisplayFormat = DisplayFormat;
            refreshData();

            //if options plan a target endpoint specification to be used, .... we use it -------------

            var endpoint = m_options.EndPointFilePath;
            if (string.IsNullOrWhiteSpace(endpoint) == false)
            {
                if (!File.Exists(endpoint))
                {
                    AppendLog($"[Warn] Unable to read device endpoint from file '{endpoint}'. File not found");
                    MessageBox.Show($"File '{endpoint}' is not found", "Initialization error !");
                }
                else
                {
                    loadCommunicationSpec(m_options.EndPointFilePath);
                }
            }

            //if options plan a data table, we load it. ------------------

            var dtable = m_options.DataTableFilePath;
            if(string.IsNullOrWhiteSpace(dtable) == false)
            {
                if (!File.Exists(dtable))
                {
                    AppendLog($"[Warn] Unable to read input data table from file '{dtable}'. File not found");
                    MessageBox.Show($"File '{dtable}' is not found", "Initialization error !");
                }
                else
                {
                    importDataTable(dtable, notice:false);
                }
            }

            _isLoaded = true;
            m_connState = BusState.off;
            updateWindowCaption();

            //if options plan an immediate communication, we actually bind the modbus server (when application is a 
            //a device slave) or start the scan (when application is a device master) ----------------

            if (m_options.AutoStart)
            {
                AppendLog("[Info] Autostart has been requested");
                //virtual-call
                StartCommunication();
            }
        }

        private void BaseFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveUserData();
        }

        private void FillRTUDropDownLists()
        {
            comboBoxSerialPorts.Items.Clear();
            foreach (var port in SerialPort.GetPortNames())
            {
                comboBoxSerialPorts.Items.Add(port);
            }
            if (comboBoxSerialPorts.Items.Count > 0)
                comboBoxSerialPorts.SelectedIndex = 0;
            comboBoxParity.Items.Clear();
            comboBoxParity.Items.Add(Parity.None.ToString());
            comboBoxParity.Items.Add(Parity.Odd.ToString());
            comboBoxParity.Items.Add(Parity.Even.ToString());
            comboBoxParity.Items.Add(Parity.Mark.ToString());
            comboBoxParity.Items.Add(Parity.Space.ToString());
        }

        private void LoadUserData()
        {
            CommunicationMode mode;
            if (Enum.TryParse(Properties.Settings.Default.CommunicationMode, out mode))
                CommunicationMode = mode;
            DisplayFormat format;
            if (Enum.TryParse(Properties.Settings.Default.DisplayFormat, out format))
                DisplayFormat = format;
            IPAddress ipAddress;
            if (IPAddress.TryParse(Properties.Settings.Default.IPAddress, out ipAddress))
                IPAddress = ipAddress;
            TCPPort = Properties.Settings.Default.TCPPort;
            PortName = Properties.Settings.Default.PortName;
            Baud = Properties.Settings.Default.Baud;
            Parity = Properties.Settings.Default.Parity;
            StartAddress = Properties.Settings.Default.StartAddress;
            DataLength = Properties.Settings.Default.DataLength;
            SlaveId = Properties.Settings.Default.SlaveId;
            SlaveDelay = Properties.Settings.Default.SlaveDelay;
            DataBits = Properties.Settings.Default.DataBits;
            StopBits = Properties.Settings.Default.StopBits;

            _lastDataDir = Properties.Settings.Default.LastDataDirectory;
            if (string.IsNullOrWhiteSpace(_lastDataDir)) _lastDataDir = Directory.GetCurrentDirectory();
        }

        private void SaveUserData()
        {
            Properties.Settings.Default.CommunicationMode = CommunicationMode.ToString();
            Properties.Settings.Default.IPAddress = IPAddress.ToString();
            Properties.Settings.Default.DisplayFormat = DisplayFormat.ToString();
            Properties.Settings.Default.TCPPort = TCPPort;
            Properties.Settings.Default.PortName = PortName;
            Properties.Settings.Default.Baud = Baud;
            Properties.Settings.Default.Parity = Parity;
            Properties.Settings.Default.StartAddress = StartAddress;
            Properties.Settings.Default.DataLength = DataLength;
            Properties.Settings.Default.SlaveId = SlaveId;
            Properties.Settings.Default.SlaveDelay = SlaveDelay;
            Properties.Settings.Default.DataBits = DataBits;
            Properties.Settings.Default.StopBits = StopBits;
            Properties.Settings.Default.LastDataDirectory = _lastDataDir;

            Properties.Settings.Default.Save();
        }

        #endregion

        #region Import - Export

        private void ButtonImportClick(object sender, EventArgs e)
        {
            prepareFileDialog(string.Empty, "csv", saveOrLoad: false);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _lastDataDir = Path.GetDirectoryName(openFileDialog.FileName);

                lock (_dataLock)
                {
                    importDataTable(openFileDialog.FileName, notice:true);
                }
            }
        }

        private void importDataTable(string filepath, bool notice)
        {
            int count = importDataTable(filepath);
            if (notice && count < 0)
            {
                string msg = $"Inconsistent data table due to {count} discarded records. The 'registers count'" +
                    $"and/or 'Start address' should be reviewed";

                MessageBox.Show(msg, "Import error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Read data table from a csv file
        /// </summary>
        /// <param name="filepath"></param>
        /// <remarks>
        /// - PRE: lock taken <br/>
        /// - POST: <see cref="StartAddress"/> is set to the 1th valid record address<br/>
        /// - POST: <see cref="DataLength"/> is set to the whole records count. In case of invalid entries, it might
        /// be inconsistent, depending on where and which of the registers are failed to be parsed
        /// </remarks>
        /// <returns>
        /// - a positive value tells the imported registers count (native registers), i.e a full successfully import.
        /// Shall be equal to <see cref="DataLength"/>
        /// <br/>
        /// - a negative value tells the discarded registers entries count. The <see cref="DataLength"/> shall be
        /// inconsistent with the truly updated registers count<br/>
        /// </returns>
        private int importDataTable(string filepath)
        {
            //invalid entries count: either 16 or 32 bit
            int discarded = 0;
            //valid registers count: either 16 or 32 bit
            int rcount = 0;
            bool is32bit = false;

            using (var s = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                DisplayFormat fmt = DisplayFormat.Integer; //fallback (decimal expected)

                using (var r = new StreamReader(s))
                {
                    var rec = r.ReadToEnd();
                    var sets = rec.Split(',');

                    //guess the (unique) format --------------------------------------------------------
                    //as of now, all values share the same format

                    ushort addr; string value;
                    int i = 0;
                    while(i < sets.Length)
                    {
                        if (!tryRegisterEntry(sets[i++], out addr, out value))
                        {
                            discarded++;
                            continue;
                        }

                        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            fmt = DisplayFormat.Hex;
                        }
                        else if (value.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                        {
                            //0b or b prefixes are not standard ... but pseudo standard and especially convenient
                            //should be binary (... or LED !!!)
                            fmt = DisplayFormat.Binary;
                        }
                        else if (value.Contains("."))
                        {
                            //floating-point have been stored with neutral culture
                            fmt = DisplayFormat.Float32;
                            is32bit = true;
                        }

                        SetFunction(fmt);
                        StartAddress = addr;

                        break;
                    }

                    //parse the records from the 1th valid ------------------------------------------------------------

                    foreach (var s1 in sets.Skip(i-1))
                    {
                        if(!tryRegisterEntry(s1, out addr, out value))
                        {
                            discarded++;
                            continue;
                        }

                        if (fmt == DisplayFormat.Hex) 
                        {
                            var sub = value.Substring(2); //eat prefix

                            if (!ushort.TryParse(sub, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex)
                                || !trySetRegister(addr, hex))
                                discarded++;
                            else
                                rcount++;
                        }
                        else if(fmt == DisplayFormat.Float32)
                        {
                            //floating-point have been stored with neutral culture --

                            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f32)
                                || !trySetRegisters(addr, f32))
                                discarded++;
                            else
                                rcount++;
                        }
                        else if (fmt == DisplayFormat.Binary || fmt == DisplayFormat.LED)
                        {
                            var sub = value.Substring(2); //eat prefix

                            try
                            {
                                var bin = Convert.ToUInt16(sub, 2);
                                if (!trySetRegister(addr, bin))
                                    discarded++;
                                else
                                    rcount++;
                            }
                            catch (Exception) { discarded++; }
                        }
                        else if(fmt == DisplayFormat.Integer)
                        {
                            if (!ushort.TryParse(value, out var dec) || !trySetRegister(addr, dec))
                                discarded++;
                            else
                                rcount++;
                        }
                        else
                        {
                            //not reached
                            discarded++;
                        }
                    }

                    r.Close();

                    //is inconsistent when discarded != 0
                    DataLength = (ushort)(rcount * (is32bit ? 2 : 1));

                    if (discarded == 0)
                        AppendLog($"[Info] A data table has been loaded from '{filepath}' with {rcount} registers "+
                            $"({(is32bit ? "32" : "16")}bit)");
                    else
                        SetError($"A data table has been loaded from '{filepath}' with {rcount} registers " +
                            $"({(is32bit ? "32" : "16")}bit) but {discarded} others have been discarded, data table might be corrupted");
                }
                s.Close();
            }

            // display data
            refreshData();

            return (discarded != 0 ? -discarded : rcount) * (is32bit ? 2 : 1);

            /** 
             * parse/split a 16/32 bit register record (register_offset:register_value) 
             * - address is parsed as a decimal or hexa value
             * - value is not parsed
             */
            bool tryRegisterEntry(string entry, out ushort addr, out string value)
            {
                addr = 0;
                value = string.Empty;

                int pos = entry.IndexOf(':');
                if (pos == -1) 
                    return false;

                //decimal or hexa address
                if (!ushort.TryParse(entry.Substring(0, pos), NumberStyles.Integer, CultureInfo.InvariantCulture, out addr))
                {
                    if (!ushort.TryParse(entry.Substring(0, pos), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out addr))
                        return false;
                }

                value = entry.Substring(pos +1);
                return true;
            }
            /**
             * set a 16bit register
             */
            bool trySetRegister(ushort addr, ushort value)
            {
                if (addr >= _registerData.Length) return false;

                _registerData[addr] = value;

                return true;
            }
            /**
             * set a 32 bit register, marshalling a floating point value as big-endian
             */
            bool trySetRegisters(ushort addr, float value)
            {
                if (addr+1 >= _registerData.Length) return false;

                //FloatReverse is float.bigendian ... 
                Marshaller.ToBinary(value, new ArraySegment<ushort>(_registerData, addr, 2), Endianness.BE);

                return true;
            }
        }

        public delegate void SetFunctionDelegate(DisplayFormat log);

        protected void SetFunction(DisplayFormat fmt)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetFunctionDelegate(SetFunction), new object[] { fmt });
                return;
            }
            DisplayFormat = fmt;
            switch (fmt)
            {
                case DisplayFormat.Integer:
                    radioButtonInteger.Checked = true;
                    break;
                case DisplayFormat.Binary:
                    radioButtonBinary.Checked = true;
                    break;
                case DisplayFormat.Hex:
                    radioButtonHex.Checked = true;
                    break;
                case DisplayFormat.LED:
                    radioButtonLED.Checked = true;
                    break;
                case DisplayFormat.Float32:
                    radioButtonReverseFloat.Checked = true;
                    break;
            }
        }

        private void ButtonExportClick(object sender, EventArgs e)
        {
            int offset = StartAddress;
            int end = StartAddress + DataLength;
            bool is32bit = false;

            string suffix = "-";
            switch (DisplayFormat)
            {
                case DisplayFormat.Integer:
                    suffix = "_Decimal_";
                    break;
                case DisplayFormat.Hex:
                    suffix = "_HEX_";
                    break;
                case DisplayFormat.Binary:
                    suffix = "_Binary_";
                    break;
                case DisplayFormat.LED:
                    suffix = "_LED_";
                    break;
                case DisplayFormat.Float32:
                    suffix = "_Fpt_";
                    is32bit = true;
                    break;
            }

            //adjust upper bound
            if (is32bit) end--;

            var filename = "ModbusExport_" + offset + suffix + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv";
            prepareFileDialog(filename, "csv", saveOrLoad: true);

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _lastDataDir = Path.GetDirectoryName(saveFileDialog.FileName);

                using (var s = saveFileDialog.OpenFile())
                {
                    using (var w = new StreamWriter(s))
                    {
                        ushort regCount = (ushort)(DisplayFormat.Float32 == DisplayFormat ? 2 : 1);

                        while(offset < end)
                        {
                            w.Write(offset);
                            w.Write(':');

                            ushort data;
                            switch (DisplayFormat)
                            {
                                case DisplayFormat.Integer:
                                    data = _registerData[offset];
                                    w.Write(string.Format("{0}", data));
                                    break;
                                case DisplayFormat.Hex:
                                    data = _registerData[offset];
                                    w.Write(string.Format("0x{0:x4}", data));
                                    break;
                                case DisplayFormat.Float32:
                                    float f32 = Marshaller.FloatFromBinary(new ArraySegment<ushort>(_registerData, offset, 2), Endianness.BE);
                                    w.Write(f32.ToString("E9", CultureInfo.InvariantCulture));
                                    break;
                                case DisplayFormat.Binary:
                                case DisplayFormat.LED:
                                    data = _registerData[offset];
                                    w.Write(string.Concat("0b", Convert.ToString(data, 2).PadLeft(16, '0')));
                                    break;
                            }

                            if (offset < end - 1)
                                w.Write(',');

                            offset += regCount;
                        }

                        w.Flush();
                        w.Close();
                    }
                    s.Close();
                }
            }
        }

        #endregion

        #region Radion button check handlers

        private void RadioButtonModeChanged(object sender, EventArgs e)
        {
            SetMode();

            updateWindowCaption();
        }

        protected void SetMode()
        {
            if (radioButtonTCP.Checked)
            {
                _communicationMode = CommunicationMode.TCP;
                groupBoxTCP.Enabled = true;
                groupBoxRTU.Enabled = false;
            }
            if (radioButtonRTU.Checked)
            {
                _communicationMode = CommunicationMode.RTU;
                groupBoxTCP.Enabled = false;
                groupBoxRTU.Enabled = true;
            }
            if (radioButtonUDP.Checked)
            {
                _communicationMode = CommunicationMode.UDP;
                groupBoxTCP.Enabled = true;
                groupBoxRTU.Enabled = false;
            }
        }

        private void RadioButtonDisplayFormatCheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton)
            {
                var rb = (RadioButton)sender;
                if (rb.Checked)
                {
                    DisplayFormat.TryParse(rb.Tag.ToString(), true, out _displayFormat);
                    CurrentTab.DisplayFormat = DisplayFormat;

                    lock(_dataLock) refreshData();
                }
            }
        }

        #endregion

        #region properties

        private ushort _startAddress;
        protected ushort StartAddress
        {
            get
            {
                return _startAddress;
            }
            set
            {
                CurrentTab.StartAddress = value;
                var tab = tabControl1.SelectedTab;
                tab.Text = value.ToString();
                _startAddress = value;
            }
        }

        private ushort _dataLength;
        private bool showDataLength;

        protected ushort DataLength
        {
            get
            {
                return _dataLength;
            }
            set
            {
                _dataLength = value;
                CurrentTab.DataLength = value;
            }
        }

        public bool ShowDataLength
        {
            get => showDataLength;
            set
            {
                showDataLength = value;
                foreach (var tab in tabPage1.Controls.OfType<DataTab>())
                {
                    tab.ShowDataLength = value;
                }
                foreach (var tab in tabPage2.Controls.OfType<DataTab>())
                {
                    tab.ShowDataLength = value;
                }
            }
        }
        protected IPAddress IPAddress
        {
            get
            {
                return IPAddress.Parse(txtIP.Text);
            }
            set
            {
                txtIP.Text = value.ToString();
            }
        }

        protected int TCPPort
        {
            get
            {
                return Int32.Parse(textBoxPort.Text);
            }
            set
            {
                textBoxPort.Text = Convert.ToString(value);

            }
        }

        protected byte SlaveId
        {
            get
            {
                return Byte.Parse(textBoxSlaveID.Text);
            }
            set
            {
                textBoxSlaveID.Text = Convert.ToString(value);
            }
        }

        protected int SlaveDelay
        {
            get
            {
                return int.Parse(textBoxSlaveDelay.Text);
            }
            set
            {
                textBoxSlaveDelay.Text = Convert.ToString(value);
            }
        }

        protected string PortName
        {
            get
            {
                return comboBoxSerialPorts.Text;
            }
            set
            {
                comboBoxSerialPorts.Text = value;
            }
        }

        protected int Baud
        {
            get
            {
                return Int32.Parse(comboBoxBaudRate.Text);
            }
            set
            {
                comboBoxBaudRate.SelectedItem = Convert.ToString(value);
            }
        }

        protected Parity Parity
        {
            get
            {
                var parity = Parity.None;
                if (comboBoxParity.SelectedItem?.Equals(Parity.None.ToString()) == true)
                {
                    parity = Parity.None;
                }
                else if (comboBoxParity.SelectedItem?.Equals(Parity.Odd.ToString()) == true)
                {
                    parity = Parity.Odd;
                }
                else if (comboBoxParity.SelectedItem?.Equals(Parity.Even.ToString()) == true)
                {
                    parity = Parity.Even;
                }
                else if (comboBoxParity.SelectedItem?.Equals(Parity.Mark.ToString()) == true)
                {
                    parity = Parity.Mark;
                }
                else if (comboBoxParity.SelectedItem?.Equals(Parity.Space.ToString()) == true)
                {
                    parity = Parity.Space;
                }
                return parity;
            }
            set
            {
                comboBoxParity.SelectedItem = Convert.ToString(value);
            }
        }

        protected int DataBits
        {
            get
            {
                int bits = 8;
                switch (comboBoxDataBits.SelectedIndex)
                {
                    case 0:
                        bits = 7;
                        break;
                    case 1:
                        bits = 8;
                        break;
                }
                return bits;
            }
            set
            {
                switch (value)
                {
                    case 7:
                        comboBoxDataBits.SelectedIndex = 0;
                        break;
                    case 8:
                    default:
                        comboBoxDataBits.SelectedIndex = 1;
                        break;
                }
            }
        }

        protected StopBits StopBits
        {
            get
            {
                StopBits bits = StopBits.None;
                switch (comboBoxStopBits.SelectedIndex)
                {
                    case 0:
                        bits = StopBits.None;
                        break;
                    case 1:
                        bits = StopBits.One;
                        break;
                    case 2:
                        bits = StopBits.OnePointFive;
                        break;
                    case 3:
                        bits = StopBits.Two;
                        break;
                }
                return bits;
            }
            set
            {
                switch (value)
                {
                    case StopBits.None:
                        comboBoxStopBits.SelectedIndex = 0;
                        break;
                    case StopBits.One:
                        comboBoxStopBits.SelectedIndex = 1;
                        break;
                    case StopBits.OnePointFive:
                        comboBoxStopBits.SelectedIndex = 2;
                        break;
                    case StopBits.Two:
                        comboBoxStopBits.SelectedIndex = 3;
                        break;
                }
            }
        }

        protected DisplayFormat DisplayFormat
        {
            get { return _displayFormat; }
            set
            {
                switch (value)
                {
                    case DisplayFormat.LED:
                        radioButtonLED.Checked = true;
                        break;
                    case DisplayFormat.Binary:
                        radioButtonBinary.Checked = true;
                        break;
                    case DisplayFormat.Hex:
                        radioButtonHex.Checked = true;
                        break;
                    case DisplayFormat.Integer:
                        radioButtonInteger.Checked = true;
                        break;
                    case DisplayFormat.Float32:
                        radioButtonReverseFloat.Checked = true;
                        break;
                }
                _displayFormat = value;
                CurrentTab.DisplayFormat = DisplayFormat;
                lock(_dataLock) refreshData();
            }
        }

        protected CommunicationMode CommunicationMode
        {
            get { return _communicationMode; }
            set
            {
                switch (value)
                {
                    case CommunicationMode.TCP:
                        radioButtonTCP.Checked = true;
                        break;
                    case CommunicationMode.UDP:
                        radioButtonUDP.Checked = true;
                        break;
                    case CommunicationMode.RTU:
                        radioButtonRTU.Checked = true;
                        break;
                }
                _communicationMode = value;
            }
        }

        #endregion

        #region Logging

        public delegate void AppendLogDelegate(String log);

        protected void ButtonClearLogClick(object sender, EventArgs e)
        {
            listBoxCommLog.Items.Clear();
        }

        private void buttonPauseLog_Click(object sender, EventArgs e)
        {
            _logPaused = !_logPaused;
            buttonPauseLog.Text = _logPaused ? "Resume" : "Pause";
        }

        private async void buttonSaveLog_ClickAsync(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "ModbusLog";
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.DefaultExt = "txt";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, append: true))
                {
                    foreach (string line in listBoxCommLog.Items)
                    {
                        await writer.WriteLineAsync(line);
                    }
                }
            }
        }

        protected void DriverIncommingData(byte[] data, int len)
        {
            if (_logPaused)
                return;
            var hex = new StringBuilder(len);
            for(int i = 0; i < len; i++)
            {
                hex.AppendFormat("{0:x2} ", data[i]);
            }
            AppendLog(String.Format("[activity.rx] {0}", hex));
        }

        protected void DriverOutgoingData(byte[] data)
        {
            if (_logPaused)
                return;
            var hex = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
                hex.AppendFormat("{0:x2} ", b);
            AppendLog(String.Format("[activity.tx] {0}", hex));
        }

        protected void AppendLog(String log)
        {
            if (_logPaused)
                return;
            if (InvokeRequired)
            {
                BeginInvoke(_call._appendLog, log);
                return;
            }
            var now = DateTime.Now;
            var tmpStr = ">" + now.ToLongTimeString() + ": " + log;
            listBoxCommLog.Items.Add(tmpStr);
            listBoxCommLog.SelectedIndex = listBoxCommLog.Items.Count - 1;
            //listBoxCommLog.SelectedIndex = -1;
        }

        #endregion

        #region Data Table

        protected void ButtonDataClearClick(object sender, EventArgs e)
        {
            ClearRegisterData();
        }

        protected void ClearRegisterData()
        {
            lock(_dataLock)
            {
                for (int i = 0; i < _registerData.Length; i++)
                {
                    _registerData[i] = 0;
                }
                refreshData();
            }
        }

        protected DataTab CurrentTab
        {
            get
            {
                var tab = tabControl1.SelectedTab;
                return tab.Controls.OfType<DataTab>().First();
            }
        }

        /// <summary>
        /// UI datatable (re)construct
        /// </summary>
        /// <remarks>PRE: lock taken</remarks>
        private void refreshData()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(refreshData));
                return;
            }
            CurrentTab.RefreshData();

            //  Reset event handler
            CurrentTab.OnApply -= dataTab_OnApply;
            CurrentTab.OnApply += dataTab_OnApply;
        }

        /// <summary>
        /// update UI
        /// </summary>
        /// <remarks>PRE: lock taken</remarks>
        private void updateDataTable()
        {
            if (InvokeRequired)
            {
                BeginInvoke(_call._updateDataTable);
                return;
            }

            //note: data reference of CurrentTab points to _registerData
            CurrentTab.UpdateDataTable();
        }

        /// <summary>
        /// Get a single register from the data table
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>[false] addr out of bounds</returns>
        protected bool TryGetRegister(ushort offset, out ushort value)
        {
            lock (_dataLock)
            {
                if(!tryRegAddr(offset, 1))
                {
                    value = 0;
                    return false;
                }
                else
                {
                    value = _registerData[offset];
                    return true;
                }
            }
        }

        /// <summary>
        /// Copy multiple registers from the data table
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="dest">target buffer</param>
        /// <param name="count">registers count</param>
        /// <returns>[false] addr out of bounds</returns>
        protected bool TryGetRegisters(ushort offset, ushort[] dest, ushort count)
        {
            lock(_dataLock)
            {
                if (!tryRegAddr(offset, count)) return false;

                Array.Copy(_registerData, offset, dest, 0, count);
                return true;
            }
        }

        private bool tryRegAddr(ushort offset, ushort count)
        {
            if (offset + count > _registerData.Length)
            {
                AppendLog($"Attempt to dereference invalid register addresses [{offset}-{offset + count - 1}] "+
                    $"bounds:[0-{_registerData.Length - 1}");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Update a single register to the data table
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="src"></param>
        /// <param name="update">update the UI data table</param>
        /// <returns>[false] addr out of bounds</returns>
        /// <remarks>Should be the main use case (as the sole writing originator is the end user and the gui) but 
        /// the application architecture seems to prefer to update the whole data table of the active tab</remarks>
        protected bool TryPutRegister(ushort offset, ushort src, bool update)
        {
            lock(_dataLock)
            {
                if (!tryRegAddr(offset, 1)) return false;

                _registerData[offset] = src; 

                if (update) updateDataTable();

                return true;
            }
        }

        /// <summary>
        /// Update multiple registers to the data table
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="src">src.Length registers will be written</param>
        /// <param name="update">update the UI data table</param>
        /// <returns>[false] addr out of bounds</returns>
        protected bool TryPutRegisters(ushort offset, ushort[] src, bool update)
        {
            lock(_dataLock)
            {
                if (!tryRegAddr(offset, (ushort)src.Length)) return false;

                src.CopyTo(_registerData, offset);

                if (update) updateDataTable();

                return true;
            }
        }

        protected void Synchronize(DataTab tab)
        {
            tab.SetSynchronize(enterLockData, releaseLockData);
        }

        private void enterLockData() { Monitor.Enter(_dataLock); }
        private void releaseLockData() { Monitor.Exit(_dataLock); }

        #endregion

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            lock (_dataLock)
            {
                CurrentTab.RegisterData = _registerData;
                CurrentTab.DisplayFormat = DisplayFormat;
                var tab = tabControl1.SelectedTab;
                if (tab.Text.Equals("...") && tabControl1.TabPages.Count < 20)
                {
                    DataTab dataTab = new DataTab();
                    dataTab.DataLength = 256;
                    dataTab.DisplayFormat = DisplayFormat.Integer;
                    dataTab.Location = new Point(3, 3);
                    dataTab.Name = "dataTab" + (tabControl1.TabPages.Count + 1);
                    dataTab.RegisterData = _registerData;
                    dataTab.ShowDataLength = ShowDataLength;
                    dataTab.Size = new Size(839, 406);
                    dataTab.StartAddress = 0;
                    dataTab.TabIndex = 0;
                    dataTab.OnApply += dataTab_OnApply;
                    TabPage tabPage = new TabPage();
                    tabPage.Controls.Add(dataTab);
                    tabPage.Location = new Point(4, 22);
                    tabPage.Name = "tabPage" + (tabControl1.TabPages.Count + 1);
                    tabPage.Padding = new Padding(3);
                    tabPage.Size = new Size(851, 411);
                    tabPage.TabIndex = tabControl1.TabPages.Count;
                    tabPage.Text = "...";
                    tabPage.UseVisualStyleBackColor = true;
                    tabControl1.Controls.Add(tabPage);
                }
                var address = CurrentTab.StartAddress;
                tab.Text = address.ToString();
                _startAddress = address;
                _dataLength = CurrentTab.DataLength;
            }
        }

        void dataTab_OnApply(object sender, EventArgs e)
        {
            var tab = tabControl1.SelectedTab;
            var address = CurrentTab.StartAddress;
            tab.Text = address.ToString();
            _startAddress = address;
            _dataLength = CurrentTab.DataLength;
        }

        private void donate_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/ClassicDIY/ModbusTool";
            System.Diagnostics.Process.Start(url);
        }

        #region connection persistence and management

        /// <summary>
        /// Store the current target connection parameters into a xml file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveComClick(object sender, EventArgs e)
        {
            prepareFileDialog("ModbusSlaveEndPoint", "xml", saveOrLoad: true);

            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            _lastDataDir = Path.GetDirectoryName(saveFileDialog.FileName);

            var serializer = new XmlSerializer(typeof(ModbusConnConfiguration));
            using (var file = new StreamWriter(saveFileDialog.FileName))
            {
                var conf = getCurrentConfiguration();
                serializer.Serialize(file, conf);
            }
        }

        /// <summary>
        /// Load the current target connection parameters from a xml file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadComClick(object sender, EventArgs e)
        {
            prepareFileDialog("ModbusSlaveEndPoint", "xml", saveOrLoad: false);

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            _lastDataDir = Path.GetDirectoryName(openFileDialog.FileName);

            loadCommunicationSpec(openFileDialog.FileName);
            updateWindowCaption();
        }

        /// <summary>
        /// load from the supplied file, the slave device connection modbus parameters
        /// </summary>
        /// <param name="filepath">xml file path</param>
        private void loadCommunicationSpec(string filepath)
        {
            var serializer = new XmlSerializer(typeof(ModbusConnConfiguration));
            using (var file = new StreamReader(filepath))
            {
                var conf = serializer.Deserialize(file) as ModbusConnConfiguration;
                if (conf.ModbusMode == CommunicationMode.TCP || conf.ModbusMode == CommunicationMode.UDP)
                {
                    var ip_conf = conf.TryCast<ModbusIPConfiguration>();
                    TCPPort = ip_conf.Port;
                    IPAddress = ip_conf.Address;
                }
                else if (conf.ModbusMode == CommunicationMode.RTU)
                {
                    var serial = conf.TryCast<ModbusRTUConfiguration>();
                    PortName = serial.PortName;
                    Baud = serial.BaudRate;
                    Parity = serial.Parity;
                    StopBits = serial.StopBits;
                    DataBits = serial.DataBits;
                }

                CommunicationMode = conf.ModbusMode;
                SlaveId = conf.SlaveID;
            }

            AppendLog($"[Info] Device endpoint has been loaded from file '{filepath}'");
        }
        
        /// <summary>
        /// Retrieve configuration object from the GUI
        /// </summary>
        /// <returns>null if initializing</returns>
        private ModbusConnConfiguration getCurrentConfiguration()
        {
            if (!_isLoaded) return null;

            IModbusConfiguration conf = null;

            if (_communicationMode == CommunicationMode.TCP)
                conf = new ModbusTCPConfiguration() 
                { 
                    EndPoint = new IPEndPoint(IPAddress, TCPPort) 
                };
            else if (_communicationMode == CommunicationMode.RTU) 
                conf = new ModbusRTUConfiguration() 
                { 
                    PortName = PortName, BaudRate = Baud, 
                    Parity = Parity, StopBits = StopBits, DataBits = (byte)DataBits
                };
            else if (_communicationMode == CommunicationMode.UDP) 
                conf = new ModbusUDPConfiguration() 
                { 
                    EndPoint = new IPEndPoint(IPAddress, TCPPort) 
                };

            return ModbusConnConfiguration.Create(conf, SlaveId);
        }

        /// <summary>
        /// </summary>
        /// <remarks>Override to effectively start the bus</remarks>
        protected virtual void StartCommunication() { }

        /// <summary>
        /// Update connected/disconnected state and GUI
        /// </summary>
        /// <param name="state"></param>
        /// <remarks>Must be executed in main thread (UI)</remarks>
        protected void SetBusState(BusState state)
        {
            if (InvokeRequired)
            {
                BeginInvoke(_call._setBusState, state);
                return;
            }

            lock(m_stateLock) DoSetBusState(state);
        }
 
        /// <summary>
        /// Actually update connected/disconnected state and GUI
        /// </summary>
        /// <param name="state"></param>
        /// <remarks>Do not forget to call the base class when overriden<br/>
        /// This method is an implementation method and must not be called directy (use <see cref="SetBusState(BusState)"/><br/>
        /// @pre lock is held
        /// </remarks>
        protected virtual void DoSetBusState(BusState state)
        {
            if (m_connState == BusState.off)
            {
                //direct transition from OFF to ErrorPassive does not make sense
                if (!state.HasFlag(BusState.error))
                    m_connState = state;
            }
            else
            {
                if(m_connState.HasFlag(BusState.on) && state == BusState.off)
                {
                    AppendLog("Disconnected");
                }

                m_connState = state;
            }

            if (state.HasFlag(BusState.on))
            {
                groupBoxTCP.Enabled = false;
                groupBoxRTU.Enabled = false;
                groupBoxMode.Enabled = false;
                grpExchange.Enabled = false;
                grpStart.Enabled = false;
            }
            else
            {
                groupBoxMode.Enabled = true;
                grpExchange.Enabled = true;
                grpStart.Enabled = true;

                SetMode();
            }

            updateWindowCaption();
        }

        /// <summary>
        /// Appends error log and updates bus state
        /// </summary>
        /// <param name="message"></param>
        /// <param name="passive">critical error</param>
        protected void SetError(string message, bool passive = false)
        {
            AppendLog($"[error] {message}");

            if(passive) SetBusState(BusState.errorPassive);
            else SetBusState(m_connState | BusState.error);
        }

        #endregion

        private void updateWindowCaption()
        {
            var conf = getCurrentConfiguration();
            if (conf == null) return;

            Text = string.Concat(_wndBaseName, " ", conf.ToString(), $" [bus:{m_connState.ToString()}]");
        }

        private void onSerialPortsTextChanged(object sender, EventArgs e)
        {
            updateWindowCaption();
        }

        private void onSlaveIDValidated(object sender, EventArgs e)
        {
            updateWindowCaption();
        }

        private void onAddressValidated(object sender, EventArgs e)
        {
            updateWindowCaption();
        }

        private void onIPPortValidated(object sender, EventArgs e)
        {
            updateWindowCaption();
        }

        private void onBtnContactClick(object sender, EventArgs e)
        {
            string url = "https://github.com/alterCoding/ModbusTool";
            System.Diagnostics.Process.Start(url);
        }

        private void onBtnContactHover(object sender, EventArgs e)
        {
            btnContact.Image = Properties.Resources.github32;
        }

        private void onBtnContactLeave(object sender, EventArgs e)
        {
            btnContact.Image = Properties.Resources.share32;
        }

        private void prepareFileDialog(string fname, string ext, bool saveOrLoad)
        {
            FileDialog dlg;
            if (saveOrLoad) dlg = saveFileDialog; else dlg = openFileDialog;

            dlg.InitialDirectory = _lastDataDir; //arguable convenience
            dlg.Filter = $"{ext} files (*.{ext})|*.{ext}|All files (*.*)|*.*";
            dlg.DefaultExt = $".{ext}";
            dlg.FileName = fname;
        }
    }
}
