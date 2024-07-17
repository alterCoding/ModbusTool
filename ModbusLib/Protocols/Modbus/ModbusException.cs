using System;

namespace ModbusLib.Protocols
{
    namespace Modbus
    {
        public enum ExceptionCode
        {
            _undefined = 0,

            illegalFunction    = 1,
            illegalDataAddress = 2,
            illegalDataValue   = 3,
            slaveDeviceFailure = 4,
            ack                = 5,
            slaveDeviceBusy    = 6,
            nack               = 7,
            memParityError     = 8,

            _next
        }
    }

    public static class ModbusException
    {
        public static Modbus.ExceptionCode ToExceptionCode(int value)
        {
            if (value > 0 && value < (int)Modbus.ExceptionCode._next)
                return (Modbus.ExceptionCode)value;
            else
                return Modbus.ExceptionCode._undefined;
        }
    }
}
