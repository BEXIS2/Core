using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
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
            model.Approved = variableTemplate.Approved;
                 
            // missing values 
            variableTemplate.MissingValues?.ToList().ForEach(m => model.MissingValues.Add(new MissingValueItem(m.Id, m.DisplayName, m.Description)));

            // constraints
            variableTemplate.VariableConstraints?.ToList().ForEach(c => model.Constraints.Add(new ListItem(c.Id, c.Name, getConstraintType(c),c.FormalDescription)));

            //if (variableTemplate.Id>0)
            //using (var variableManager = new VariableManager())
            //using (var missingValueManager = new MissingValueManager())
            //{
            //    model.InUse = variableManager.VariableInstanceRepo.Query().Any(v => v.VariableTemplate.Id.Equals(variableTemplate.Id));

            //    var mvs = missingValueManager.Repo.Query(m => m.Variable.Id.Equals(variableTemplate.Id));
            //    if(mvs != null && mvs.Any())
            //    {
            //        mvs.ToList().ForEach(mv => model.MissingValues.Add(new MissingValueItem(mv.Id, mv.DisplayName, mv.Description)));
            //    }
            //}

            variableTemplate.Meanings?.ToList().ForEach(m => model.Meanings.Add(new MeaningItem(m.Id, m.Name)));

            return model;
        }

        public VariableTemplate ConvertTo(VariableTemplateModel _model)
        {
            using (var unitManager = new UnitManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var meaningManager = new MeaningManager())
            using (var constraintManager = new ConstraintManager())
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
                variableTemplate.Approved = _model.Approved;

                if (_model.Meanings.Any())
                {
                    foreach (var item in _model.Meanings)
                    {
                        var meaning = meaningManager.getMeaning(item.Id);
                        variableTemplate.Meanings.Add(meaning);
                    }
                }

                // constraints
                if (_model.Constraints.Any())
                {
                    foreach (var item in _model.Constraints)
                    {
                        var c = constraintManager.ConstraintRepository.Get(item.Id);
                        variableTemplate.VariableConstraints.Add(c);
                    }
                }

                return variableTemplate;
            }
        }

        public List<MeaningItem> GetMeanings()
        {
            using (var meaningsManager = new MeaningManager())
            {
                var meanings = meaningsManager.getMeanings(); // get meanings from db
                List<MeaningItem> list = new List<MeaningItem>();

                if (meanings.Any())
                {
                    // use only the meanings that are selectable and approved
                    foreach (var item in meanings.Where(m=> (m.Selectable==true && m.Approved==true)))
                    {
                        list.Add(new MeaningItem(item.Id, item.Name,"", item.Constraints.Select(c=>c.Name).ToList()));
                    }
                }

                return list;
            }
         }

        public List<ListItem> GetConstraints()
        {
            using (var constraintManager = new ConstraintManager())
            {
                var constraints = constraintManager.Get().Where(c =>c.DataContainer==null);
                List<ListItem> list = new List<ListItem>();

                if (constraints.Any())
                {
                    foreach (var item in constraints)
                    {
                        list.Add(new ListItem(item.Id, item.Name, getConstraintType(item), item.FormalDescription));
                    }
                }

                return list;
            }
        }

        public VariableTemplateItem ConvertTo(VariableTemplate variableTemplate, string group="")
        {
            VariableTemplateItem item = new VariableTemplateItem();
            item.Id = variableTemplate.Id;
            item.Text = variableTemplate.Label;
            item.Units = new List<string>() { variableTemplate.Unit.Abbreviation };
            item.DataTypes = variableTemplate.Unit.AssociatedDataTypes.Select(x => x.Name).ToList();
            item.Meanings = variableTemplate.Meanings.Select(x => x.Name).ToList();
            item.Group = group;

            if(variableTemplate.VariableConstraints.Any())
                item.Constraints = variableTemplate.VariableConstraints.Select(x => x.Name).ToList();

            return item;

        }

        public List<MeaningItem> ConvertTo(ICollection<Meaning> meanings)
        {
            List<MeaningItem> list = new List<MeaningItem>();
            meanings.ToList().ForEach(m => list.Add(ConvertTo(m)));

            return list;

        }

        public MeaningItem ConvertTo(Meaning meaning)
        {
            MeaningItem item = new MeaningItem();
            item.Id = meaning.Id;
            item.Text = meaning.Name;

            return item;

        }

        public List<ListItem> ConvertTo(ICollection<Constraint> constraints)
        {
            List<ListItem> list = new List<ListItem>();
            constraints.ToList().ForEach(m => list.Add(ConvertTo(m)));

            return list;

        }

        public ListItem ConvertTo(Constraint constraint)
        {
            constraint.Materialize();

            ListItem item = new ListItem();
            item.Id = constraint.Id;
            item.Text = constraint.Name;
            item.Group = getConstraintType(constraint);
            item.Description = constraint.FormalDescription;

            return item;

        }

        public List<Constraint> ConvertTo(ICollection<ListItem> constraints, ConstraintManager constraintManager)
        {
            List<Constraint> list = new List<Constraint>();
            List<long> ids = constraints.Select(c => c.Id).ToList();
            list = constraintManager.ConstraintRepository.Query(c => ids.Contains(c.Id)).ToList();
            return list;

        }

        public List<Meaning> ConvertTo(List<MeaningItem> meanings, MeaningManager meaningManager)
        {
            List<Meaning> list = new List<Meaning>();
            List<long> ids = meanings.Select(c => c.Id).ToList();
            list = meaningManager.getMeanings().Where(c => ids.Contains(c.Id)).ToList();
            return list;
        }

        public VariableInstanceModel ConvertTo(VariableInstance variable)
        {
            var var = new VariableInstanceModel()
            {
                Id = variable.Id,
                Name = variable.Label,
                Description = variable.Description,
                DataType = new ListItem(variable.DataType.Id, variable.DataType.Name),
                SystemType = variable.DataType.SystemType,
                Unit = new UnitItem(variable.Unit.Id, variable.Unit.Abbreviation, variable.Unit.AssociatedDataTypes.Select(x => x.Name).ToList(), "copied"),
                IsKey = variable.IsKey,
                IsOptional = variable.IsValueOptional,
                Meanings = ConvertTo(variable.Meanings),
                Constraints = ConvertTo(variable.VariableConstraints)
            };

            // add template if exist
            if (variable.VariableTemplate != null) var.Template = ConvertTo(variable.VariableTemplate, "copied");

            // get suggestes DisplayPattern / currently only for DateTime
            if (var.SystemType.Equals(typeof(DateTime).Name))
            {
                if (variable.DisplayPatternId >= 0) var.DisplayPattern = new ListItem(variable.DisplayPatternId, DataTypeDisplayPattern.Get(variable.DisplayPatternId).DisplayPattern);
                var displayPattern = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.ToString().Equals(var.SystemType));
                displayPattern.ToList().ForEach(d => var.PossibleDisplayPattern.Add(new ListItem(d.Id, d.DisplayPattern)));
            };

            //

            return var;
        }

        public VariableInstanceModel Copy(VariableInstance variable)
        {
            var var = new VariableInstanceModel()
            {
                Id = variable.Id,
                Name = variable.Label,
                Description = variable.Description,
                DataType = new ListItem(variable.DataType.Id, variable.DataType.Name, "copied"),
                SystemType = variable.DataType.SystemType,
                Unit = new UnitItem(variable.Unit.Id, variable.Unit.Abbreviation, variable.Unit.AssociatedDataTypes.Select(x => x.Name).ToList(), "copied"),
                IsKey = variable.IsKey,
                IsOptional = variable.IsValueOptional,
                Meanings = ConvertTo(variable.Meanings),
                Constraints = ConvertTo(variable.VariableConstraints)
            };


            // add template if exist
            if (variable.VariableTemplate != null) var.Template = ConvertTo(variable.VariableTemplate, "copied");

            // get suggestes DisplayPattern / currently only for DateTime
            if (var.SystemType.Equals(typeof(DateTime).Name))
            {
                if (variable.DisplayPatternId >= 0) var.DisplayPattern = new ListItem(variable.DisplayPatternId, DataTypeDisplayPattern.Get(variable.DisplayPatternId).DisplayPattern);
                var displayPattern = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.ToString().Equals(var.SystemType));
                displayPattern.ToList().ForEach(d => var.PossibleDisplayPattern.Add(new ListItem(d.Id, d.DisplayPattern)));
            };

            return var;
        }

        public List<MissingValue> ConvertTo(List<MissingValueModel> MissingValues)
        {
            List<MissingValue> list = new List<MissingValue>();
            foreach (var mv in MissingValues)
            {
                list.Add(new MissingValue()
                {
                    DisplayName = mv.DisplayName,
                    Description = mv.Description,
                });
            }

            return list;
        }

        public List<MissingValue> ConvertTo(ICollection<MissingValueItem> items)
        {
            List<MissingValue> list = new List<MissingValue>();

            using (var missingValueManager = new MissingValueManager())
            {
                List<long> ids = items.Select(m => m.Id).ToList();
                missingValueManager.Repo.Query(m => ids.Contains(m.Id));
                return list;
            }

        }

        private string getConstraintType(Constraint c)
        {
            if (c is DomainConstraint) return "Domain";
            if (c is RangeConstraint) return "Range";
            if (c is PatternConstraint) return "Pattern";

            return string.Empty;
        }
    }
}