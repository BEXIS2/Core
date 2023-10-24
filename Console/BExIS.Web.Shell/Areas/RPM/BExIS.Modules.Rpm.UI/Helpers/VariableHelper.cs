using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
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
            model.Unit = new UnitItem(variableTemplate.Unit.Id, variableTemplate.Unit.Name,variableTemplate.Unit.AssociatedDataTypes.Select(x => x.Name).ToList());

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


            variableTemplate.Meanings?.ToList().ForEach(m => model.Meanings.Add(new MeaningItem(m.Id, m.Name)));

            return model;
        }

        public VariableTemplate ConvertTo(VariableTemplateModel _model)
        {
            using (var unitManager = new UnitManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var meaningManager = new MeaningManager())
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

                if (_model.Meanings.Any())
                {
                    foreach (var item in _model.Meanings)
                    {
                        var meaning = meaningManager.getMeaning(item.Id);
                        variableTemplate.Meanings.Add(meaning);
                    }
                }

                return variableTemplate;
            }
        }

        public List<MeaningItem> GetMeanings()
        {
            using (var meaningsManager = new MeaningManager())
            {
                var meanings = meaningsManager.getMeanings();
                List<MeaningItem> list = new List<MeaningItem>();

                if (meanings.Any())
                {
                    foreach (var item in meanings)
                    {
                        list.Add(new MeaningItem(item.Id, item.Name));
                    }
                }

                return list;
            }
         }

    }
}