using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Modules.Rpm.UI.Models.DataStructure;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Rpm.UI.Helpers
{
    public class VariableHelper
    {
        public VariableTemplateModel ConvertTo(VariableTemplate variableTemplate)
        {
            var model = new VariableTemplateModel();
            model.Id = variableTemplate.Id;
            model.Name = variableTemplate.Label;
            model.Description = variableTemplate.Description;
       
            model.DataType = new ListItem(variableTemplate.DataType.Id, variableTemplate.DataType.Name);
            model.DisplayPattern = new ListItem(variableTemplate.DisplayPatternId,"");
      
            model.Unit = new ListItem(variableTemplate.Unit.Id, variableTemplate.Unit.Name);

            // missing values 
            variableTemplate.MissingValues?.ToList().ForEach(m => model.MissingValues.Add(new ListItem(m.Id, m.Placeholder)));

            //variableTemplate.VariableConstraints = _model.VariableConstraints;

            return model;
        }

        public VariableTemplate ConvertTo(VariableTemplateModel _model)
        {
            using (var unitManager = new UnitManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var missingValueManager = new MissingValueManager())
            {

                var variableTemplate = new VariableTemplate();

                if (_model == null) throw new ArgumentNullException("model");
                if(_model.DataType == null) throw new ArgumentNullException("DataType");
                if(_model.Unit == null) throw new ArgumentNullException("Unit");

                DataType dataType = dataTypeManager.Repo.Get(_model.DataType.Id);
                Unit unit = unitManager.Repo.Get(_model.Unit.Id);

                if (dataType == null) throw new ArgumentNullException("dataType not exist");
                if (unit == null) throw new ArgumentNullException("unit not exist");

                List<MissingValue> missingValues = new List<MissingValue>();
                if (_model.MissingValues!=null && _model.MissingValues.Any())
                {
                    foreach (var mv in _model.MissingValues)
                    {
                        var m = missingValueManager.Repo.Get(mv.Id);
                        if (m != null) variableTemplate.MissingValues.Add(m);
                    }
                }

                variableTemplate.Id = _model.Id;
                variableTemplate.Label = _model.Name;
                variableTemplate.Description = _model.Description;
                variableTemplate.MissingValues = missingValues;
                variableTemplate.Approved = _model.Approved;
                variableTemplate.DataType = dataType;
                variableTemplate.Unit = unit;
                variableTemplate.DisplayPatternId = (Int32)_model.DisplayPattern.Id;

                variableTemplate.Unit = unit;
                //variableTemplate.VariableConstraints = _model.VariableConstraints;

                return variableTemplate;
            }

        }


    }
}