using System.Xml.Serialization;

namespace FireManager.Concrete
{
    public class ResultsRanges
    {
        [XmlElement(ElementName = "range")]
        public ResultRange[] Range { get; set; } = System.Array.Empty<ResultRange>();
    }
}
