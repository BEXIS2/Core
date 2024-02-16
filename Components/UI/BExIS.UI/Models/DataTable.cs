using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace BExIS.UI.Models
{
    public class DataTableSendModel
    {
        public long Id { get; set; }
        public long Version { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public List<DataTableFilter> Filter { get; set; }
        public List<DataTableOrderBy> OrderBy { get; set; }


        public DataTableSendModel() {
            Id = 0;
            Version=0;
            Offset = 0; 
            Limit = 100;
            Filter = new List<DataTableFilter>();
            OrderBy = new List<DataTableOrderBy>();
        }
    }

    public class DataTableRecieveModel
    {
        public int Count { get; set; }
        public DataTable Data { get; set; }

        public DataTableColumn Columns  { get; set; }

        public DataTableSendModel SendModel { get; set; }

        public DataTableRecieveModel()
        {
            Count = 0;
            Data = new DataTable();
            SendModel = new DataTableSendModel();
            Columns = new DataTableColumn();
        }
    }



    public class DataTableColumn
    {
        public string Header { get; set; }// key by default

        public Boolean exclude { get; set; }// false by default

        public DataTableInstruction Instructions { get; set; }
    }

    public class DataTableInstruction
    {
        public Dictionary<object,string> MissingValues { get; set; }
        public string  DisplayPattern { get; set; }


    }

    public class DataTableFilter
    {
        public string Column { get; set; }
        public DataTableFilterType FilterBy { get; set; }
        public string Value { get; set; }
    }

    public class DataTableOrderBy
    {
        public string Column { get; set; }
        public DataTableOrderType Direction { get; set; }
    }

    public enum DataTableFilterType
    {
	    ie, // Is equal to
	    ne, // Is not equal to
	    gt, // Greater than
	    lt, // Less than
	    gte, // Greater than or equal to
	    lte, // Less than or equal to
	    c, // Contains
	    nc, // Does not contain
	    sw, // Starts with
	    ew // Ends with
    }

    public enum DataTableOrderType
    {
        asc,
        desc
    }
}
