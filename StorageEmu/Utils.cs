using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OneAzureStorageFS
{
   public static class Utils
    {
       public static T Deserialize<T>(Stream xmlv)
       {
           XmlSerializer serializer = new XmlSerializer(typeof(T));
           return (T)serializer.Deserialize(xmlv);
       }
        public static string Serialize<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var streamWriter = new StreamWriter(memoryStream, System.Text.Encoding.UTF8))
                        {
                            serializer.Serialize(streamWriter, value);
                            return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                        }
                    }
                

            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }
    }
}
