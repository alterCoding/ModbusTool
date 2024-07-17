using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Modbus.Common;
using ModbusLib;
using ModbusLib.Protocols;

namespace ModbusSlave
{
    /// <summary>
    /// </summary>
    /// <remarks>Architecture is simple: <br/>
    /// - 1 single worker thread listens for input connections <br/>
    /// - each opened connection starts a single worker thread to process communication requests. This thread completes
    /// if session expires w/o any activity and connection is lost (See <see cref="ModbusLib.TcpServer"/>)
    /// </remarks>
    public partial class SlaveForm : BaseForm
    {
        private Function _function = Function.HoldingRegister;
        private ICommServer _listener;
        private SerialPort _uart;

        private Thread _thread;
        private volatile bool _cancel;

        #region Form
        
        /// <summary>
        /// stupid constructor to satisfy the win.forms designer
        /// </summary>
        public SlaveForm() : this(null) { }

        public SlaveForm(AppOptions options)
            : base("Modbus device", "Modbus Slave", options)
        {
            base.ShowDataLength = false;
            InitializeComponent();
        }

        private void SlaveFormClosing(object sender, FormClosingEventArgs e)
        {
            DoDisconnect();
        }

        private void SlaveFormLoading(object sender, EventArgs e)
        {
            //SlaveForm copes with 1+ worker threads in addition to the main/ui thread
            //concurrency topic remains focus around the user updates of the data registers table

            //each DataTab instance must be surrounded by the base class data register table thread safety policy
            //TabControl -> TabPage -> DataTab
            foreach(var p in tabControl1.TabPages)
            {
                if(p is TabPage page)
                    synchronize(page);
            }

            //we need to enroll the future DataTab instances too 
            tabControl1.ControlAdded += (_, c) => { if (c.Control is TabPage page) synchronize(page); };

            void synchronize(TabPage page)
            {
                foreach (var item in page.Controls)
                    if (item is DataTab tab)
                        Synchronize(tab);
            }
        }

        #endregion

        #region Connect/disconnect

        private void BtnConnectClick(object sender, EventArgs e)
        {
            _cancel = false;

            try
            {
                switch (CommunicationMode)
                {
                    case CommunicationMode.RTU:
                        _uart = new SerialPort(PortName, Baud, Parity, DataBits, StopBits);
                        _uart.Open();
                        var rtuServer = new ModbusServer(new ModbusRtuCodec()) { Address = SlaveId };
                        rtuServer.OutgoingData += DriverOutgoingData;
                        rtuServer.IncommingData += DriverIncommingData;
                        _listener = _uart.GetListener(rtuServer);
                        _listener.ServeCommand += listener_ServeCommand;
                        _listener.Start();
                        AppendLog(String.Format("Connected using RTU to {0}", PortName));
                        break;

                    case CommunicationMode.UDP:
                        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        _socket.Bind(new IPEndPoint(IPAddress.Any, TCPPort));
                        //create a server driver
                        var udpServer = new ModbusServer(new ModbusTcpCodec()) { Address = SlaveId };
                        udpServer.OutgoingData += DriverOutgoingData;
                        udpServer.IncommingData += DriverIncommingData;
                        //listen for an incoming request
                        _listener = _socket.GetUdpListener(udpServer);
                        _listener.ServeCommand += listener_ServeCommand;
                        _listener.Start();
                        AppendLog(String.Format("Listening to UDP port {0}", TCPPort));
                        break;

                    case CommunicationMode.TCP:
                        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        _socket.Bind(new IPEndPoint(IPAddress.Any, TCPPort));
                        _socket.Listen(10);
                        //create a server driver
                        _thread = new Thread(Worker);
                        _thread.Start();
                        AppendLog(String.Format("Listening to TCP port {0}", TCPPort));
                        break;
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message, passive:true);
                return;
            }

            SetBusState(BusState.on);
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
            }
            else
            {
                btnConnect.Enabled = true;
                buttonDisconnect.Enabled = false;
            }
        }

