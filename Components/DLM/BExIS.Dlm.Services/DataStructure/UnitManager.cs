using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public class UnitManager : IDisposable
    {
        private IUnitOfWork guow = null;
        public UnitManager() //: base(false, true, true)
        {
            //// define aggregate paths
            ////AggregatePaths.Add((Unit u) => u.ConversionsIamTheSource);
            guow = this.GetIsolatedUnitOfWork();
            this.Repo = guow.GetReadOnlyRepository<Unit>();
            //this.ConversionMethodRepo = uow.GetReadOnlyRepository<ConversionMethod>();
            this.DimensionRepo = guow.GetReadOnlyRepository<Dimension>();
        }

        private bool isDisposed = false;
        ~UnitManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (guow != null)
                        guow.Dispose();
                    isDisposed = true;
                }
            }
        }

        #region Data Readers
        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<Unit> Repo { get; private set; }
        //public IReadOnlyRepository<ConversionMethod> ConversionMethodRepo { get; private set; }
        public IReadOnlyRepository<Dimension> DimensionRepo { get; private set; }
        #endregion

        #region Unit

        public Unit Create(string name, string abbreviation, string description, Dimension dimension, MeasurementSystem measurementSystem)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(abbreviation));
            Contract.Requires(dimension != null);

            Contract.Ensures(Contract.Result<Unit>() != null && Contract.Result<Unit>().Id >= 0);


            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Unit> repo = uow.GetRepository<Unit>();

                if (repo.Query(p => p.Name.ToLower() == name.ToLower()).Count() <= 0)
                {
                    Unit unit = new Unit()
                    {
                        Name = name,
                        Abbreviation = abbreviation,
                        Description = description,
                        Dimension = dimension,
                        MeasurementSystem = measurementSystem,
                    };
                    repo.Put(unit);
                    uow.Commit();
                    return (unit);
                }
                return null; // This should throw an exception instead.
            }
        }

        public bool Delete(Unit entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Unit> repo = uow.GetRepository<Unit>();
                IRepository<ConversionMethod> repoCM = uow.GetRepository<ConversionMethod>();

                entity = repo.Reload(entity);

                // delete all conversions that are somehow connected to current unit
                repoCM.Delete(entity.ConversionsIamTheSource);
                repoCM.Delete(entity.ConversionsIamTheTarget);

                // remove all associations between current unit and the conversions
                entity.ConversionsIamTheSource.ToList().ForEach(a => a.Source = a.Target = null);
                entity.ConversionsIamTheTarget.ToList().ForEach(a => a.Source = a.Target = null);
                entity.ConversionsIamTheSource.Clear();
                entity.ConversionsIamTheTarget.Clear();

                //delete the unit
                repo.Delete(entity);

                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<Unit> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (Unit e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (Unit e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Unit> repo = uow.GetRepository<Unit>();
                IRepository<ConversionMethod> repoCM = uow.GetRepository<ConversionMethod>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);

                    // delete all conversions that are somehow connected to current unit
                    repoCM.Delete(latest.ConversionsIamTheSource);
                    repoCM.Delete(latest.ConversionsIamTheTarget);

                    // remove all associations between current unit and the conversions
                    latest.ConversionsIamTheSource.ToList().ForEach(a => a.Source = a.Target = null);
                    latest.ConversionsIamTheTarget.ToList().ForEach(a => a.Source = a.Target = null);
                    latest.ConversionsIamTheSource.Clear();
                    latest.ConversionsIamTheTarget.Clear();

                    //delete the unit
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
        }

        public Unit Update(Unit entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<Unit>() != null && Contract.Result<Unit>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Unit> repo = uow.GetRepository<Unit>();
                //IRepository<DataType> dtRepo = uow.GetRepository<DataType>();
                //List<long> dtIds = entity.AssociatedDataTypes.Select(d => d.Id).ToList();
                //entity.AssociatedDataTypes = dtRepo.Query().Where(p => dtIds.Contains(p.Id)).ToList();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region Dimension

        public Dimension Create(string name, string description, string specification)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(specification));

            Contract.Ensures(Contract.Result<Dimension>() != null && Contract.Result<Dimension>().Id >= 0);

            Dimension d = new Dimension()
            {
                Name = name,
                Description = description,
                Specification = specification,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dimension> repo = uow.GetRepository<Dimension>();
                repo.Put(d);
                uow.Commit();
            }
            return (d);
        }

        public bool Delete(Dimension entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dimension> repo = uow.GetRepository<Dimension>();
                entity = repo.Reload(entity);
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<Dimension> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (Dimension e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (Dimension e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dimension> repo = uow.GetRepository<Dimension>();

                foreach (var entity in entities)
                {
                    //var latest = repo.Reload(entity);
                    repo.Delete(entity);
                }
                uow.Commit();
            }
            return (true);
        }

        public Dimension Update(Dimension entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<Dimension>() != null && Contract.Result<Dimension>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dimension> repo = uow.GetRepository<Dimension>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region Conversion Method

        /// <summary>
        /// if there is no conversion method between source and target units, creates one otherwise fails
        /// </summary>
        /// <param name="description"></param>
        /// <param name="formula"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public ConversionMethod CreateConversionMethod(string formula, string description, Unit source, Unit target)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(formula), "formula can not be empty");
            Contract.Requires(formula.Contains("s"), "Formula must use \'s\' as the representative for the source unit");
            Contract.Requires(source != null, "source unit can not be null");
            Contract.Requires(source.Id >= 0, "source unit must be persisted before this call");
            Contract.Requires(target != null, "target unit can not be null");
            Contract.Requires(target.Id >= 0, "target unit must be persisted before this call");

            Contract.Ensures(Contract.Result<ConversionMethod>() != null && Contract.Result<ConversionMethod>().Id >= 0, "No Conversion Method persisted!");

            ConversionMethod cm = new ConversionMethod()
            {
                Formula = formula,
                Description = description,
                Source = source,
                Target = target,
            };

            source.ConversionsIamTheSource.Add(cm);
            target.ConversionsIamTheTarget.Add(cm);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ConversionMethod> repoCM = uow.GetRepository<ConversionMethod>();
                ConversionMethod temp = repoCM.Get(c => c.Source.Id == source.Id && c.Target.Id == target.Id).FirstOrDefault(); // change it to Count instead of Get
                if (temp != null)
                    throw new Exception(string.Format("There is already a conversion method between {0} and {1} having [{2}] formula", cm.Source.Name, cm.Target.Name, cm.Formula));
                repoCM.Put(cm);
                uow.Commit();
            }
            return (cm);
        }

        /// <summary>
        /// Deletes the proveded conversion method, but does not touch the source and target units
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool DeleteConversionMethod(ConversionMethod entity)
        {
            Contract.Requires(entity != null, "provided unit can not be null");
            Contract.Requires(entity.Id >= 0, "Id can not be empty");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ConversionMethod> repoCM = uow.GetRepository<ConversionMethod>();

                entity = repoCM.Reload(entity);

                // remove all associations
                entity.Source = null;
                entity.Target = null;

                //delete the entity
                repoCM.Delete(entity);

                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool DeleteConversionMethod(IEnumerable<ConversionMethod> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (ConversionMethod e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (ConversionMethod e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ConversionMethod> repoCM = uow.GetRepository<ConversionMethod>();

                foreach (var entity in entities)
                {
                    var latest = repoCM.Reload(entity); ;

                    // remove all associations
                    latest.Source = null;
                    latest.Target = null;

                    //delete the entity
                    repoCM.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
        }

        public ConversionMethod UpdateConversionMethod(ConversionMethod entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<ConversionMethod>() != null && Contract.Result<ConversionMethod>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ConversionMethod> repo = uow.GetRepository<ConversionMethod>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region Associations

        public bool AddAssociatedDataType(Unit end1, DataType end2)
        {
            Contract.Requires(end1 != null && end1.Id >= 0);
            Contract.Requires(end2 != null && end2.Id >= 0);

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Unit> repo = uow.GetRepository<Unit>();

                end1 = repo.Reload(end1);
                repo.LoadIfNot(end1.AssociatedDataTypes);
                if (!end1.AssociatedDataTypes.Contains(end2))
                {
                    end1.AssociatedDataTypes.Add(end2);
                    end2.ApplicableUnits.Add(end1);
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        public bool AddAssociatedDataType(Unit end1, IEnumerable<DataType> end2)
        {
            Contract.Requires(end1 != null && end1.Id >= 0);
            Contract.Requires(Contract.ForAll(end2, (DataType e) => e != null));
            Contract.Requires(Contract.ForAll(end2, (DataType e) => e.Id >= 0));

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Unit> repo = uow.GetRepository<Unit>();

                end1 = repo.Reload(end1);
                repo.LoadIfNot(end1.AssociatedDataTypes);
                foreach (var e2 in end2)
                {
                    if (!end1.AssociatedDataTypes.Contains(e2))
                    {
                        end1.AssociatedDataTypes.Add(e2);
                        e2.ApplicableUnits.Add(end1);
                    }
                }
                uow.Commit();
                result = true;
            }
            return (result);
        }

        public bool RemoveAssociatedDataType(Unit end1, DataType end2)
        {
            Contract.Requires(end1 != null && end1.Id >= 0);
            Contract.Requires(end2 != null && end2.Id >= 0);

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Unit> repo = uow.GetRepository<Unit>();
                IRepository<DataType> dtRepo = uow.GetRepository<DataType>();

                end1 = repo.Reload(end1);
                repo.LoadIfNot(end1.AssociatedDataTypes);

                end2 = dtRepo.Reload(end2);
                dtRepo.LoadIfNot(end2.ApplicableUnits);

                if (end1.AssociatedDataTypes.Contains(end2) || end2.ApplicableUnits.Contains(end1))
                {
                    end1.AssociatedDataTypes.Remove(end2);
                    end2.ApplicableUnits.Remove(end1);
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        public bool RemoveAssociatedDataType(Unit end1, IEnumerable<DataType> end2)
        {
            Contract.Requires(end1 != null && end1.Id >= 0);
            Contract.Requires(Contract.ForAll(end2, (DataType e) => e != null));
            Contract.Requires(Contract.ForAll(end2, (DataType e) => e.Id >= 0));

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Unit> repo = uow.GetRepository<Unit>();
                IRepository<DataType> dtRepo = uow.GetRepository<DataType>();

                //end1 = repo.Reload(end1);
                repo.LoadIfNot(end1.AssociatedDataTypes);

                foreach (var e2 in end2)
                {
                    //var e2Loaded = dtRepo.Reload(e2);
                    //dtRepo.LoadIfNot(e2Loaded.ApplicableUnits);
                    if (end1.AssociatedDataTypes.Contains(e2) || e2.ApplicableUnits.Contains(end1))
                    {
                        end1.AssociatedDataTypes.Remove(e2);
                        e2.ApplicableUnits.Remove(end1);
                    }
                }
                uow.Commit();
                result = true;
            }
            return (result);
        }
        #endregion

    }
}
