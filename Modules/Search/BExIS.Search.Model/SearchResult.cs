using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BExIS.Search.Model
{

    public abstract class SearchResultsBase
    {
        public HeaderItem Id { get; set; }
        public IEnumerable<HeaderItem> Header { get; set; }
        public IEnumerable<HeaderItem> DefaultVisibleHeaderItem { get; set; }
        public IEnumerable<Row> Rows { get; set; }

        public DataTable ConvertToDataTable()
        {
            DataTable searchResults = new DataTable();
            if (Header != null && Rows != null)
            {
                foreach (var colum in Header)
                {
                    DataColumn col = searchResults.Columns.Add(colum.Name); // or DisplayName also
                    if (colum.DisplayName == "")
                        col.Caption = colum.Name;
                    else
                        col.Caption = colum.DisplayName;


                    if (colum.Name.Equals(Id.Name))
                    {
                        col.DataType = System.Type.GetType("System.Int32");
                    }

                    //switch (colum.DataType)
                    //{
                    //    case "String":
                    //        {
                    //            System.Type.GetType("System.String");
                    //            break;
                    //        }
                    //    case "Integer":
                    //        {
                    //            System.Type.GetType("System.Int32");
                    //            break;
                    //        }

                    //    case "DateTime":
                    //        {
                    //            System.Type.GetType("System.DateTime");
                    //            break;
                    //        }
                    //    default:
                    //        {
                    //            System.Type.GetType("System.String");
                    //            break;
                    //        }
                    //}

                   
                }

                foreach (var row in Rows)
                {
                    searchResults.Rows.Add(row.Values.ToArray());
                }
            }
            return searchResults;
        }

    }

    public class HeaderItem
    {
        public string Name { get; set; } // good for property name matching when trying to dynamically show the rows in a grid
        public string DisplayName { get; set; } // proper for UI, showing grid columns and so on
        public string DataType { get; set; }
    }

    public class Row
    {
        //why IEnumerable???
        public IEnumerable<object> Values { get; set; }
        public SearchResultPreview PreviewItem { get; set; }
        // or I provide IEnumerable<Value> Values { get; set; }
        // and create another Property(s) with every type Int, decimal, ...
    }

    public class SearchResultPreview : SearchResultsBase
    {
        
    }

    public class SearchResult : SearchResultsBase
    {
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int NumberOfHits { get; set; }


        private int GetIndexOfID() {
                return Header.ToList().IndexOf(Id);

        }

        public int GetIndexOfRowByID(int id)
        {
            int headerId = GetIndexOfID();
            int index = 0;

            foreach (Row r in Rows)
            {
                if (r.Values.ToList().ElementAt(headerId).Equals(id)) return index;
                else index++;
            }

            return index;
        }

        public Row GetRowByID(int id)
        {
            int headerId = GetIndexOfID();
            foreach (Row r in Rows)
            {
                if (r.Values.ToList().ElementAt(headerId).Equals(id)) return r;
            }

            return null;
        }

        public SearchResultPreview GetPreviewByID(int id)
        {
            int headerId = GetIndexOfID();
            foreach (Row r in Rows)
            {
                if (r.Values.ToList().ElementAt(headerId).Equals(id)) return r.PreviewItem;
            }

            return null;
        }

    }

    
}