        /// <summary>
        /// Running thread handler
        /// </summary>
        protected void Worker()
        {
            var server = new ModbusServer(new ModbusTcpCodec()) { Address = SlaveId };
            server.IncommingData += DriverIncommingData;
            server.OutgoingData += DriverOutgoingData;
            try
            {
                while (!_cancel && _thread.ThreadState == ThreadState.Running)
                {
                    //wait for an incoming connection
                    _listener = _socket.GetTcpListener(server);
                    _listener.ServeCommand += listener_ServeCommand;
                    _listener.Start();
                    AppendLog(String.Format("Accepted connection."));
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                if(!_cancel) SetError(ex.Message, passive:true);
                else SetBusState(BusState.errorPassive);
            }
        }

        private void ButtonDisconnectClick(object sender, EventArgs e)
        {
            DoDisconnect();
        }

        private void DoDisconnect()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(DoDisconnect));
                return;
            }

            //here we go, old fashion
            _cancel = true;

            if (_listener != null)
            {
                _listener.Abort();
                _listener = null;
            }
            if (_uart != null)
            {
                _uart.Close();
                _uart.Dispose();
                _uart = null;
            }
            if (_thread != null && _thread.IsAlive)
            {
                if (_thread.Join(2000) == false)
                {
                    _thread.Abort();
                    _thread = null;
                }
            }
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            SetBusState(BusState.off);
        }

        #endregion

        #region Listen functions

        void listener_ServeCommand(object sender, ServeCommandEventArgs e)
        {
            var command = (ModbusCommand)e.Data.UserData;

            Thread.Sleep(SlaveDelay);

            //take the proper function command handler
            switch (command.FunctionCode)
            {
                case ModbusCommand.FuncReadCoils:
                case ModbusCommand.FuncReadInputDiscretes:
                case ModbusCommand.FuncReadInputRegisters:
                case ModbusCommand.FuncReadMultipleRegisters:
                case ModbusCommand.FuncReadCustom:
                    DoRead(command);
                    break;

                case ModbusCommand.FuncWriteCoil:
                case ModbusCommand.FuncForceMultipleCoils:
                case ModbusCommand.FuncWriteMultipleRegisters:
                case ModbusCommand.FuncWriteSingleRegister:
                    DoWrite(command);
                    break;
                default:
                    SetError($"[activity] Unsupported function: {command.FunctionCode}");
                    //return an exception
                    command.ExceptionCode = ModbusCommand.ErrorIllegalFunction;
                    break;
            }
        }

        private void DoRead(ModbusCommand command)
        {
            bool success;
            if (command.Count == 1)
                success = TryGetRegister((ushort)command.Offset, out command.Data[0]);
            else
                success = TryGetRegisters((ushort)command.Offset, command.Data, (ushort)command.Count);

            if(success == false)
                command.ExceptionCode = ModbusCommand.ErrorIllegalDataAddress;
            else
                AppendLog($"[activity.tx] {command.Caption()}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        private void DoWrite(ModbusCommand command)
        {
            var dataAddress = command.Offset;
            if (dataAddress < StartAddress || dataAddress > StartAddress + DataLength)
            {
                AppendLog(String.Format("Received address is not within viewable range, Received address:{0}.", dataAddress));
                command.ExceptionCode = ModbusCommand.ErrorSlaveDeviceBusy;
                return;
            }

            if(TryPutRegisters((ushort)dataAddress, command.Data, update: true) == false)
            {
                command.ExceptionCode = ModbusCommand.ErrorIllegalDataAddress;
                return;
            }

            AppendLog($"[activity.rx] {command.Caption()}");
        }

        #endregion

        #region Radion button check handlers

        private void RadioButtonFunctionCheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton)
            {
                var rb = (RadioButton)sender;
                if (rb.Checked)
                {
                    Function.TryParse(rb.Tag.ToString(), true, out _function);
                    ClearRegisterData();
                }
            }
        }

        #endregion

    }
}
