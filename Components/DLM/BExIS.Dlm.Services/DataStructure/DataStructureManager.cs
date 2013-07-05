using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities.DataStructure;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using BExIS.Core.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public sealed class DataStructureManager
    {

        public DataStructureManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.SdsRepo = uow.GetReadOnlyRepository<StructuredDataStructure>();
            this.UnSdsRepo = uow.GetReadOnlyRepository<UnStructuredDataStructure>();
            this.UsageRepo = uow.GetReadOnlyRepository<VariableUsage>();
        }

        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<StructuredDataStructure> SdsRepo { get; private set; }
        public IReadOnlyRepository<UnStructuredDataStructure> UnSdsRepo { get; private set; }
        public IReadOnlyRepository<VariableUsage> UsageRepo { get; private set; }

        #endregion

        #region StructuredDataStructure

        public StructuredDataStructure CreateStructuredDataStructure(string name, string description, string xsdFileName, string xslFileName, DataStructureCategory indexerType, VariableUsage indexer = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(indexerType != DataStructureCategory.Generic ? (indexer != null) : true);            
            Contract.Ensures(Contract.Result<StructuredDataStructure>() != null && Contract.Result<StructuredDataStructure>().Id >= 0);

            StructuredDataStructure e = new StructuredDataStructure()
            {
                Name = name,
                Description = description,
                XsdFileName = xsdFileName,
                XslFileName = xslFileName,
                IndexerType = indexerType,
                // Indexer = indexer, // how its possible to have the indexer before assigning variable to the structure
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);            
        }

        public bool DeleteStructuredDataStructure(StructuredDataStructure entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();

                entity = repo.Reload(entity);
                repo.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        public bool DeleteStructuredDataStructure(IEnumerable<StructuredDataStructure> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (StructuredDataStructure e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (StructuredDataStructure e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public StructuredDataStructure UpdateStructuredDataStructure(StructuredDataStructure entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<StructuredDataStructure>() != null && Contract.Result<StructuredDataStructure>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);    
        }

        #endregion

        #region UnStructuredDataStructure

        public UnStructuredDataStructure CreateUnStructuredDataStructure(string name, string description)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(Contract.Result<UnStructuredDataStructure>() != null && Contract.Result<UnStructuredDataStructure>().Id >= 0);

            UnStructuredDataStructure e = new UnStructuredDataStructure()
            {
                Name = name,
                Description = description,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public bool DeleteUnStructuredDataStructure(UnStructuredDataStructure entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();

                entity = repo.Reload(entity);
                repo.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        public bool DeleteUnStructuredDataStructure(IEnumerable<UnStructuredDataStructure> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (UnStructuredDataStructure e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (UnStructuredDataStructure e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public UnStructuredDataStructure UpdateUnStructuredDataStructure(UnStructuredDataStructure entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");
            Contract.Ensures(Contract.Result<UnStructuredDataStructure>() != null && Contract.Result<UnStructuredDataStructure>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }
           
        #endregion

        #region Associations

        public VariableUsage AddVariableUsage(StructuredDataStructure dataStructure, DataAttribute dataAttribute, bool isValueOptional, string label)
        {
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(dataAttribute != null && dataAttribute.Id >= 0);
            Contract.Ensures(Contract.Result<VariableUsage>() != null && Contract.Result<VariableUsage>().Id >= 0);

            SdsRepo.Reload(dataStructure);
            SdsRepo.LoadIfNot(dataStructure.VariableUsages);
            int count = (   from v in dataStructure.VariableUsages
                            where v.DataAttribute.Id.Equals(dataAttribute.Id)
                            select v
                        )
                        .Count();

            //if (count > 0)
            //    throw new Exception(string.Format("Data attribute {0} is already used as a variable in data structure {0}", dataAttribute.Id, dataStructure.Id));

            VariableUsage usage = new VariableUsage()
            {
                DataStructure = dataStructure,
                DataAttribute = dataAttribute,
                IsValueOptional = isValueOptional,
                // if there is no label provided, use the data attribute name and a sequence number caculated by the number of occurances of that data attribute in the current structure
                Label = !string.IsNullOrWhiteSpace(label)? label: (count <=0 ? dataAttribute.Name: string.Format("{0} ({1})", dataAttribute.Name, count)),
            };
            dataAttribute.UsagesAsVariable.Add(usage);
            dataStructure.VariableUsages.Add(usage);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableUsage> repo = uow.GetRepository<VariableUsage>();
                repo.Put(usage);
                uow.Commit();
            }
            return (usage);
        }

        public void RemoveVariableUsage(VariableUsage usage)
        {
            Contract.Requires(usage != null && usage.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableUsage> repo = uow.GetRepository<VariableUsage>();
                IRepository<ParameterUsage> paramRepo = uow.GetRepository<ParameterUsage>();
                repo.Delete(usage);
                paramRepo.Delete(usage.ParameterUsages.ToList());
                uow.Commit();
            }            
        }

        public ParameterUsage AddParameterUsage(VariableUsage variableUsage, DataAttribute dataAttribute, bool isValueOptional, string label)
        {
            Contract.Requires(variableUsage != null && variableUsage.DataAttribute.Id >= 0);
            Contract.Requires(dataAttribute != null && dataAttribute.Id >= 0);
            Contract.Ensures(Contract.Result<ParameterUsage>() != null && Contract.Result<ParameterUsage>().Id >= 0);

            UsageRepo.Reload(variableUsage);
            UsageRepo.LoadIfNot(variableUsage.ParameterUsages);
            int count = (from pu in variableUsage.ParameterUsages
                         where pu.DataAttribute.Id.Equals(dataAttribute.Id)
                            select pu
                        )
                        .Count();

            // supprot multiple use of a data attribute as a paramater ina variable context
            //if (count > 0)
            //    throw new Exception(string.Format("Data attribute {0} is already used as a parameter in conjunction with variable {1} in data structure {2}", dataAttribute.Id, variableUsage.DataAttribute.Id, variableUsage.DataStructure.Id));

            ParameterUsage usage = new ParameterUsage()
            {
                DataAttribute = dataAttribute,
                VariableUsage = variableUsage,
                IsValueOptional = isValueOptional,
                // if there is no label provided, use the data attribute name and a sequence number caculated by the number of occurances of that data attribute in the current usage
                Label = !string.IsNullOrWhiteSpace(label) ? label : (count <= 0 ? dataAttribute.Name : string.Format("{0} ({1})", dataAttribute.Name, count)),

            };
            dataAttribute.UsagesAsParameter.Add(usage);
            variableUsage.ParameterUsages.Add(usage);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ParameterUsage> repo = uow.GetRepository<ParameterUsage>();
                repo.Put(usage);
                uow.Commit();
            }
            return (usage);
        }

        public void RemoveParameterUsage(ParameterUsage usage)
        {
            Contract.Requires(usage != null && usage.Id >= 0);
            
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ParameterUsage> repo = uow.GetRepository<ParameterUsage>();
                repo.Delete(usage);
                uow.Commit();
            }
        }

        public void AddDataView(BExIS.Dlm.Entities.DataStructure.StructuredDataStructure dataStructure, DataView view)
        {
            // view should not be connected to a Dataset. if so throw an exception and the caller must remove the relationship to that dataset and then add to a data structure
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<StructuredDataStructure>() != null && Contract.Result<StructuredDataStructure>().Id >= 0);

            
            SdsRepo.Reload(dataStructure);
            SdsRepo.LoadIfNot(dataStructure.Views);
            int count = (from v in dataStructure.Views
                         where v.Id.Equals(view.Id)
                         select v
                        )
                        .Count();

            if (count > 0)
                throw new Exception(string.Format("There is a connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

            dataStructure.Views.Add(view);
            view.DataStructures.Add(dataStructure);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                repo.Put(dataStructure);
                uow.Commit();
            }       
        }

        public void AddDataView(BExIS.Dlm.Entities.DataStructure.UnStructuredDataStructure dataStructure, DataView view)
        {
            // view should not be connected to a Dataset. if so throw an exception and the caller must remove the relationship to that dataset and then add to a data structure
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<UnStructuredDataStructure>() != null && Contract.Result<UnStructuredDataStructure>().Id >= 0);


            UnSdsRepo.Reload(dataStructure);
            UnSdsRepo.LoadIfNot(dataStructure.Views);
            int count = (from v in dataStructure.Views
                         where v.Id.Equals(view.Id)
                         select v
                        )
                        .Count();

            if (count > 0)
                throw new Exception(string.Format("There is a connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

            dataStructure.Views.Add(view);
            view.DataStructures.Add(dataStructure);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();
                repo.Put(dataStructure);
                uow.Commit();
            }
        }

        public void RemoveDataView(BExIS.Dlm.Entities.DataStructure.StructuredDataStructure dataStructure, DataView view)
        {
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<StructuredDataStructure>() != null && Contract.Result<StructuredDataStructure>().Id >= 0);
            
            SdsRepo.Reload(dataStructure);
            SdsRepo.LoadIfNot(dataStructure.Views);
            int count = (from v in dataStructure.Views
                         where v.Id.Equals(view.Id)
                         select v
                        )
                        .Count();

            if (count <= 0)
                throw new Exception(string.Format("There is no connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

            dataStructure.Views.Remove(view);
            view.DataStructures.Remove(dataStructure);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                repo.Put(dataStructure);
                uow.Commit();
            } throw new NotImplementedException();
        }

        public void RemoveDataView(BExIS.Dlm.Entities.DataStructure.UnStructuredDataStructure dataStructure, DataView view)
        {
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<UnStructuredDataStructure>() != null && Contract.Result<UnStructuredDataStructure>().Id >= 0);

            UnSdsRepo.Reload(dataStructure);
            UnSdsRepo.LoadIfNot(dataStructure.Views);
            int count = (from v in dataStructure.Views
                         where v.Id.Equals(view.Id)
                         select v
                        )
                        .Count();

            if (count <= 0)
                throw new Exception(string.Format("There is no connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

            dataStructure.Views.Remove(view);
            view.DataStructures.Remove(dataStructure);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();
                repo.Put(dataStructure);
                uow.Commit();
            } throw new NotImplementedException();
        }

        #endregion

    }
}
