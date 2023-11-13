using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;
using BExIS.Dlm.Entities.DataStructure;
using Newtonsoft.Json.Linq;
using BExIS.Dlm.Entities.Meanings;
using Newtonsoft.Json;
using System.Security.Policy;

namespace BExIS.Dlm.Services.Meanings
{
    public class MeaningManager : ImeaningManagr
    {
        // Track whether Dispose has been called.
        private bool disposedValue;

        public MeaningManager()
        {
        }
        #region meanings
        public Meaning addMeaning(Meaning meaning)
        {

            Contract.Requires(meaning != null);
            try
            {


                foreach (ExternalLink ext_link in meaning.ExternalLink)
                {
                    if (!string.IsNullOrEmpty(ext_link.Name) && !string.IsNullOrEmpty(ext_link.URI) && this.getExternalLink(ext_link.URI) == null)
                    {
                        if(ext_link.Id==0) this.addExternalLink(ext_link);
                    }
                }

                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    repo.Put(meaning);
                    uow.Commit();
                }
                return meaning;
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public Meaning addMeaning(string Name, String ShortName, String Description, Selectable selectable, Approved approved, List<string> ExternalLink, List<string> meaning_ids)
        {
            Contract.Requires(ExternalLink != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    //List<Variable> variables = uow.GetRepository<Variable>().Get().Where(x => variables_id.Contains(x.Id.ToString())).ToList<Variable>();
                    List<ExternalLink> externalLinks = uow.GetRepository<ExternalLink>().Get().Where(x => ExternalLink.Contains(x.Id.ToString())).ToList<ExternalLink>();

                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    List<Meaning> related_meanings = new List<Meaning>();
                    if (meaning_ids != null)
                    {
                        related_meanings = (List<Meaning>)repo.Get().Where(x => meaning_ids.Contains(x.Id.ToString())).ToList<Meaning>();
                    }
                    
                    using (Meaning meaning = new Meaning(Name, ShortName, Description, selectable, approved, externalLinks, related_meanings))
                    {
                        repo.Put(meaning);
                        uow.Commit();
                        var xx = JsonConvert.SerializeObject(meaning, Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                        return meaning;
                    }
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public Boolean deleteMeaning(Meaning meaning)
        {
            Contract.Requires(meaning != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();

                    meaning = repo.Reload(meaning);
                    repo.Delete(meaning);
                    uow.Commit();
                }
                return true;
            }
            catch (Exception exc)
            {
                throw (exc);
                return false;
            }
        }
        public List<Meaning> deleteMeaning(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    Meaning meaning = repo.Get().FirstOrDefault(x => id == x.Id);
                    repo.Delete(meaning);
                    uow.Commit();
                    return this.getMeanings();
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public Meaning editMeaning(Meaning meaning)
        {
            Contract.Requires(meaning != null);
            try
            {
                if (meaning.ExternalLink != null)
                {
                    foreach (ExternalLink ext_link in meaning.ExternalLink)
                    {
                        if (this.getExternalLink(ext_link.URI) == null)
                        {
                            if (ext_link.Id == 0) this.addExternalLink(ext_link);
                        }
                    }
                }

                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    repo.Merge(meaning);
                    var merged = repo.Get(meaning.Id);
                    repo.Put(merged);
                    uow.Commit();
                    return  merged;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public Meaning editMeaning(string id, string Name, String ShortName, String Description, Selectable selectable, Approved approved, List<string> ExternalLink, List<string> meaning_ids)
        {
            Contract.Requires(ExternalLink != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();

                    //List<Variable> variables = uow.GetRepository<Variable>().Get().Where(x => variables_id.Contains(x.Id.ToString())).ToList<Variable>();
                    List < ExternalLink >externalLinks = uow.GetRepository<ExternalLink>().Get().Where(x => ExternalLink.Contains(x.Id.ToString())).ToList<ExternalLink>();
                    List<Meaning> related_meanings = repo.Get().Where(x => meaning_ids.Contains(x.Id.ToString())).ToList<Meaning>();

                    Meaning meaning = repo.Get().FirstOrDefault(x => id == x.Id.ToString());

                    meaning.Name = Name;
                    meaning.Related_meaning = related_meanings;
                    meaning.Selectable = selectable;
                    meaning.ShortName = ShortName;
                    //meaning.Variables = variables;
                    meaning.ExternalLink = externalLinks;
                    meaning.Description = Description;
                    meaning.Approved = approved;
                    //repo.Merge(meaning);
                    repo.Put(meaning);
                    uow.Commit();
                    var merged = repo.Get(meaning.Id);
                    return merged;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public Meaning getMeaning(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<Meaning>();
                    Meaning Meanings = repo.Get().FirstOrDefault(x => x.Id == id);
                    return Meanings;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public List<Meaning> getMeanings()
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<Meaning>();
                    List<Meaning> Meanings = repo.Get().ToList<Meaning>();
                    //IDictionary<long, Meaning> fooDict = Meanings.ToDictionary(f => f.Id, f => f);
                    return Meanings;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }

        public List<Meaning> updateRelatedManings(string parentID, string childID)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();


                    Meaning parentMeaning = repo.Get().FirstOrDefault(x => x.Id.ToString() == parentID);
                    Meaning childMeaning = repo.Get().FirstOrDefault(x => x.Id.ToString() == childID);
                    List<Meaning> relatedMeaning = parentMeaning.Related_meaning.ToList<Meaning>();
                    relatedMeaning.Add(childMeaning);
                    parentMeaning.Related_meaning = relatedMeaning;
                    repo.Put(parentMeaning);
                    uow.Commit();

                    //IDictionary<long, Meaning> fooDict = Meanings.ToDictionary(f => f.Id, f => f);
                    return repo.Get().ToList<Meaning>();
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        #endregion

        #region external link
        public ExternalLink addExternalLink(ExternalLink externalLink)
        {
            Contract.Requires(externalLink != null);
            Contract.Requires(externalLink.URI != null);
            Contract.Requires(externalLink.Name != null);
            Contract.Requires(externalLink.Type != null);
            if (externalLink.Type == ExternalLinkType.prefix)
            {
                Contract.Requires(externalLink.Prefix == null);
                Contract.Requires(externalLink.prefixCategory != null);
            }
            if (externalLink.Type == ExternalLinkType.link)
            {
                Contract.Requires(externalLink.Prefix != null);
                Contract.Requires(externalLink.prefixCategory == null);
                externalLink.URI = getFormattedLinkUri(externalLink);
            }
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();
                    repo.Put(externalLink);
                    uow.Commit();
                }
                return externalLink;
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public ExternalLink addExternalLink(string uri, String name, ExternalLinkType type, ExternalLink Prefix, PrefixCategory prefixCategory)
        {
            Contract.Requires(uri != null);
            Contract.Requires(name != null);
            Contract.Requires(type != null);
            if (type == ExternalLinkType.prefix)
            {
                Contract.Requires(Prefix == null);
                Contract.Requires(prefixCategory != null);
            }
            if (type == ExternalLinkType.link)
            {
                Contract.Requires(Prefix != null);
                Contract.Requires(prefixCategory == null);
            }
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {

                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();
                    using (ExternalLink externalLink = new ExternalLink(uri, name, type, Prefix, prefixCategory))
                    {
                        if (type == ExternalLinkType.link) externalLink.URI = getFormattedLinkUri(externalLink);
                        repo.Put(externalLink);
                        uow.Commit();
                        return externalLink;
                    }
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public Boolean deleteExternalLink(ExternalLink externalLink)
        {
            Contract.Requires(externalLink != null);
            if (externalLink.Type == ExternalLinkType.prefix)
                Contract.Requires(externalLink.Prefix == null);
            if (externalLink.Type == ExternalLinkType.link)
                Contract.Requires(externalLink.Prefix != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();

                    externalLink = repo.Reload(externalLink);
                    repo.Delete(externalLink);
                    uow.Commit();
                }
                return true;
            }
            catch (Exception exc)
            {
                throw (exc);
                return false;
            }

        }
        public List<ExternalLink> deleteExternalLink(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();
                    ExternalLink externalLink = repo.Get().FirstOrDefault(x => id == x.Id);
                    repo.Delete(externalLink);
                    uow.Commit();
                    return this.getExternalLinks();
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public ExternalLink editExternalLink(ExternalLink externalLink)
        {
            Contract.Requires(externalLink != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();
                    repo.Merge(externalLink);
                    var merged = repo.Get(externalLink.Id);
                    repo.Put(merged);
                    uow.Commit();
                    return merged;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public ExternalLink editExternalLink(string id, string uri, String name, ExternalLinkType type, ExternalLink Prefix, PrefixCategory prefixCategory)
        {
            Contract.Requires(uri != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();

                    ExternalLink externalLink = repo.Get().FirstOrDefault(x => id == x.Id.ToString());

                    externalLink.URI = uri;
                    externalLink.Name = name;
                    externalLink.Type = type;
                    externalLink.Prefix = Prefix;
                    externalLink.prefixCategory = prefixCategory;
                    repo.Merge(externalLink);
                    uow.Commit();
                    return externalLink;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public ExternalLink getExternalLink(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<ExternalLink>();
                    ExternalLink externalLink = repo.Get().FirstOrDefault(x => x.Id == id);
                    return externalLink;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public ExternalLink getExternalLink(string uri)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<ExternalLink>();
                    ExternalLink externalLink = repo.Get().FirstOrDefault(x => x.URI == uri);
                    return externalLink;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public List<ExternalLink> getExternalLinks()
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<ExternalLink>();
                    List<ExternalLink> externalLinks = repo.Get().ToList<ExternalLink>();
                    return externalLinks;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public List<ExternalLink> getPrefixes()
        {
            return getExternalLinks().Where(p => p.Prefix == null).ToList<ExternalLink>();
        }
        public string getPrefixfromUri(string uri)
        {
            return getPrefixes().Where(p => uri.ToLower().Contains(p.URI.ToLower())).FirstOrDefault().URI;
        }
        public string getfullUri(ExternalLink externalLink)
        {
            return externalLink.URI.Replace(externalLink.Prefix.Name, externalLink.Prefix.URI);
        }
        public string getFormattedLinkUri(ExternalLink externalLink)
        {
            return externalLink.URI.Replace(externalLink.Prefix.URI, externalLink.Prefix.Name);
        }
        public string getViewLinkUri(ExternalLink externalLink)
        {
            return externalLink.URI.Replace(externalLink.Prefix.Name, externalLink.Prefix.URI);
        }
        public Boolean updatePreviousLinks()
        {
            foreach (ExternalLink pref in getPrefixes())
            {
                foreach (ExternalLink link in getExternalLinks().Where(p => p.Prefix != null))
                {
                    try
                    {
                        if (link.URI.ToLower().Contains(pref.URI.ToLower()))
                            pref.URI.ToLower().Replace(pref.URI.ToLower(), pref.Name);
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion

        #region PrefixCategory
        public PrefixCategory addPrefixCategory(PrefixCategory prefixCategory)
        {
            Contract.Requires(prefixCategory != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<PrefixCategory> repo = uow.GetRepository<PrefixCategory>();
                    repo.Put(prefixCategory);
                    uow.Commit();
                }
                return prefixCategory;
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public PrefixCategory addPrefixCategory(string Name, string Description)
        {
            Contract.Requires(Name != null);
            Contract.Requires(Description != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {

                    IRepository<PrefixCategory> repo = uow.GetRepository<PrefixCategory>();
                    using (PrefixCategory prefixCategory = new PrefixCategory(Name, Description))
                    {
                        repo.Put(prefixCategory);
                        uow.Commit();
                        return prefixCategory;
                    }
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public Boolean deletePrefixCategory(PrefixCategory prefixCategory)
        {
            Contract.Requires(prefixCategory != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<PrefixCategory> repo = uow.GetRepository<PrefixCategory>();

                    prefixCategory = repo.Reload(prefixCategory);
                    repo.Delete(prefixCategory);
                    uow.Commit();
                }
                return true;
            }
            catch (Exception exc)
            {
                throw (exc);
                return false;
            }

        }
        public List<PrefixCategory> deletePrefixCategory(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<PrefixCategory> repo = uow.GetRepository<PrefixCategory>();
                    PrefixCategory prefixCategory = repo.Get().FirstOrDefault(x => id == x.Id);
                    repo.Delete(prefixCategory);
                    uow.Commit();
                    return this.getPrefixCategory();
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public PrefixCategory editPrefixCategory(PrefixCategory prefixCategory)
        {
            Contract.Requires(prefixCategory != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<PrefixCategory> repo = uow.GetRepository<PrefixCategory>();
                    repo.Merge(prefixCategory);
                    var merged = repo.Get(prefixCategory.Id);
                    repo.Put(merged);
                    uow.Commit();
                    return merged;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public PrefixCategory editPrefixCategory(string id, string Name, string Description)
        {
            Contract.Requires(Name != null);
            Contract.Requires(Description != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<PrefixCategory> repo = uow.GetRepository<PrefixCategory>();

                    PrefixCategory prefixCategory = repo.Get().FirstOrDefault(x => id == x.Id.ToString());

                    prefixCategory.Name = Name;
                    prefixCategory.Description = Description;
                    repo.Merge(prefixCategory);
                    uow.Commit();
                    return prefixCategory;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public PrefixCategory getPrefixCategory(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<PrefixCategory>();
                    PrefixCategory prefixCategory = repo.Get().FirstOrDefault(x => x.Id == id);
                    return prefixCategory;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public PrefixCategory getPrefixCategory(string Name)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<PrefixCategory>();
                    PrefixCategory prefixCategory = repo.Get().FirstOrDefault(x => x.Name == Name);
                    return prefixCategory;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        public List<PrefixCategory> getPrefixCategory()
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<PrefixCategory>();
                    List<PrefixCategory> prefixCategorys = repo.Get().ToList<PrefixCategory>();
                    return prefixCategorys;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
                return null;
            }
        }
        #endregion
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}