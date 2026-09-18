using System;

namespace FireManager.Extensions
{
    public class FireManagerOptions
    {
        private string url;
        private string acckey;
        private string accid;

        public string Accid
        {
            get
            {
                if (string.IsNullOrEmpty(accid))
                    throw new NullReferenceException("Account Id cannot be null");
                else
                    return accid;
            }
            private set
            {
                accid = value;
            }
        }
        public string AccKey
        {
            get
            {
                if (string.IsNullOrEmpty(acckey))
                    throw new NullReferenceException("Account Key cannot be null");
                else
                    return acckey;
            }
            private set
            {
                acckey = value;
            }
        }
        public string Url
        {
            get
            {
                if (string.IsNullOrEmpty(url))
                    throw new NullReferenceException("Account Url cannot be null");
                else
                    return url;
            }
            private set
            {
                url = value;
            }
        }

        // Explicit department timezone; never infer it from the application host.
        public TimeZoneInfo DepartmentTimeZone { get; set; } = GetCentralTimeZone();

        public TimeSpan ShiftStart { get; set; } = TimeSpan.FromHours(5);

        private static TimeZoneInfo GetCentralTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            }
        }

        public bool RunTests { get; set; }

        public void AccountId(string Accid)
        {
            this.Accid = Accid;
        }
        public void AccountKey(string AccKey)
        {
            this.AccKey = AccKey;
        }
        public void AccountUrl(string Url)
        {
            this.Url = Url;
        }
    }
}
