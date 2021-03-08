using System;

namespace C9FileHelpers
{
    [Serializable]
    public class ExTimeStamp
    {
        public ExTimeStamp(DateTime when)
        {
            Time = when;
            Iso8601 = when.ToUniversalTime().ToString("yyyyMMddThhMMss.fffZ");
            Raw = when.Ticks;
        }

        public DateTime Time
        {
            get;
        }

        public string Iso8601
        {
            get;
        }

        public long Raw
        {
            get;
        }
    }
}
