using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BExIS.UI.Models
{

    public class DataTableSendModel
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public Object data { get; set; }
        public string error { get; set; }

        public DataTableSendModel()
        {
            draw = 0;
            recordsTotal = 0;
            recordsFiltered = 0;
            data = new Object();
            error = "";
        }
    }

    public sealed class DataTablesColumnsModel
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Orderable { get; set; }
        public DataTablesSearchModel Search { get; set; }
        public bool Searchable { get; set; }
    }

    public sealed class DataTablesOrderModel
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }

    [ModelBinder(typeof(DataTablesParametersModelBinder))]
    public class DataTableRecieverModel
    {
        public List<DataTablesColumnsModel> Columns { get; set; }
        public int Draw { get; set; }
        public int Length { get; set; }
        public List<DataTablesOrderModel> Order { get; set; }
        public DataTablesSearchModel Search { get; set; }
        public int Start { get; set; }
    }

    public class DataTablesParametersModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            base.BindModel(controllerContext, bindingContext);
            var request = controllerContext.HttpContext.Request;
            // Retrieve request data
            var draw = Convert.ToInt32(request["draw"]);
            var start = Convert.ToInt32(request["start"]);
            var length = Convert.ToInt32(request["length"]);
            // Search
            var search = new DataTablesSearchModel()
            {
                Value = request["search[value]"],
                Regex = Convert.ToBoolean(request["search[regex]"])
            };
            // Order
            var o = 0;
            var order = new List<DataTablesOrderModel>();
            while (request["order[" + o + "][column]"] != null)
            {
                order.Add(new DataTablesOrderModel()
                {
                    Column = Convert.ToInt32(request["order[" + o + "][column]"]),
                    Dir = request["order[" + o + "][dir]"]
                });
                o++;
            }
            // Columns
            var c = 0;
            var columns = new List<DataTablesColumnsModel>();
            while (request["columns[" + c + "][name]"] != null)
            {
                columns.Add(new DataTablesColumnsModel()
                {
                    Data = request["columns[" + c + "][data]"],
                    Name = request["columns[" + c + "][name]"],
                    Orderable = Convert.ToBoolean(request["columns[" + c + "][orderable]"]),
                    Searchable = Convert.ToBoolean(request["columns[" + c + "][searchable]"]),
                    Search = new DataTablesSearchModel()
                    {
                        Value = request["columns[" + c + "][search][value]"],
                        Regex = Convert.ToBoolean(request["columns[" + c + "][search][regex]"])
                    }
                });
                c++;
            }

            return new DataTableRecieverModel()
            {
                Draw = draw,
                Start = start,
                Length = length,
                Search = search,
                Order = order,
                Columns = columns
            };
        }
    }

    public sealed class DataTablesSearchModel
    {
        public bool Regex { get; set; }
        public string Value { get; set; }
    }

}
