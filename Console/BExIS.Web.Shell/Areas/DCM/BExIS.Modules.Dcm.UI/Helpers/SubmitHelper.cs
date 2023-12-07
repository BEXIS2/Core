using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Hooks.Logs;
using System;
using System.IO;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class SubmitHelper
    {
        public SubmitModel GetModel(long id)
        {
            using (var datasetManager = new DatasetManager())
            {
                if (datasetManager.IsDatasetCheckedIn(id) == false) throw new Exception("Dataset is in process, try again later");

                var dataset = datasetManager.GetDataset(id);
                if (dataset == null) new NullReferenceException("dataset with id " + id + " not exist");
                var datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                if (datasetVersion == null) new NullReferenceException("latest datasetVersion not exist");

                SubmitModel model = new SubmitModel();
                HookManager hookManager = new HookManager();

                // set dataset and structre informations
                model.Id = id;
                model.Title = datasetVersion.Title;

                if (dataset.DataStructure != null)
                {
                    model.StructureId = dataset.DataStructure.Id;
                    model.StructureTitle = dataset.DataStructure.Name;
                    model.HasStructrue = true;
                }
                // load cache to get informations about the current upload workflow
                EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

                if (cache != null)
                {
                    #region file informations

                    model.AsciiFileReaderInfo = cache.AsciiFileReaderInfo;

                    string path = Path.Combine(AppConfiguration.DataPath, "datasets", id.ToString(), "Temp");
                    if (cache.Files != null)
                    {
                        int countReadableFile = 0;
                        for (int i = 0; i < cache.Files.Count; i++)
                        {
                            var file = cache.Files[i];
                            //check if if exist on server or not
                            if (file != null && !string.IsNullOrEmpty(file.Name) && System.IO.File.Exists(Path.Combine(path, file.Name)))
                            {
                                if (EditHelper.IsReadable(file)) countReadableFile++;
                                model.Files.Add(file); // if exist  add to model
                            }
                        }

                        // set flag to know if every file is readable or not
                        model.AllFilesReadable = cache.Files.Count == countReadableFile;
                    }
                    #endregion

                    model.IsDataValid = cache.IsDataValid;
                }

                return model;
            }
        }
    }
}