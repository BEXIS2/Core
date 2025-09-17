using BExIS.Dlm.Entities.Curation;
using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

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

        private static void FixPositions(IEnumerable<CurationEntry> entries, CurationEntry entry)
        {
            if (entry.Type == CurationEntryType.StatusEntryItem) return;

            if (entry.Position <= 0) throw new ArgumentException("Position must be greater than 0.");

            // take all entries of the same type
            var entryList = entries.Where(e => e.Type == entry.Type)
                .OrderBy(e => e.Topic + e.Name + e.Description).OrderBy(e => e.Position).ToList();

            if (entry.Position <= entryList.Count)
            {
                // place entry at the correct place inside the list
                entryList.Remove(entry);
                entryList.Insert(entry.Position - 1, entry);
            }

            for (var i = 0; i < entryList.Count; i++)
            {
                // override every position according to the new list
                entryList[i].Position = i + 1;
            }
        }

        public CurationEntry Create(string topic, CurationEntryType type, long datasetId, string name, string description, string solution, int position, string source, IEnumerable<CurationNote> notes, bool userIsDone, bool isApproved, User user)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "name is empty but is required.");
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source), "source is empty but is required.");

            if (type == CurationEntryType.StatusEntryItem && position != 0) throw new ArgumentException("Position for StatusEntryItem must be 0.");

            using (var uow = this.GetUnitOfWork())
            {
                var repoEntries = uow.GetRepository<CurationEntry>();
                var repoDataset = uow.GetRepository<Dataset>();
                var repoUser = uow.GetRepository<User>();

                // Re-fetch user from the current session and ensure Groups are loaded
                var loadedUser = repoUser.Get(user.Id);
                // Force loading of Groups collection
                var _ = loadedUser.Groups.Count;

                // set last change date time
                if (CurationEntry.GetCurationUserType(loadedUser) != CurationUserType.Curator)
                    throw new UnauthorizedAccessException("Only curators are allowed to create curation entries.");

                var dataset = repoDataset.Get(datasetId);

                // check if dataset exists
                if (dataset == null)
                    throw new ArgumentException($"Dataset with id {datasetId} does not exist.", nameof(datasetId));

                CurationEntry curationEntry = new CurationEntry(
                    topic,
                    type,
                    dataset,
                    name,
                    description,
                    solution,
                    position,
                    source,
                    notes,
                    DateTime.Now,
                    loadedUser,
                    userIsDone,
                    isApproved,
                    lastChangeDatetime_User: DateTime.MinValue,
                    lastChangeDatetime_Curator: DateTime.Now
                );

                repoEntries.Put(curationEntry);

                // check/fix positions
                var datasetEntries = repoEntries.Get(e => e.Dataset == dataset);
                if (type != CurationEntryType.StatusEntryItem) 
                { 
                    FixPositions(datasetEntries, curationEntry);
                }

                uow.Commit();

                return curationEntry;
            }
        }

        public CurationEntry Update(long id, string topic, CurationEntryType type, string name, string description, string solution, int position, string source, IEnumerable<CurationNote> notes, bool userIsDone, bool isApproved, User user)
        {
            if (id <= 0) throw new ArgumentException("id must be greater than 0.");

            if (type == CurationEntryType.StatusEntryItem && position != 0) throw new ArgumentException("Position for StatusEntryItem must be 0.");
            if (type != CurationEntryType.StatusEntryItem && position <= 0) throw new ArgumentException("Position for none StatusEntryItem must be greater than 0.");

            using (var uow = this.GetUnitOfWork())
            {
                var repoEntries = uow.GetRepository<CurationEntry>();
                var repoNotes = uow.GetRepository<CurationNote>();
                var repoUser = uow.GetRepository<User>();

                // Re-fetch user from the current session and ensure Groups are loaded
                var loadedUser = repoUser.Get(user.Id);
                // Force loading of Groups collection
                var _ = loadedUser.Groups.Count;

                var isCurator = CurationEntry.GetCurationUserType(loadedUser) == CurationUserType.Curator;

                var merged = repoEntries.Get(id);

                var currentNotes = merged.Notes.ToList();
                
                var incomingIds = new HashSet<long>(notes.Select(n => n.Id));
                incomingIds.Remove(0);
                var deletedNotes = merged.Notes.Where(n => !incomingIds.Contains(n.Id)).ToList();
                foreach (var note in deletedNotes)
                {
                    if (loadedUser.Id != note.User.Id) continue; // do not delete notes from other users
                    currentNotes.Remove(note);
                    repoNotes.Delete(note);
                }

                foreach (var incomingNote in notes)
                {
                    var existingNote = currentNotes.FirstOrDefault(n => n.Id == incomingNote.Id);
                    if (incomingNote.Id == 0 || existingNote == null)
                    {
                        var newNote = new CurationNote(loadedUser, incomingNote.Comment);
                        currentNotes.Add(newNote);
                    }
                    else
                    {
                        if (loadedUser.Id != existingNote.User.Id) continue; // do not change notes from other users
                        existingNote.Comment = incomingNote.Comment;
                    }
                }

                merged.Notes = currentNotes;

                var changed = false;

                if (isCurator)
                {
                    changed = topic != merged.Topic ||
                              type != merged.Type ||
                              name != merged.Name ||
                              description != merged.Description ||
                              solution != merged.Solution ||
                              source != merged.Source ||
                              userIsDone != merged.UserIsDone ||
                              isApproved != merged.IsApproved;
                    merged.Topic = topic;
                    merged.Type = type;
                    merged.Name = name;
                    merged.Description = description;
                    merged.Solution = solution;
                    merged.Source = source;
                    merged.UserIsDone = userIsDone;
                    merged.IsApproved = isApproved;
                    if (changed)
                        merged.LastChangeDatetime_Curator = DateTime.Now;
                }
                else
                {
                    changed = userIsDone != merged.UserIsDone;
                    merged.UserIsDone = userIsDone;
                    if (!isApproved)
                    {
                        changed |= isApproved != merged.IsApproved;
                        merged.IsApproved = isApproved;
                    }                        
                    if (changed)
                        merged.LastChangeDatetime_User = DateTime.Now;
                }


                // check/fix positions
                if (merged.Position != position)
                {
                    merged.Position = position;
                    var datasetEntries = repoEntries.Get(e => e.Dataset == merged.Dataset);
                    FixPositions(datasetEntries, merged);
                }

                //repoEntries.Put(merged);
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
                    _guow?.Dispose();
                    _isDisposed = true;
                }
            }
        }
    }
}
