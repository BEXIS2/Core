using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.DataType.DisplayPattern;
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
            model.Unit = new ListItem(variableTemplate.Unit.Id, variableTemplate.Unit.Name);

            // missing values 
            variableTemplate.MissingValues?.ToList().ForEach(m => model.MissingValues.Add(new MissingValueItem(m.Id, m.DisplayName, m.Description)));

            if(variableTemplate.Id>0)
            using (var variableManager = new VariableManager())
            using (var missingValueManager = new MissingValueManager())
            {
                model.InUse = variableManager.VariableInstanceRepo.Query().Any(v => v.VariableTemplate.Id.Equals(variableTemplate.Id));

                var mvs = missingValueManager.Repo.Query(m => m.Variable.Id.Equals(variableTemplate.Id));
                if(mvs != null && mvs.Any())
                {
                    mvs.ToList().ForEach(mv => model.MissingValues.Add(new MissingValueItem(mv.Id, mv.DisplayName, mv.Description)));
                }
            }

            //variableTemplate.VariableConstraints = _model.VariableConstraints;

            return model;
        }

        public VariableTemplate ConvertTo(VariableTemplateModel _model)
        {
            using (var unitManager = new UnitManager())
            using (var dataTypeManager = new DataTypeManager())
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

                variableTemplate.Id = _model.Id;
                variableTemplate.Label = _model.Name;
                variableTemplate.Description = _model.Description;
                variableTemplate.Approved = _model.Approved;
                variableTemplate.DataType = dataType;
                variableTemplate.Unit = unit;

                //variableTemplate.VariableConstraints = _model.VariableConstraints;

                return variableTemplate;
            }

        }


    }
}