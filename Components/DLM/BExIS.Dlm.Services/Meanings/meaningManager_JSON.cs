using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;
using BExIS.Dlm.Entities.DataStructure;
using Newtonsoft.Json.Linq;
using BExIS.Dlm.Entities.Meanings;
using Newtonsoft.Json;

namespace BExIS.Dlm.Services.Meanings
{
    public class meaningManager_JSON : ImeaningManagr_JSON
    {
        // Track whether Dispose has been called.
        private bool disposedValue;

        public meaningManager_JSON()
        {
        }

        public JObject addMeaning(Meaning meaning)
        {

            Contract.Requires(meaning != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {

                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    repo.Put(meaning);
                    uow.Commit();
                }
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(meaning)));
                return JObject.Parse(json_string);
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject addMeaning(string Name, String ShortName, String Description, Selectable selectable, Approved approved, List<string> variables_id, List<string> ExternalLink, List<string> meaning_ids)
        {
            Contract.Requires(ExternalLink != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    List<Variable> variables = uow.GetRepository<Variable>().Get().Where(x => variables_id.Contains(x.Id.ToString())).ToList<Variable>();
                    List<ExternalLink> externalLinks = uow.GetRepository<ExternalLink>().Get().Where(x => ExternalLink.Contains(x.Id.ToString())).ToList<ExternalLink>();
                    
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    List<Meaning> related_meanings = (List<Meaning>)repo.Get().Where(x => meaning_ids.Contains(x.Id.ToString())).ToList<Meaning>();
                    using (Meaning meaning = new Meaning(Name, ShortName, Description, selectable, approved, externalLinks, variables, related_meanings))
                    {
                        repo.Put(meaning);
                        uow.Commit();
                        var xx = JsonConvert.SerializeObject(meaning, Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                        string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", xx));
                        return JObject.Parse(json_string);
                    }
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject deleteMeaning(Meaning meaning)
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
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, bool>("Success", true));
                return JObject.Parse(json_string);
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject deleteMeaning(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    Meaning meaning = repo.Get().FirstOrDefault(x => id == x.Id);
                    repo.Delete(meaning);
                    uow.Commit();
                    string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(this.getMeanings())));
                    return JObject.Parse(json_string);
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject editMeaning(Meaning meaning)
        {
            Contract.Requires(meaning != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();
                    repo.Merge(meaning);
                    var merged = repo.Get(meaning.Id);
                    repo.Put(merged);
                    uow.Commit();
                }
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(meaning)));
                return JObject.Parse(json_string);
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject editMeaning(string id, string Name, String ShortName, String Description, Selectable selectable, Approved approved, List<string> variables_id, List<string> ExternalLink, List<string> meaning_ids)
        {
            Contract.Requires(ExternalLink != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Meaning> repo = uow.GetRepository<Meaning>();

                    List<Variable> variables = uow.GetRepository<Variable>().Get().Where(x => variables_id.Contains(x.Id.ToString())).ToList<Variable>();
                    List < ExternalLink >externalLinks = uow.GetRepository<ExternalLink>().Get().Where(x => ExternalLink.Contains(x.Id.ToString())).ToList<ExternalLink>();
                    List<Meaning> related_meanings = repo.Get().Where(x => meaning_ids.Contains(x.Id.ToString())).ToList<Meaning>(); 

                    Meaning meaning = repo.Get().FirstOrDefault(x => id == x.Id.ToString());

                    meaning.Name = Name;
                    meaning.Related_meaning = related_meanings;
                    meaning.Selectable = selectable;
                    meaning.ShortName = ShortName;
                    meaning.Variables = variables;
                    meaning.ExternalLink = externalLinks;
                    meaning.Description = Description;
                    meaning.Approved = approved;
                    //repo.Merge(meaning);
                    repo.Put(meaning);
                    uow.Commit();
                    var merged = repo.Get(meaning.Id);
                    var xx = JsonConvert.SerializeObject(merged, Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                    string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", xx));

                    return JObject.Parse(json_string);
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject getMeaning(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<Meaning>();
                    Meaning Meanings = repo.Get().FirstOrDefault(x => x.Id == id);
                    string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(Meanings)));
                    return JObject.Parse(json_string);
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject getMeanings()
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<Meaning>();
                    List<Meaning> Meanings = repo.Get().ToList<Meaning>();
                    IDictionary<long, Meaning> fooDict = Meanings.ToDictionary(f => f.Id, f => f);
                    string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(Meanings)));
                    return JObject.Parse(json_string);
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject editRelatedMeaning(Meaning m)
        {
            throw new NotImplementedException();
        }

        public JObject updateRelatedManings(string parentID, string childID)
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
                    string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(repo.Get().ToList<Meaning>())));
                    return JObject.Parse(json_string);
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }

        public JObject addExternalLink(ExternalLink externalLink)
        {
            Contract.Requires(externalLink != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();
                    repo.Put(externalLink);
                    uow.Commit();
                }
                string json_string = JsonConvert.SerializeObject(externalLink);
                return JObject.Parse(json_string);
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("error", exc.Message));
                return JObject.Parse(json_string);
            }
        }

        public JObject addExternalLink(string uri, String name, String type)
        {
            Contract.Requires(uri != null);
            Contract.Requires(name != null);
            Contract.Requires(type != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {

                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();
                    using (ExternalLink externalLink = new ExternalLink(uri, name,  type))
                    {
                        repo.Put(externalLink);
                        uow.Commit();
                        string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(externalLink)));
                        return JObject.Parse(json_string);
                    }
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject deleteExternalLink(ExternalLink externalLink)
        {
            Contract.Requires(externalLink != null);
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();

                    externalLink = repo.Reload(externalLink);
                    repo.Delete(externalLink);
                    uow.Commit();
                }
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, bool>("Success", true));
                return JObject.Parse(json_string);
            }
            catch(Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
            
        }
        public JObject deleteExternalLink(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<ExternalLink> repo = uow.GetRepository<ExternalLink>();
                    ExternalLink externalLink = repo.Get().FirstOrDefault(x => id == x.Id);
                    repo.Delete(externalLink);
                    uow.Commit();
                    string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(this.getExternalLinks())));
                    return JObject.Parse(json_string);
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject editExternalLink(ExternalLink externalLink)
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
                }
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(externalLink)));
                return JObject.Parse(json_string);
            }
            catch(Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject editExternalLink(string id, string uri, String name, String type)
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
                    repo.Merge(externalLink);
                    uow.Commit();
                    string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(externalLink)));
                    return JObject.Parse(json_string);
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }

        public JObject getExternalLink(Int64 id)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<ExternalLink>();
                    ExternalLink externalLink = repo.Get().FirstOrDefault(x => x.Id == id);
                    string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(externalLink)));
                    return JObject.Parse(json_string);
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        public JObject getExternalLinks()
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetReadOnlyRepository<ExternalLink>();
                    List<ExternalLink> externalLinks = repo.Get().ToList<ExternalLink>();
                    string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Success", JsonConvert.SerializeObject(externalLinks)));
                    return JObject.Parse(json_string);
                }
            }
            catch (Exception exc)
            {
                string json_string = JsonConvert.SerializeObject(new KeyValuePair<string, string>("Error", exc.Message));
                return JObject.Parse(json_string);
            }
        }
        
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~meaningManagr()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
