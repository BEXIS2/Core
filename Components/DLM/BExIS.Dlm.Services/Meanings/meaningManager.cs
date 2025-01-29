using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.Meanings
{
    public class MeaningManager : IMeaningManager, IDisposable
    {
        // Track whether Dispose has been called.
        private bool disposedValue;

        private bool isDisposed = false;
        private IUnitOfWork guow = null;

        public MeaningManager()
        {
        }

        ~MeaningManager()
        {
            Dispose(true);
        }

        #region meanings

        public Meaning AddMeaning(Meaning meaning)
        {
            Contract.Requires(meaning != null);
            Contract.Requires(GetWrongMappings(meaning.ExternalLinks).Count() == 0);
            try
            {
                var externalLinksDictionary = meaning.ExternalLinks.Select(entry => new MeaningEntry
                {
                    MappingRelation = GetOrCreateExternalLink(entry.MappingRelation),
                    MappedLinks = entry.MappedLinks.Select(value => GetOrCreateExternalLink(value)).ToList()
                }).ToList();

                List<MeaningEntry> meaningEntries = (List<MeaningEntry>)meaning.ExternalLinks;

                meaning.ExternalLinks = externalLinksDictionary;
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    repo.Put(meaning);
                    uow.Commit();
                }
                UpdateMeaningEntry();
                return meaning;
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public Meaning AddMeaning(string Name, String ShortName, String Description, bool selectable, bool approved, List<MeaningEntry> externalLinks, List<long> meaning_ids, List<long> constraint_ids)
        {
            Contract.Requires(externalLinks != null);
            Contract.Requires(GetWrongMappings(externalLinks).Count() == 0);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    IRepository<Constraint> repoConstraints = uow.GetRepository<Constraint>();

                    var externalLinksDictionary = externalLinks.Select(entry => new MeaningEntry
                    {
                        MappingRelation = GetOrCreateExternalLink(entry.MappingRelation),
                        MappedLinks = entry.MappedLinks.Select(value => GetOrCreateExternalLink(value)).ToList()
                    }).ToList();
                    externalLinks = externalLinksDictionary;

                    List<Meaning> related_meanings = new List<Meaning>();
                    if (meaning_ids != null)
                    {
                        related_meanings = (List<Meaning>)repo.Get().Where(x => meaning_ids.Contains(x.Id)).ToList<Meaning>();
                    }

                    List<Constraint> constraints = new List<Constraint>();
                    if (constraint_ids != null)
                    {
                        constraints = repoConstraints.Get().Where(x => constraint_ids.Contains(x.Id)).ToList<Constraint>();
                    }

                    Meaning meaning = new Meaning(Name, ShortName, Description, selectable, approved, externalLinks, related_meanings, constraints);

                    repo.Put(meaning);
                    uow.Commit();
                    var xx = JsonConvert.SerializeObject(meaning, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                        });
                    UpdateMeaningEntry();
                    return meaning;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public Boolean DeleteMeaning(Meaning meaning)
        {
            Contract.Requires(meaning != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    meaning = repo.Reload(meaning);
                    meaning.ExternalLinks.Clear();
                    repo.Delete(meaning);
                    uow.Commit();
                }
                UpdateMeaningEntry();
                return true;
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public List<Meaning> DeleteMeaning(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    Meaning meaning = repo.Get().FirstOrDefault(x => id == x.Id);
                    meaning = repo.Reload(meaning);
                    meaning.ExternalLinks.Clear();
                    repo.Delete(meaning);
                    uow.Commit();
                    UpdateMeaningEntry();
                    return this.GetMeanings();
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public Meaning EditMeaning(Meaning meaning)
        {
            return EditMeaning(meaning.Id, meaning.Name, meaning.ShortName, meaning.Description, meaning.Selectable, meaning.Approved, meaning.ExternalLinks.ToList(), meaning.Related_meaning?.Select(m => m.Id).ToList(), meaning.Constraints?.Select(c => c.Id).ToList());
        }

        public Meaning EditMeaning(long id, string Name, String ShortName, String Description, bool selectable, bool approved, List<MeaningEntry> externalLinks, List<long> meaning_ids, List<long> constraint_ids)
        {
            Contract.Requires(externalLinks != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    IRepository<Constraint> repoConstraints = uow.GetRepository<Constraint>();

                    var externalLinksDictionary = externalLinks.Select(entry => new MeaningEntry
                    {
                        MappingRelation = GetOrCreateExternalLink(entry?.MappingRelation),
                        MappedLinks = entry.MappedLinks.Select(value => GetOrCreateExternalLink(value)).ToList()
                    }).ToList();

                    externalLinks = externalLinksDictionary;
                    List<Meaning> related_meanings = new List<Meaning>();
                    if (meaning_ids != null)
                        related_meanings = (List<Meaning>)repo.Get().Where(x => meaning_ids.Contains(x.Id)).ToList<Meaning>();

                    ISet<Constraint> constraints = new HashSet<Constraint>();
                    if (constraint_ids != null)
                        constraints = repoConstraints.Get().Where(x => constraint_ids.Contains(x.Id)).ToHashSet<Constraint>();

                    Meaning meaning = repo.Get().FirstOrDefault(x => id == x.Id);

                    meaning.Name = Name;
                    meaning.Related_meaning = related_meanings;
                    meaning.Selectable = selectable;
                    meaning.ShortName = ShortName;
                    meaning.ExternalLinks = externalLinks;
                    meaning.Description = Description;
                    meaning.Approved = approved;
                    meaning.Constraints = constraints;

                    repo.Merge(meaning);
                    uow.Commit();
                    var merged = repo.Get(meaning.Id);
                    UpdateMeaningEntry();
                    return merged;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public Meaning GetMeaning(Int64 id)
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
            }
        }

        public Meaning GetMeaning(string name)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<Meaning>();
                    Meaning Meanings = repo.Get().FirstOrDefault(x => x.Name == name);
                    return Meanings;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public List<Meaning> GetMeanings()
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<Meaning>();
                    List<Meaning> Meanings = repo.Get().OrderBy(m=>m.Id).ToList<Meaning>();
                    //IDictionary<long, Meaning> fooDict = Meanings.ToDictionary(f => f.Id, f => f);
                    return Meanings;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public List<Meaning> UpdateRelatedManings(string parentID, string childID)
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
                    return repo.Get().ToList<Meaning>();
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        // [20241128][sven] todo: why should you intend to get back the 'wrong' mappings?
        public IEnumerable<MeaningEntry> GetWrongMappings(IEnumerable<MeaningEntry> mapping)
        {
            return mapping
                .Where(pair => pair.MappingRelation.Type != ExternalLinkType.relationship);
        }

        public IEnumerable<MeaningEntry> GetWrongMappings(IEnumerable<string> mapping)
        {
            IEnumerable<MeaningEntry> externalLinksDictionary = mapping.Select(JsonConvert.DeserializeObject<MeaningEntry>);

            return GetWrongMappings(externalLinksDictionary);
        }

        public void UpdateMeaningEntry()
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<MeaningEntry> repoMeaningEntry = uow.GetRepository<MeaningEntry>();
                    IRepository<Meaning> repoMeaning = uow.GetRepository<Meaning>();
                    foreach (MeaningEntry me in repoMeaningEntry.Get())
                    {
                        List<Meaning> meaningsWithEntries = repoMeaning.Get().Where(x => x.ExternalLinks.Contains(me)).ToList();
                        if (meaningsWithEntries.Count() == 0) repoMeaningEntry.Delete(me);
                    }
                    uow.Commit();
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        #endregion meanings

        #region external link

        public ExternalLink AddExternalLink(ExternalLink externalLink)
        {
            return AddExternalLink(externalLink.URI, externalLink.Name, externalLink.Type, externalLink.Prefix, externalLink.prefixCategory);
        }

        public ExternalLink AddExternalLink(string uri, String name, ExternalLinkType type, ExternalLink Prefix, PrefixCategory prefixCategory)
        {
            Contract.Requires(uri != null);
            Contract.Requires(name != null);
            Contract.Requires(type != null);
            if (this.GetExternalLink(uri) != null) return this.GetExternalLink(uri);
            if (type == ExternalLinkType.prefix)
            {
                Contract.Requires(Prefix == null);
                Contract.Requires(prefixCategory != null);
            }
            else
            {
                Contract.Requires(prefixCategory == null);
            }
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();
                    ExternalLink externalLink = new ExternalLink(uri, name, type, Prefix, prefixCategory);

                    if (Prefix != null) externalLink.URI = GetFormattedLinkUri(externalLink);
                    repo.Put(externalLink);
                    uow.Commit();
                    return externalLink;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public Boolean DeleteExternalLink(ExternalLink externalLink)
        {
            Contract.Requires(externalLink != null);
            if (externalLink.Type == ExternalLinkType.prefix)
                Contract.Requires(externalLink.Prefix == null);
            else Contract.Requires(externalLink.Prefix != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();

                    //externalLink = repo.Reload(externalLink);
                    repo.Delete(externalLink);
                    uow.Commit();
                }
                return true;
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public List<ExternalLink> DeleteExternalLink(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();
                    ExternalLink externalLink = repo.Get().FirstOrDefault(x => id == x.Id);
                    repo.Delete(externalLink);
                    uow.Commit();
                    return this.GetExternalLinks();
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public ExternalLink EditExternalLink(ExternalLink externalLink)
        {
            Contract.Requires(externalLink != null);
            return editExternalLink(externalLink.Id.ToString(), externalLink.URI, externalLink.Name, externalLink.Type, externalLink.Prefix, externalLink.prefixCategory);
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
            }
        }

        public ExternalLink GetExternalLink(Int64 id)
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
            }
        }

        public ExternalLink GetExternalLink(string uri)
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
            }
        }

        public List<ExternalLink> GetExternalLinks()
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
            }
        }

        public List<ExternalLink> GetPrefixes()
        {
            return GetExternalLinks().Where(p => p.Prefix == null && p.Type.Equals(ExternalLinkType.prefix)).ToList<ExternalLink>();
        }

        public string GetPrefixfromUri(string uri)
        {
            return GetPrefixes().Where(p => uri.ToLower().Contains(p.URI.ToLower())).FirstOrDefault().URI;
        }

        public string GetfullUri(ExternalLink externalLink)
        {
            string url = externalLink.URI; ;
            if (externalLink.Prefix != null)
                url = Path.Combine(externalLink.Prefix.URI, externalLink.URI);

            return url;
        }

        public string GetFormattedLinkUri(ExternalLink externalLink)
        {
            return externalLink.URI.Replace(externalLink.Prefix.URI, externalLink.Prefix.Name);
        }

        public string GetViewLinkUri(ExternalLink externalLink)
        {
            if (externalLink.Prefix != null)
                return externalLink.URI.Replace(externalLink.Prefix.Name, externalLink.Prefix.URI);
            else return externalLink.URI;
        }

        public Boolean UpdatePreviousLinks()
        {
            foreach (ExternalLink pref in GetPrefixes())
            {
                foreach (ExternalLink link in GetExternalLinks().Where(p => p.Prefix != null))
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

        private ExternalLink GetOrCreateExternalLink(ExternalLink externalLink_)
        {
            Contract.Requires(externalLink_ != null);
            if (!string.IsNullOrEmpty(externalLink_?.Name) && !string.IsNullOrEmpty(externalLink_?.URI) && this.GetExternalLink(externalLink_?.URI) == null)
                return this.AddExternalLink(externalLink_.URI, externalLink_.Name, externalLink_.Type, externalLink_.Prefix, externalLink_.prefixCategory);
            else return this.GetExternalLink(externalLink_?.URI);
        }

        public ExternalLink GetOrCreateExternalLink(string id, string name, string uri, ExternalLinkType type, ExternalLink Prefix, PrefixCategory prefixCategory)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(uri) && this.GetExternalLink(uri) == null) return this.AddExternalLink(uri, name, type, Prefix, prefixCategory);
            else return this.GetExternalLink(uri);
        }

        #endregion external link

        #region PrefixCategory

        public PrefixCategory AddPrefixCategory(PrefixCategory prefixCategory)
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
            }
        }

        public PrefixCategory AddPrefixCategory(string Name, string Description)
        {
            Contract.Requires(Name != null);
            Contract.Requires(Description != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<PrefixCategory> repo = uow.GetRepository<PrefixCategory>();
                    PrefixCategory prefixCategory = new PrefixCategory(Name, Description);
                    repo.Put(prefixCategory);
                    uow.Commit();
                    return prefixCategory;
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public Boolean DeletePrefixCategory(PrefixCategory prefixCategory)
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
            }
        }

        public List<PrefixCategory> DeletePrefixCategory(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<PrefixCategory> repo = uow.GetRepository<PrefixCategory>();
                    PrefixCategory prefixCategory = repo.Get().FirstOrDefault(x => id == x.Id);
                    repo.Delete(prefixCategory);
                    uow.Commit();
                    return this.GetPrefixCategory();
                }
            }
            catch (Exception exc)
            {
                throw (exc);
            }
        }

        public PrefixCategory EditPrefixCategory(PrefixCategory prefixCategory)
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
            }
        }

        public PrefixCategory EditPrefixCategory(string id, string Name, string Description)
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
            }
        }

        public PrefixCategory GetPrefixCategory(Int64 id)
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
            }
        }

        public PrefixCategory GetPrefixCategory(string Name)
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
            }
        }

        public List<PrefixCategory> GetPrefixCategory()
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
            }
        }

        #endregion PrefixCategory

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