using BExIS.Dim.Entities.Mappings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Services.Mappings
{
    public class MappingManager : IDisposable
    {
        private IUnitOfWork guow = null;
        private bool isDisposed = false;

        public MappingManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.LinkElementRepo = guow.GetReadOnlyRepository<LinkElement>();
            this.MappingRepo = guow.GetReadOnlyRepository<Mapping>();
            this.TransformationRuleRepo = guow.GetReadOnlyRepository<TransformationRule>();
        }

        ~MappingManager()
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
        public IReadOnlyRepository<LinkElement> LinkElementRepo { get; private set; }

        public IReadOnlyRepository<Mapping> MappingRepo { get; private set; }
        public IReadOnlyRepository<TransformationRule> TransformationRuleRepo { get; private set; }

        #endregion Data Readers

        #region LINK ELEMENT

        public LinkElement CreateLinkElement(long elementId, LinkElementType type, LinkElementComplexity complexity, string name, string xpath, bool isSequence = false)
        {
            Contract.Requires(elementId >= 0);

            //LinkElement parent = this.GetLinkElement(parentId);

            LinkElement linkElement;

            linkElement = new LinkElement()
            {
                ElementId = elementId,
                Type = type,
                Name = name,
                XPath = xpath,
                IsSequence = isSequence,
                //Parent = parent,
                Complexity = complexity,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<LinkElement> repo = uow.GetRepository<LinkElement>();
                repo.Put(linkElement);
                uow.Commit();
            }

            return (linkElement);
        }

        public bool DeleteLinkElement(LinkElement entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<LinkElement> repo = uow.GetRepository<LinkElement>();

                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public LinkElement GetLinkElement(long elementid, LinkElementType type)
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<LinkElement>().Get().FirstOrDefault(le => le.ElementId.Equals(elementid) && le.Type.Equals(type));
        }

        public LinkElement GetLinkElement(long elementid, string name, LinkElementType type)
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<LinkElement>().Get().FirstOrDefault(le => le.ElementId.Equals(elementid) && le.Name.Equals(name) && le.Type.Equals(type));
        }

        public IEnumerable<LinkElement> GetLinkElements()
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<LinkElement>().Get();
        }

        public LinkElement UpdateLinkElement(LinkElement entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<LinkElement>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();

                return (entity);
            }
        }

        #endregion LINK ELEMENT

        #region Mapping

        public Mapping CreateMapping(

            long source_elementId,
            LinkElementType source_type,
            LinkElementComplexity source_complexity,
            string source_name,
            string source_xpath,
            long target_elementId,
            LinkElementType target_type,
            LinkElementComplexity target_complexity,
            string target_name,
            string target_xpath,
            bool source_isSequence = false,
            bool target_isSequence = false,
            TransformationRule rule = null,
            long parentMappingId = 0

            )
        {
            LinkElement source = CreateLinkElement(
                source_elementId,
                source_type,
                source_complexity,
                source_name,
                source_xpath,
                source_isSequence
                );

            LinkElement target = CreateLinkElement(
                target_elementId,
                target_type,
                target_complexity,
                target_name,
                target_xpath,
                target_isSequence
                );

            Mapping mapping = new Mapping();

            mapping.Source = source;
            mapping.Target = target;
            mapping.TransformationRule = rule;

            if (parentMappingId > 0)
            {
                mapping.Parent = this.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Get(parentMappingId);
            }

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Mapping> repo = uow.GetRepository<Mapping>();
                repo.Put(mapping);
                uow.Commit();
            }

            return (mapping);
        }

        public Mapping CreateMapping(LinkElement source, LinkElement target, long level, TransformationRule rule, Mapping parent)
        {
            Mapping mapping = new Mapping();

            mapping.Source = source;
            mapping.Target = target;
            mapping.TransformationRule = rule;
            mapping.Level = level;
            mapping.Parent = parent;

            Debug.WriteLine("------------------------------------");
            if (source != null) Debug.WriteLine(source.Id); else Debug.WriteLine("null");
            if (target != null) Debug.WriteLine(target.Id); else Debug.WriteLine("null");
            if (rule != null) Debug.WriteLine(rule.Id); else Debug.WriteLine("null");
            if (parent != null) Debug.WriteLine(parent.Id); else Debug.WriteLine("null");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Mapping> repo = uow.GetRepository<Mapping>();
                if (repo != null) Debug.WriteLine("repo not null"); else Debug.WriteLine("null");
                repo.Put(mapping);
                uow.Commit();
            }

            return (mapping);
        }

        //TODO add more complexity to the deleting function, also source and target need to delete if no mapping is using that link elements
        public bool DeleteMapping(long id)
        {
            Contract.Requires(id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                var entity = uow.GetRepository<Mapping>().Get(id);
                if (entity != null)
                {
                    IRepository<Mapping> repo = uow.GetRepository<Mapping>();

                    repo.Delete(entity);

                    uow.Commit();
                }
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public IEnumerable<Mapping> GetChildMapping(LinkElement source, LinkElement target)
        {
            long parentMappingId = GetMapping(source, target).Id;

            return this.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Query().Where(m => m.Parent != null && m.Parent.Id.Equals(parentMappingId));
        }

        public IEnumerable<Mapping> GetChildMapping(LinkElement source, LinkElement target, long level)
        {
            long parentMappingId = GetMapping(source, target).Id;

            return this.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Query().Where(m => m.Parent != null &&
            m.Parent.Id.Equals(parentMappingId) &&
            m.Level.Equals(level));
        }

        public IEnumerable<Mapping> GetChildMapping(long id)
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Query().Where(m => m.Parent != null &&
            m.Parent.Id.Equals(id));
        }

        public IEnumerable<Mapping> GetChildMapping(long id, long level)
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Query().Where(m => m.Parent != null &&
            m.Parent.Id.Equals(id) &&
            m.Level.Equals(level));
        }

        public IEnumerable<Mapping> GetChildMappingFromRoot(long id, long level)
        {
            if (level == 1) //if id is root and you want to get only the next children in level 1
            {
                return this.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Query().Where(m => m.Parent != null &&
                m.Parent.Id.Equals(id) &&
                m.Level.Equals(level));
            }
            else
            if (level == 2) //if you want to have all mappings from level 2 - starten from root
            {
                var mappingsLvl1 = GetChildMapping(id, 1);

                // get all level 2
                var mappingsLvl2 = new List<Mapping>();
                foreach (var mapping in mappingsLvl1)
                {
                    mappingsLvl2.AddRange(GetChildMapping(mapping.Id, 2).ToList());
                }

                return mappingsLvl2;
            }

            return new List<Mapping>();
        }

        public Mapping GetMapping(long id)
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Get().FirstOrDefault(m => m.Id.Equals(id));
        }

        public Mapping GetMapping(LinkElement source, LinkElement target)
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Get().FirstOrDefault(m => m.Source.Id.Equals(source.Id) &&
                    m.Source.Type.Equals(source.Type) &&
                    m.Target.Id.Equals(target.Id) &&
                    m.Target.Type.Equals(target.Type));
        }

        public IEnumerable<Mapping> GetMappings()
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Get();
        }

        public Mapping GetMappings(long id)
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Get().FirstOrDefault(m => m.Id.Equals(id));
        }

        public Mapping UpdateMapping(Mapping mapping)
        {
            Contract.Requires(mapping != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Mapping> repo = uow.GetRepository<Mapping>();
                repo.Merge(mapping);
                var merged = repo.Get(mapping.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return (mapping);
        }

        #endregion Mapping

        #region Transformation Rule

        public TransformationRule CreateTransformationRule(string regex, string mask, string defaultValue = "")
        {
            var transformationRule = new TransformationRule()
            {
                RegEx = regex,
                Mask = mask,
                DefaultValue = defaultValue
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TransformationRule> repo = uow.GetRepository<TransformationRule>();
                repo.Put(transformationRule);
                uow.Commit();
            }

            return (transformationRule);
        }

        public TransformationRule UpdateTransformationRule(long id, string regex, string mask, string defaultValue = "")
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                var transformationRule = uow.GetRepository<TransformationRule>().Get(id);

                if (transformationRule == null)
                    transformationRule = CreateTransformationRule(regex, mask, defaultValue);
                else
                {
                    transformationRule.RegEx = regex;
                    transformationRule.Mask = mask;
                    transformationRule.DefaultValue = defaultValue;
                }

                IRepository<TransformationRule> repo = uow.GetRepository<TransformationRule>();
                repo.Put(transformationRule);
                uow.Commit();

                return (transformationRule);
            }
        }

        #endregion Transformation Rule
    }
}