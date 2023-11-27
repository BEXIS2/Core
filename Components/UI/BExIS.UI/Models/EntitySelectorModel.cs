using System.Collections.Generic;
using System.Data;

namespace BExIS.UI.Models
{
    public class EntitySelectorModel
    {
        public string Title { get; set; }

        public string TargetId { get; set; }

        public DataTable Data { get; set; }

        public EntitySelectorModelAction DataSource { get; set; }
        public EntitySelectorModelAction Reciever { get; set; }

        public string IDKey { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public EntitySelectorModel()
        {
            Title = "";
            Data = new DataTable();
            DataSource = new EntitySelectorModelAction();
            Reciever = new EntitySelectorModelAction();
            IDKey = "";
            Parameters = new Dictionary<string, string>();
        }
    }

    public class EntitySelectorModelAction
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Area { get; set; }

        public EntitySelectorModelAction()
        {
            Action = "";
            Controller = "";
            Area = "";
        }

        public EntitySelectorModelAction(string action, string controller, string area)
        {
            Action = action;
            Controller = controller;
            Area = area;
        }
    }
}