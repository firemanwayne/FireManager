using System;
using System.Xml.Serialization;

namespace FireManager.Concrete
{
    public class ResultRange
    {
        [XmlElement(ElementName = "schedule")]
        public Schedule Schedule { get; set; }

        [XmlElement(ElementName = "position")]
        public Position Position { get; set; }

        [XmlElement(ElementName = "timetype")]
        public TimeType TimeType { get; set; }

        [XmlElement(ElementName = "member")]
        public Member Member { get; set; }

        [XmlElement("begin")]
        public DateTime Begin
        {
            get { return begin; }
            set { begin = AsUtc(value); }
        }

        [XmlElement("end")]
        public DateTime End
        {
            get { return end; }
            set { end = AsUtc(value); }
        }

        private static DateTime AsUtc(DateTime value) => value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();

        private DateTime begin;
        private DateTime end;
    }
}
