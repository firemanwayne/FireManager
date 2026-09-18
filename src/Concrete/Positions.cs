using System.Xml.Serialization;

namespace FireManager.Concrete
{
    public class Positions
    {
        [XmlElement(ElementName = "position")]
        public Position[] Position { get; set; } = System.Array.Empty<Position>();
    }
}