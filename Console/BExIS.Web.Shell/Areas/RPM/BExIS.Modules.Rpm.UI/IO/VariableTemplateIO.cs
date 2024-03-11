using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace BExIS.Modules.Rpm.UI.Classes
{
    public static class VariableTemplateIO
    {
        public static VariableTemplate getVariableTemplate(long variableTemplateId)
        {
            if (variableTemplateId > 0)
            {
                VariableManager variableManager = null;
                try
                {
                    variableManager = new VariableManager();
                    return variableManager.GetVariableTemplate(variableTemplateId);
                }
                finally
                {
                    variableManager.Dispose();
                }
            }
            return null;
        }
        public static List<VariableTemplate> getVariableTemplates()
        {
            VariableManager variableManager = null;
            try
            {
                variableManager = new VariableManager();
                return variableManager.VariableTemplateRepo.Get().ToList();
            }
            finally
            {
                variableManager.Dispose();
            }
        }
        public static VariableTemplate createVariableTemplate(VariableTemplate variableTemplate)
        {
            if (variableTemplate != null && variableTemplate.Id == 0)
            {
                VariableManager variableManager = null;
                try
                {
                    variableManager = new VariableManager();
                    return variableManager.CreateVariableTemplate(variableTemplate.Label, variableTemplate.DataType, variableTemplate.Unit, variableTemplate.Description = "", variableTemplate.DefaultValue, variableTemplate.FixedValue);
                }
                finally
                {
                    variableManager.Dispose();
                }
            }
            return null;
        }
        public static bool deleteVariableTemplate(VariableTemplate variableTemplate)
        {
            if (variableTemplate != null)
            {
                VariableManager variableManager = null;
                try
                {
                    variableManager = new VariableManager();
                    return variableManager.DeleteVariableTemplate(variableTemplate.Id);
                }
                finally
                {
                    variableManager.Dispose();
                }
            }
            return false;
        }
        public static VariableTemplate editVariableTemplate(VariableTemplate variableTemplate)
        {
            if (variableTemplate != null && variableTemplate.Id != 0)
            {
                VariableManager variableManager = null;
                try
                {
                    variableManager = new VariableManager();
                    return variableManager.UpdateVariableTemplate(variableTemplate);
                }
                finally
                {
                    variableManager.Dispose();
                }
            }
            return null;
        }
        public static string variableTemplateToJson(VariableTemplate variableTemplate)
        {
            if (variableTemplate != null)
            {
                return JsonConvert.SerializeObject(variableTemplate);
            }
            return null;
        }
        public static VariableTemplate variableTemplateToJson(string json)
        {
            if (json != null && json.Length > 0)
            {
                return JsonConvert.DeserializeObject<VariableTemplate>(json);
            }
            return null;
        }
        public static string variableTemplatesToJson(List<VariableTemplate> variableTemplates)
        {
            if (variableTemplates != null && variableTemplates.Count() > 0)
            {
                return JsonConvert.SerializeObject(variableTemplates);
            }
            return null;
        }

    }
}