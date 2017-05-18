using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Geonorge.Kartografi.Formatter
{
    public class CsvFormatter : BufferedMediaTypeFormatter
    {
        private readonly string csv = "text/csv";

        public CsvFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(csv));
            MediaTypeMappings.Add(new UriPathExtensionMapping("csv", csv));
        }

        Func<Type, bool> SupportedTypeCSV = (type) =>
        {
            if (type == typeof(Models.Api.Cartography) ||
                type == typeof(List<Models.Api.Cartography>) )
                return true;
            else
                return false;
        };

        public override bool CanWriteType(System.Type type)
        {
            return SupportedTypeCSV(type);
        }
        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            if (type == typeof(Models.Api.Cartography) ||
                type == typeof(List<Models.Api.Cartography>))
                BuildCSV(value, writeStream, content.Headers.ContentType.MediaType);
        }

        private void BuildCSV(object models, Stream stream, string contenttype)
        {
            StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);

            if (models is List<Models.Api.Cartography>)
            {
                streamWriter.WriteLine(RegisterHeading());
                var regItems = models as List<Models.Api.Cartography>;
                foreach (var item in regItems.OrderBy(r => r.DatasetName))
                {
                    ConvertRegisterToCSV(streamWriter, item);
                }
            }
            streamWriter.Close();
        }



        private static void ConvertRegisterToCSV(StreamWriter streamWriter, Models.Api.Cartography item)
        {
            string text = $"{item.Uuid};{item.Name}";
            streamWriter.WriteLine(text);
        }

        private static string RemoveBreaksFromText(string text)
        {
            string replaceWith = " ";
            if (!string.IsNullOrWhiteSpace(text))
            {
                text = text.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
            }
            else
            {
                text = "ikke angitt";
            }

            return text;
        }


        private string RegisterHeading()
        {
            return "Uuid;Name";
        }
        
    }
}