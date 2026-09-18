using System.Xml.Serialization;

namespace FireManager.Concrete
{
    public class Attributes
    {
        [XmlElement(ElementName = "attribute")]
        public Attribute[] Attribute { get; set; } = System.Array.Empty<Attribute>();
    }
}
