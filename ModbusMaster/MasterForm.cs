using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Modbus.Common;
using ModbusLib;
using ModbusLib.Protocols;

namespace ModbusMaster
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// Architecture communication is very basic. It's a single-threaded application (main). All the modbus
    /// single commands, but also the polling (if any) are processed into the window loop (thus the main thread). As a
    /// result, the shared registers map is not synchronized
    /// </remarks>
    public partial class MasterForm : BaseForm
    {
        private int _transactionId;
        private ModbusClient _driver;
        private ICommClient _portClient;
        private SerialPort _uart;

        private byte _lastReadCommand = 0;

        #region Form

        public MasterForm() : this(null) { }
        
        public MasterForm(AppOptions options) 
            : base("Modbus scanner", "Modbus Master", options)
        {
            InitializeComponent();
        }

        private void MasterFormClosing(object sender, FormClosingEventArgs e)
        {
            DoDisconnect();
        }

        #endregion

        #region Connect/disconnect

        private void DoDisconnect()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket.Dispose();
                _socket = null;
            }
            if (_uart != null)
            {
                _uart.Close();
                _uart.Dispose();
                _uart = null;
            }
            _portClient = null;
            _driver = null;
        }

        private void BtnConnectClick(object sender, EventArgs e)
        {
            doConnect();
        }

        private bool doConnect()
        {
            try
            {
                switch (CommunicationMode)
                {
                    case CommunicationMode.RTU:
                        _uart = new SerialPort(PortName, Baud, Parity, DataBits, StopBits);
                        _uart.Open();
                        _portClient = _uart.GetClient();
                        _driver = new ModbusClient(new ModbusRtuCodec()) { Address = SlaveId };
                        _driver.OutgoingData += DriverOutgoingData;
                        _driver.IncommingData += DriverIncommingData;
                        AppendLog(String.Format("Connected using RTU to {0}", PortName));
                        break;

                    case CommunicationMode.UDP:
                        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        _socket.Connect(new IPEndPoint(IPAddress, TCPPort));
                        _portClient = _socket.GetClient();
                        _driver = new ModbusClient(new ModbusTcpCodec()) { Address = SlaveId };
                        _driver.OutgoingData += DriverOutgoingData;
                        _driver.IncommingData += DriverIncommingData;
                        AppendLog(String.Format("Connected using UDP to {0}", _socket.RemoteEndPoint));
                        break;

                    case CommunicationMode.TCP:
                        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                        _socket.SendTimeout = 2000;
                        _socket.ReceiveTimeout = 2000;
                        _socket.Connect(new IPEndPoint(IPAddress, TCPPort));
                        _portClient = _socket.GetClient();
                        _driver = new ModbusClient(new ModbusTcpCodec()) { Address = SlaveId };
                        _driver.OutgoingData += DriverOutgoingData;
                        _driver.IncommingData += DriverIncommingData;
                        AppendLog(String.Format("Connected using TCP to {0}", _socket.RemoteEndPoint));
                        break;
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message, passive:true);
                return false;
            }

            SetBusState(BusState.on);
            return true;
        }

        private void ButtonDisconnectClick(object sender, EventArgs e)
        {
            DoDisconnect();
            SetBusState(BusState.off);
        }

        protected override void StartCommunication()
        {
            BtnConnectClick(this, EventArgs.Empty);
        }

        protected override void DoSetBusState(BusState state)
        {
            base.DoSetBusState(state);

            if(state.HasFlag(BusState.on))
            {
                btnConnect.Enabled = false;
                buttonDisconnect.Enabled = true;
                groupBoxFunctions.Enabled = true;
            }
            else
            {
                btnConnect.Enabled = true;
                buttonDisconnect.Enabled = false;
                groupBoxFunctions.Enabled = false;
            }
        }

        #endregion

        #region processing

        private string formatAsError(CommResponse response, ModbusCommand command)
        {
            return $"[activity] {command.Caption()} status:{response.ErrorLabel()}";
        }

        private bool execute(ModbusCommand cmd, bool retry = true)
        {
            try
            {
                var result = _driver.ExecuteGeneric(_portClient, cmd);

                if (result.Status == CommResponse.Ack)
                {
                    AppendLog($"[activity] {cmd.Caption()} success");
                }
                else
                {
                    SetError(formatAsError(result, cmd));
                    return false; //no 2nd attempt
                }
            }
            catch (SocketException ex)
            {
                SetError(ex.Message, passive: true);

                if (ex.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    //could be an closed session (expiration). Try to recover
                    if (doConnect() == false)
                        return false;
                    else
                        return execute(cmd, retry:false); // 2nd and last attempt
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                return false; //no 2nd attempt
            }

            return true;
        }

        #endregion

        #region Functions buttons

        private void BtnReadCoilsClick(object sender, EventArgs e)
        {
            ExecuteReadCommand(ModbusCommand.FuncReadCoils);
        }

        private void BtnReadDisInpClick(object sender, EventArgs e)
        {
            ExecuteReadCommand(ModbusCommand.FuncReadInputDiscretes);
        }

        private void BtnReadHoldRegClick(object sender, EventArgs e)
        {
            ExecuteReadCommand(ModbusCommand.FuncReadMultipleRegisters);
        }

        private void BtnReadInpRegClick(object sender, EventArgs e)
        {
            ExecuteReadCommand(ModbusCommand.FuncReadInputRegisters);
        }

        private void ExecuteReadCommand(byte function)
        {
            _lastReadCommand = function;

            var command = new ModbusCommand(function) 
            { 
                Offset = StartAddress, 
                Count = DataLength,
                TransId = _transactionId++ 
            };

            if(execute(command))
                if (TryPutRegisters(StartAddress, command.Data, update: true) == false)
                    throw new InvalidOperationException("Invalid ModbusCommand read object");
        }

        private void ExecuteWriteCommand(byte function)
        {
            var command = new ModbusCommand(function)
            {
                Offset = StartAddress,
                Count = DataLength,
                TransId = _transactionId++,
                Data = new ushort[DataLength]
            };

            if (TryGetRegisters(StartAddress, command.Data, DataLength) == false)
                throw new InvalidOperationException("Invalid ModbusCommand write object");

            execute(command);
        }

        private void BtnWriteSingleCoilClick(object sender, EventArgs e)
        {
            var command = new ModbusCommand(ModbusCommand.FuncWriteCoil)
            {
                Offset = StartAddress,
                Count = 1,
                TransId = _transactionId++,
                Data = new ushort[1]
            };

            if (TryGetRegister(StartAddress, out command.Data[0]) == false)
                throw new InvalidOperationException("Invalid ModbusCommand write object");
            else
                command.Data[0] &= 0x0100;

            execute(command);
        }

        private void BtnWriteSingleRegClick(object sender, EventArgs e)
        {
            ExecuteWriteCommand(ModbusCommand.FuncWriteSingleRegister);
        }

        private void BtnWriteMultipleCoilsClick(object sender, EventArgs e)
        {
            ExecuteWriteCommand(ModbusCommand.FuncForceMultipleCoils);
        }

        private void BtnWriteMultipleRegClick(object sender, EventArgs e)
        {
            ExecuteWriteCommand(ModbusCommand.FuncWriteMultipleRegisters);
        }

        private void ButtonReadExceptionStatusClick(object sender, EventArgs e)
        {

        }

        #endregion

        private void txtPollDelay_Leave(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            if (int.TryParse(textBox.Text, out var parsedMillisecs))
            {
                pollTimer.Interval = parsedMillisecs;
            }
            else
            {
                textBox.Text = "0";
                cbPoll.Checked = false;
                pollTimer.Enabled = false;
            }

        }

        private void cbPoll_CheckStateChanged(object sender, EventArgs e)
        {
            pollTimer.Enabled = cbPoll.Checked;

            if (!pollTimer.Enabled)
                _lastReadCommand = 0;
        }

        private void pollTimer_Tick(object sender, EventArgs e)
        {
            if (_lastReadCommand != 0)
                ExecuteReadCommand(_lastReadCommand);
        }

        private void MasterForm_Load(object sender, EventArgs e)
        {

        }
    }
}
