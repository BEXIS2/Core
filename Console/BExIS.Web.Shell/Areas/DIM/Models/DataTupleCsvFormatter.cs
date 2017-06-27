using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Data;
using System.Net.Http;

namespace BExIS.Modules.Dim.UI.Models.Formatters
{
    // used: http://www.asp.net/web-api/overview/formats-and-model-binding/media-formatters
    // see: App_Start folder -> WebApiConfi.cs
    public class DatasetModelCsvFormatter: BufferedMediaTypeFormatter
    {
        public DatasetModelCsvFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            SupportedEncodings.Add(Encoding.GetEncoding("iso-8859-1"));
        }

        public DatasetModelCsvFormatter(MediaTypeMapping mediaTypeMapping)
        : this()
        {
            MediaTypeMappings.Add(mediaTypeMapping);
        }

        private string fileName = "dataset";
        public DatasetModelCsvFormatter(string fileName)
        : this()
        {
            this.fileName = fileName;
        }

        public override void SetDefaultContentHeaders(Type type,
            HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            headers.ContentDisposition.FileName = fileName + ".csv";
        }

        public override Boolean CanReadType(Type type)
        {
            return false;
        }

        public override Boolean CanWriteType(Type type)
        {
            if (type == typeof(DatasetModel))
            {
                return true;
            }
            //else
            //{
            //    Type enumerableType = typeof(IEnumerable<AbstractTuple>);
            //    return enumerableType.IsAssignableFrom(type);
            //}
            return false;
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            Encoding effectiveEncoding = SelectCharacterEncoding(content.Headers);
            DatasetModel model = (DatasetModel)value;
            DataTable table = model.DataTable;
            content.Headers.ContentDisposition.FileName = table.TableName + ".csv";

            using (var writer = new StreamWriter(writeStream, effectiveEncoding))
            {
                // write the header line
                writeHeader(table.Columns, writer);

                foreach (DataRow row in table.Rows)
                {
                    writeItem(row, writer);
                }
            }
        }

        private void writeHeader(DataColumnCollection columns, StreamWriter writer)
        {
            string columnStr = "";
            List<string> cols = new List<string>();
            foreach (var item in columns)
            {
                cols.Add(item.ToString());
            }
            columnStr = string.Join(",", cols);
            writer.WriteLine(columnStr);
        }

        private void writeItem(DataRow row, StreamWriter writer)
        {
            string rowStr = "";
            List<string> values = new List<string>();
            foreach (DataColumn column in row.Table.Columns)
            {
                values.Add(escape(row[column].ToString()));
            }
            rowStr = string.Join(",", values);
            writer.WriteLine(rowStr);
        }

        private static char[] specialChars = new char[] { ',', '\n', '\r', '"' };

        private string escape(object o)
        {
            if (o == null)
            {
                return "";
            }
            string field = o.ToString();
            if (field.IndexOfAny(specialChars) != -1)
            {
                // Delimit the entire field with quotes and replace embedded quotes with "".
                return String.Format("\"{0}\"", field.Replace("\"", "\"\""));
            }
            else return field;
        }
    }
}