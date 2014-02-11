using System.Collections.Generic;
using System.Diagnostics.Contracts;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public sealed class ClassifierManager
    {

        public ClassifierManager() //: base(false, true, true)
        {
            this.Repo = this.GetUnitOfWork().GetReadOnlyRepository<Classifier>();
        }

        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<Classifier> Repo { get; private set; }

        #endregion

        #region Classifier

        public Classifier Create(string name, string description, Classifier parent)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(Contract.Result<Classifier>() != null && Contract.Result<Classifier>().Id >= 0);

            Classifier u = new Classifier()
            {
                Name = name,
                Description = description,
                Parent = parent, // if parent is null, current node will be a root
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Classifier> repo = uow.GetRepository < Classifier>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);            
        }

        public bool Delete(Classifier entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Classifier> repo = uow.GetRepository<Classifier>();
                entity = repo.Reload(entity);
                
                //delete the Classifier
                repo.Delete(entity);

                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<Classifier> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (Classifier e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (Classifier e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Classifier> repo = uow.GetRepository<Classifier>();
                IRepository<ConversionMethod> repoCM = uow.GetRepository<ConversionMethod>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);

                    //delete the Classifier
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
        }

        public Classifier Update(Classifier entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<Classifier>() != null && Contract.Result<Classifier>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Classifier> repo = uow.GetRepository<Classifier>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);    
        }

        #endregion

        #region Associations

        public bool AddChild(Classifier end1, Classifier end2)
        {
            Contract.Requires(end1 != null && end1.Id >= 0);
            Contract.Requires(end2 != null && end2.Id >= 0);

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Classifier> repo = uow.GetRepository<Classifier>();

                end1 = repo.Reload(end1);
                repo.LoadIfNot(end1.Children);
                if (!end1.Children.Contains(end2))
                {
                    end1.Children.Add(end2);
                    end2.Parent = end1;
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        public bool RemoveChild(Classifier end1, Classifier end2)
        {
            Contract.Requires(end1 != null && end1.Id >= 0);
            Contract.Requires(end2 != null && end2.Id >= 0);

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Classifier> repo = uow.GetRepository<Classifier>();

                end1 = repo.Reload(end1);
                repo.LoadIfNot(end1.Children); 
                
                end2 = repo.Reload(end2);
                repo.LoadIfNot(end2.Parent);

                if (end1.Children.Contains(end2) || end2.Parent.Equals(end1))
                {
                    end1.Children.Remove(end2);
                    end2.Parent = null;
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        #endregion

    }
}
