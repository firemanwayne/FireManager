using System;
using System.Xml.Serialization;

namespace FireManager.Concrete
{
    [Serializable]
    [XmlType("results")]
    public class Results
    {
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }

        [XmlAttribute(AttributeName = "accessTime")]
        public string AccessTime { get; set; }

        [XmlElement(ElementName = "members")]
        public Members Members { get; set; }

        [XmlElement(ElementName = "schedules")]
        public Schedules Schedules { get; set; }

        [XmlElement(ElementName = "ranges")]
        public ResultsRanges ResultsRanges { get; set; }

        [XmlElement(ElementName = "error")]
        public Error Error { get; set; }

        [XmlElement(ElementName = "authentication")]
        public Authentication Authentication { get; set; }
    }
}
