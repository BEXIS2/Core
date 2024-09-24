using System.Collections.Generic;
using System.Data;
using System.Linq;

/// <summary>
///
/// </summary>
namespace BExIS.Utils.Models
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class HeaderItem
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string DataType { get; set; }

        /// <summary>
        /// proper for UI, showing grid columns and so on
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string DisplayName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks> good for property name matching when trying to dynamically show the rows in a grid </remarks>
        /// <seealso cref=""/>
        public string Name { get; set; }

        /// <summary>
        /// PlaceHolder as string
        /// </summary>
        public string Placeholder { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class Row
    {
        public SearchResultPreview PreviewItem { get; set; }

        //why IEnumerable???
        public IEnumerable<object> Values { get; set; }

        // or I provide IEnumerable<Value> Values { get; set; }
        // and create another Property(s) with every type Int, decimal, ...
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class SearchResult : SearchResultsBase
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public int CurrentPage { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public int NumberOfHits { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public int PageSize { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        /// <returns></returns>
        public SearchResultPreview GetPreviewByID(int id)
        {
            int headerId = GetIndexOfID();
            foreach (Row r in Rows)
            {
                if (r.Values.ToList().ElementAt(headerId).Equals(id)) return r.PreviewItem;
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        /// <returns></returns>
        public Row GetRowByID(int id)
        {
            int headerId = GetIndexOfID();
            foreach (Row r in Rows)
            {
                if (r.Values.ToList().ElementAt(headerId).Equals(id)) return r;
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        private int GetIndexOfID()
        {
            return Header.ToList().IndexOf(Id);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class SearchResultPreview : SearchResultsBase
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public abstract class SearchResultsBase
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public IEnumerable<HeaderItem> DefaultVisibleHeaderItem { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public IEnumerable<HeaderItem> Header { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public HeaderItem Id { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public IEnumerable<Row> Rows { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public DataTable ConvertToDataTable()
        {
            DataTable searchResults = new DataTable();
            if (Header != null && Rows != null)
            {
                // check for duplicates in header name; add "( no.)" to make names unique
                foreach (HeaderItem hi in Header)
                {
                    if (Header.Where(p => p.Name.Equals(hi.Name)).Count() > 1)
                    {
                        List<HeaderItem> list = Header.Where(p => p.Name.Equals(hi.Name)).ToList();
                        for (int i = 0; i < list.Count(); i++)
                        {
                            if (i > 0)
                            {
                                list.ElementAt(i).Name += i;
                                list.ElementAt(i).DisplayName += " (" + i + ")";
                            }
                        }
                    }
                }
                foreach (var colum in Header)
                {
                    if (!searchResults.Columns.Contains(colum.Name)) // when the search provider is reloaded, it contains duplicate items
                    {
                        DataColumn col = searchResults.Columns.Add(colum.Name);  // or DisplayName also
                        if (colum.DisplayName == "")
                            col.Caption = colum.Name;
                        else
                            col.Caption = colum.DisplayName;
                        if (colum.Name.Equals(Id.Name))
                        {
                            col.DataType = System.Type.GetType("System.Int64");
                        }
                    }
                }
                foreach (var row in Rows)
                {
                    searchResults.Rows.Add(row.Values.ToArray());
                }
            }

            // sort search result by ID (descending) as datasets from the index returned in the order of last edit (last re-index followed by new created or updated datasets)
            var l = Id.DisplayName;
            searchResults.DefaultView.Sort = l + " Desc";
            return searchResults.DefaultView.ToTable();
        }
    }
}