using System.Xml.Serialization;

namespace FireManager.Concrete
{
    public class Schedules
    {
        [XmlElement(ElementName = "schedule")]
        public Schedule[] Schedule { get; set; } = System.Array.Empty<Schedule>();
    }
}