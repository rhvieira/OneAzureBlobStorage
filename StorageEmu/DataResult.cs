using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OneAzureStorageFS
{
    [XmlRoot(ElementName = "Properties")]
    public class PropertiesBlob
    {
        [XmlElement(ElementName = "Last-Modified")]
        public DateTime LastModified { get; set; }
        [XmlElement(ElementName = "Etag")]
        public string Etag { get; set; }
        [XmlElement(ElementName = "Content-Length")]
        public long ContentLength { get; set; }
        [XmlElement(ElementName = "Content-Type")]
        public string ContentType { get; set; }
        [XmlElement(ElementName = "Content-Encoding")]
        public string ContentEncoding { get; set; }
        [XmlElement(ElementName = "Content-Language")]
        public string ContentLanguage { get; set; }
        [XmlElement(ElementName = "Content-MD5")]
        public string ContentMD5 { get; set; }
        [XmlElement(ElementName = "Cache-Control")]
        public string CacheControl { get; set; }
        [XmlElement(ElementName = "Content-Disposition")]
        public string ContentDisposition { get; set; }
        [XmlElement(ElementName = "BlobType")]
        public string BlobType { get; set; }
        [XmlElement(ElementName = "LeaseStatus")]
        public string LeaseStatus { get; set; }
        [XmlElement(ElementName = "LeaseState")]
        public string LeaseState { get; set; }
    }



    [XmlRoot(ElementName = "Properties")]
    public class PropertiesContainer
    {
        [XmlElement(ElementName = "Last-Modified")]
        public DateTime LastModified { get; set; }
        [XmlElement(ElementName = "Etag")]
        public string Etag { get; set; }
       
        [XmlElement(ElementName = "LeaseStatus")]
        public string LeaseStatus { get; set; }
        [XmlElement(ElementName = "LeaseState")]
        public string LeaseState { get; set; }
    }

    [XmlRoot(ElementName = "Blob")]
    public class Blob
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Properties")]
        public PropertiesBlob Properties { get; set; }
        [XmlElement(ElementName = "Metadata")]
        public string Metadata { get; set; }
    }

    [XmlRoot(ElementName = "Container")]
    public class Container
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Properties")]
        public PropertiesContainer Properties { get; set; }
     
    }
    [XmlRoot(ElementName = "Containers")]
    public class Containers
    {
        [XmlElement(ElementName = "Container")]
        public List<Container> Container { get; set; }
    }

    [XmlRoot(ElementName = "Blobs")]
    public class Blobs
    {
        [XmlElement(ElementName = "Blob")]
        public List<Blob> Blob { get; set; }
    }

    [XmlRoot(ElementName = "EnumerationResults")]
    public class EnumerationResults
    {
        [XmlElement(ElementName = "Blobs")]
        public Blobs Blobs { get; set; }
        [XmlElement(ElementName = "NextMarker")]
        public string NextMarker { get; set; }
        [XmlAttribute(AttributeName = "ServiceEndpoint")]
        public string ServiceEndpoint { get; set; }
        [XmlAttribute(AttributeName = "ContainerName")]
        public string ContainerName { get; set; }

        [XmlElement(ElementName = "Containers")]
        public Containers Containers { get; set; }
    }
}
