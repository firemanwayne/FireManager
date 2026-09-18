using System.Xml.Serialization;

namespace FireManager.Concrete
{
    public class Members
    {
        [XmlElement(ElementName = "member")]
        public Member[] Member { get; set; } = System.Array.Empty<Member>();
    }
}
