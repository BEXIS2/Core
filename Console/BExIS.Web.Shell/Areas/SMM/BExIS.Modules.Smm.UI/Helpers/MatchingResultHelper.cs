using BExIS.Dlm.Entities.SpeciesMatching;
using BExIS.Dlm.Services.SpeciesMatching;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Smm.UI.Models;
using BExIS.Utils.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Smm.UI.Helpers
{
    // This helper class is used for reading and parsing the matching result files. Currently only ChecklistBank (CLB) but later also other APIs.
    public class MatchingResultHelper
    {
        // returns all SpeciesMatchingResult entries for a given datasetId, or null if an error occurs
        public static List<SpeciesMatchingResult> GetAll(long datasetId)
        {
            try
            {
                using (var smrm = new SpeciesMatchingResultManager())
                {
                    var smrmRepo = smrm.GetBulkUnitOfWork().GetReadOnlyRepository<SpeciesMatchingResult>();
                    List<SpeciesMatchingResult> result = smrmRepo.Query().Where(r => r.Dataset.Id == datasetId).ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool ApplyTailorEdits(long datasetId, long versionId, List<TailorEdit> edits)
        {
            try
            {
                using (var smrm = new SpeciesMatchingResultManager())
                using (var uow = smrm.GetUnitOfWork())
                {
                    var repo = uow.GetRepository<SpeciesMatchingResult>();
                    foreach (var edit in edits)
                    {
                        var entity = repo.Query().FirstOrDefault(e => e.Id == edit.Id && e.Dataset.Id == datasetId && e.DatasetVersionId == versionId && e.ConfirmedByUser == false);
                        if (entity != null)
                        {
                            // TODO: - adapt this when further data cleaning constraints are clearer
                            // optionally also update other fields
                            if (edit.EditedName != "")
                            {
                                entity.EditedName = edit.EditedName;
                            } else
                            {
                                entity.EditedName = edit.CleanedName;
                            }
                        }
                    }
                    uow.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}