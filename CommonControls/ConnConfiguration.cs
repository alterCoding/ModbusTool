using System;
using System.Net;
using System.Xml.Serialization;
using System.IO.Ports;

namespace Modbus.Common
{
    /// <summary>
    /// Base configuration for tcp/udp, to be used with xml serialization
    /// </summary>
    public abstract class ModbusIPConfiguration : IModbusConfiguration
    {
        public enum Transport { TCP, UDP }

        [XmlIgnore]
        public abstract Transport ModbusType { get; }
     
        public string address
        {
            get => m_ipAddress.ToString();

            set => m_ipAddress = string.IsNullOrWhiteSpace(value) ? IPAddress.Loopback : IPAddress.Parse(value);
        }

        public int port
        {
            get => Port;

            set => Port = (ushort)(value >= IPEndPoint.MinPort && value <= IPEndPoint.MaxPort ? value
                : throw new ArgumentException($"{value} is not a valid ip port"));
        }

        [XmlIgnore]
        public IPEndPoint EndPoint
        {
            get
            {
                return new IPEndPoint(m_ipAddress, Port);
            }
            set
            {
                m_ipAddress = new IPAddress(value.Address.GetAddressBytes());
                Port = (ushort)value.Port;
            }
        }

        [XmlIgnore]
        public IPAddress Address
        {
            get => m_ipAddress;
            set => m_ipAddress = new IPAddress(value.GetAddressBytes()) ?? IPAddress.Loopback;
        }

        [XmlIgnore]
        public ushort Port { get; set; } = 502;

        public override string ToString() => EndPoint.ToString();

        CommunicationMode IModbusConfiguration.ComType => ModbusType == Transport.TCP ? CommunicationMode.TCP : CommunicationMode.UDP;

        private IPAddress m_ipAddress = IPAddress.Loopback;
    }

    /// <summary>
    /// Slave TCP configuration target endpoint. To be used with xml serialization
    /// </summary>
    public class ModbusTCPConfiguration : ModbusIPConfiguration
    {
        public override Transport ModbusType => Transport.TCP;
    }
    /// <summary>
    /// Slave UDP configuration target endpoint. To be used with xml serialization
    /// </summary>
    public class ModbusUDPConfiguration : ModbusIPConfiguration
    {
        public override Transport ModbusType => Transport.UDP;
    }

    public interface IModbusConfiguration
    {
        CommunicationMode ComType { get; }
    }

    /// <summary>
    /// Slave RTU configuration target endpoint. To be used with xml serialization
    /// </summary>
    public class ModbusRTUConfiguration : IModbusConfiguration
    {
        [XmlElement(ElementName = "portName")]
        public string PortName
        {
            get => m_portName;

            set => m_portName = string.IsNullOrWhiteSpace(value) ? "COM0"
                : value.ToUpper().StartsWith("COM") ? value
                : throw new ArgumentException($"{value} is not an expected value for a serial port name");
        }

        [XmlElement(ElementName = "parity")]
        public Parity Parity { get; set; }

        public float stopBits
        {
            get => (float)(StopBits == StopBits.None ? 0 : StopBits == StopBits.One ? 1 : StopBits == StopBits.Two ? 2 : StopBits == StopBits.OnePointFive ? 1.5 : 0);

            set => StopBits = value == 0 ? StopBits.None : value == 1 ? StopBits.One : value == 1.5 ? StopBits.OnePointFive
                : throw new ArgumentException($"{value} is not a valid Stop-Bits specification");
        }

        public int dataBits
        {
            get => DataBits;
            set => DataBits = (byte)value;
        }

        [XmlIgnore]
        public byte DataBits 
        {
            get => m_dataBits;

            set => m_dataBits = value >= 5 && value <= 8 ? value 
                : throw new ArgumentException($"{value} is not a valid Data-Bits specification. Expecting [5-8]");
        }

        [XmlIgnore]
        public StopBits StopBits { get; set; }

        [XmlElement(ElementName = "baudRate")]
        public int BaudRate
        {
            get => m_baudRate;

            set => m_baudRate = value > 0 && value <= 115200 ? value
                : throw new ArgumentException($"{value} is not an expected value for modbus-rtu baud rate");
        }

        public override string ToString() => PortName;

        CommunicationMode IModbusConfiguration.ComType => CommunicationMode.RTU;

        private string m_portName = "COM0";
        private int m_baudRate = 9600;
        private byte m_dataBits = 8;
    }

    /// <summary>
    /// Target endpoint configuration to be used with xml serialization
    /// </summary>
    public class ModbusConnConfiguration
    {
        public ModbusConnConfiguration() { }

        public int slaveID
        {
            get => SlaveID;

            set => SlaveID = (byte)(value >= 0 && value <= byte.MaxValue ? value 
                : throw new ArgumentException($"{value} is not a valid device modbus slave id"));
        }

        [XmlIgnore]
        public byte SlaveID { get; set; }

        [XmlElement(ElementName = "modbus")]
        public CommunicationMode ModbusMode { get; set; }

        [XmlChoiceIdentifier(nameof(ModbusMode))]
        [XmlElement(nameof(CommunicationMode.TCP), typeof(ModbusTCPConfiguration))]
        [XmlElement(nameof(CommunicationMode.RTU), typeof(ModbusRTUConfiguration))]
        [XmlElement(nameof(CommunicationMode.UDP), typeof(ModbusUDPConfiguration))]
        public object Parameters { get; set; }

        public IModbusConfiguration EndPoint => Parameters as IModbusConfiguration;

        public T TryCast<T>() where T : IModbusConfiguration
        {
            if (Parameters is T conf) return conf;
            else throw new InvalidCastException($"Expecting a configuration of type {typeof(T)} but was {Parameters.GetType()}");
        }

        public static ModbusConnConfiguration Create(IModbusConfiguration conf, byte slaveID)
        {
            var instance = new ModbusConnConfiguration() { SlaveID = slaveID };
            instance.Configure(conf);
            return instance;
        }

        public void Configure(IModbusConfiguration conf)
        {
            Parameters = conf;
            ModbusMode = conf.ComType;
        }

        public override string ToString()
        {
            return $"({ModbusMode.ToString()}) slave:{SlaveID} {EndPoint.ToString()}";
        }
    }
}
