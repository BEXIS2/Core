using BExIS.Dlm.Entities.Curation;
using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.Dlm.Services.Curation
{
    public class CurationManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public CurationManager()
        {
            _guow = this.GetIsolatedUnitOfWork();

            CurationEntryRepository = _guow.GetReadOnlyRepository<CurationEntry>();
        }

        ~CurationManager()
        {
            Dispose(true);
        }

        public IQueryable<CurationEntry> CurationEntries => CurationEntryRepository.Query();

        public IReadOnlyRepository<CurationEntry> CurationEntryRepository { get; }

        public CurationEntry Create(string topic, CurationEntryType type, long datasetId, string name, string description, string solution, int position, string source, IEnumerable<CurationNote> notes, long creatorId, bool userlsDone, bool isApproved)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "name is empty but is required.");
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source), "source is empty but is required.");

            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<CurationEntry>();
                var repoDataset = uow.GetRepository<Dataset>();
                var repoUser = uow.GetRepository<User>();

                var ds = repoDataset.Get(datasetId);
                var creator = repoUser.Get().FirstOrDefault();

                CurationEntry curationEntry = new CurationEntry(
                   topic,
                   type,
                   ds,
                   name,
                   description,
                   solution,
                   position,
                   source,
                   notes,
                   DateTime.Now,
                   creator,
                   userlsDone,
                   isApproved
                   );


                repo.Put(curationEntry);
                uow.Commit();

                return curationEntry;
            }
        }

        public CurationEntry Update(long id, string topic, string name, string description, string solution, int position, string source, IEnumerable<CurationNote> notes, bool userlsDone, bool isApproved )
        {
          
            if (id <= 0) throw new ArgumentException("id must be greater then 0.");

            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<CurationEntry>();

                var merged = repo.Get(id);
                merged.Topic = topic;
                merged.Name = name;
                merged.Description = description;
                merged.Solution = solution;
                merged.Position = position;
                merged.Source = source;
                merged.Notes = notes;
                merged.UserlsDone = userlsDone;

                repo.Put(merged);
                uow.Commit();

                return merged;
            }
        }


        public void Delete(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<CurationEntry>();
                var curationEntry = repo.Get(id);
                repo.Delete(curationEntry);
                uow.Commit();
            }
        }

        #region notes


        public CurationEntry AddNote(long id, CurationNote note)
        {
            if (id <= 0) throw new ArgumentException("id must be greater then 0.");
            if (note == null) throw new ArgumentException("note is required");


            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<CurationEntry>();

                var merged = repo.Get(id);

                merged.Notes.ToList().Add(note);

                if (note.UserType == CurationUserType.User) merged.LastChangeDatetime_User = DateTime.Now;
                else merged.LastChangeDatetime_Curator = DateTime.Now;

                repo.Put(merged);
                uow.Commit();

                return merged;
            }
        }

        public CurationEntry DeleteNote(long id, long noteId)
        {
            if (id <= 0) throw new ArgumentException("id must be greater then 0.");
            if (noteId == null) throw new ArgumentException("noteId must be greater then 0.");


            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<CurationEntry>();
                var repoNote = uow.GetRepository<CurationNote>();

                var merged = repo.Get(id);
                var note = repoNote.Get(noteId);

                merged.Notes.ToList().Remove(note);


                repo.Put(merged);
                uow.Commit();

                return merged;
            }
        }

        #endregion


        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }
    }
}
