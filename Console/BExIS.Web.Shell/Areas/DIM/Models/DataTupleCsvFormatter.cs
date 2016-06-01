using BExIS.Dlm.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Text;

namespace BExIS.Web.Shell.Areas.DIM.Models.Formatters
{
    // used: http://www.asp.net/web-api/overview/formats-and-model-binding/media-formatters
    // see: App_Start folder 0> WebApiConfi.cs
    public class DataTupleCsvFormatter: BufferedMediaTypeFormatter
    {
        public DataTupleCsvFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            SupportedEncodings.Add(Encoding.GetEncoding("iso-8859-1"));
        }

        public override Boolean CanReadType(Type type)
        {
            return false;
        }

        public override Boolean CanWriteType(Type type)
        {
            if (type == typeof(AbstractTuple))
            {
                return true;
            }
            else
            {
                Type enumerableType = typeof(IEnumerable<AbstractTuple>);
                return enumerableType.IsAssignableFrom(type);
            }
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            Encoding effectiveEncoding = SelectCharacterEncoding(content.Headers);

            using (var writer = new StreamWriter(writeStream, effectiveEncoding))
            {
                var tuples = value as IEnumerable<AbstractTuple>;
                // think of projection!?
                List<string> columns = getProjection(tuples, content);
                // write the header line
                writeHeader(columns, writer);

                if (tuples != null)
                {
                    foreach (var tuple in tuples)
                    {
                        writeItem(tuple, writer);
                    }
                }
                else
                {
                    var singleTuple = value as AbstractTuple;
                    if (singleTuple == null)
                    {
                        throw new InvalidOperationException("Cannot serialize type");
                    }
                    writeItem(singleTuple, writer);
                }
            }
        }

        private void writeHeader(List<string> columns, StreamWriter writer)
        {
            throw new NotImplementedException();
        }

        private List<string> getProjection(IEnumerable<AbstractTuple> tuples, HttpContent content)
        {
            // the projection may have been passed through the http context
            throw new NotImplementedException();
        }

        private void writeItem(AbstractTuple tuple, StreamWriter writer)
        {
            writer.WriteLine("{0},{1},{2},{3}", escape(tuple.Id),
                escape(tuple.VersionNo), escape(tuple.OrderNo), escape(tuple.TupleType)); // sample code only. change it freely.
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