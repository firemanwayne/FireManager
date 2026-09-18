using System.Xml.Serialization;

namespace FireManager.Concrete
{
    public class Schedule
    {
        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlElement(ElementName = "name")]
        public Name Name { get; set; }

        [XmlElement(ElementName = "positions")]
        public Positions[] Positions { get; set; } = System.Array.Empty<Positions>();
    }
}
