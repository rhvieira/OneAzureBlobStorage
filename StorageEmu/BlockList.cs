using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OneAzureStorageFS
{
    [XmlRoot(ElementName = "BlockList")]
    public class BlockList
    {
        [XmlElement(ElementName = "Latest")]
        public List<string> Latest { get; set; }
    }
}
