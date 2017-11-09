using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Geonorge.Kartografi.Resources;

namespace Geonorge.Kartografi.Formatter
{
    public class CsvFormatter : BufferedMediaTypeFormatter
    {
        private readonly string csv = "text/csv";

        public CsvFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(csv));
            MediaTypeMappings.Add(new QueryStringMapping("mediatype", "csv", csv));
            MediaTypeMappings.Add(new UriPathExtensionMapping("csv", csv));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            SupportedEncodings.Add(Encoding.GetEncoding("iso-8859-1"));
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            if (CanWriteType(type) && mediaType.MediaType == csv)
            {
                headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                headers.ContentDisposition.FileName = "kartografi.csv";
            }
            else
            {
                base.SetDefaultContentHeaders(type, headers, mediaType);
            }
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
            Encoding effectiveEncoding = SelectCharacterEncoding(content.Headers);

            if (type == typeof(Models.Api.Cartography) ||
                type == typeof(List<Models.Api.Cartography>))
                BuildCSV(value, writeStream, effectiveEncoding);
        }

        private void BuildCSV(object models, Stream stream, Encoding effectiveEncoding)
        {
            StreamWriter streamWriter = new StreamWriter(stream, effectiveEncoding);

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
            string text = $"{item.Uuid};{item.Name};{item.FileUrl};{item.Owner};{item.Theme};{(item.OfficialStatus ? UI.Yes : UI.No)};{item.Format};{item.DatasetUuid};{item.DatasetName};{item.OwnerDataset}";
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
                text = UI.NotSet;
            }

            return text;
        }


        private string RegisterHeading()
        {
            return $"Uuid;{UI.Name};FileUrl;{UI.Owner};{UI.Theme};{UI.Official};Format;DatasetUuid;{UI.DatasetName};{UI.OwnerDataset}";
        }
        
    }
}