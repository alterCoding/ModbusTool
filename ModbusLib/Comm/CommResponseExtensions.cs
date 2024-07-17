using System;

namespace ModbusLib
{
    public static class CommResponseExtensions
    {
        public static string ErrorLabel(this CommResponse resp)
        {
            if (resp.Status == CommResponse.Unknown) return "<?>";
            else if (resp.Status == CommResponse.Ack) return "DELIVERED";
            else if (resp.Status == CommResponse.Critical) return "KO";
            else if (resp.Status == CommResponse.Ignore) return "LOST";
            else return string.Empty;
        }
    }
}
