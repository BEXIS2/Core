using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BExIS.Modules.Ddm.UI.Helpers
{
    public class TagInfoHelper
    {
        public Tag Update(TagInfoEditModel model, Tag tag)
        {
            if (model != null && tag != null)
            {
                tag.Nr = model.TagNr;
                tag.ReleaseDate = model.ReleaseDate;
                tag.Final = model.Publish;

                if (tag.Final && model.ReleaseDate == new DateTime())
                {
                    tag.ReleaseDate = DateTime.Now;
                }
            }

            return tag;
        }

        public List<TagInfoEditModel> ConvertTo(List<DatasetVersion> versions, DatasetManager datasetManager)
        {
            List<TagInfoEditModel> models = new List<TagInfoEditModel>();

            foreach (var version in versions)
            {
                models.Add(ConvertTo(version, datasetManager.GetDatasetVersionNr(version)));
            }

            return models;
        }

        public TagInfoEditModel ConvertTo(DatasetVersion dsv, int versionNr)
        {
            TagInfoEditModel model = new TagInfoEditModel();

            if (dsv != null)
            {
                model.VersionId = dsv.Id;
                model.VersionNr = versionNr;
                model.ReleaseNote = dsv.ChangeDescription;
                model.SystemDescription = dsv.ModificationInfo?.Comment;
                model.SystemAuthor = dsv.ModificationInfo?.Performer;
                model.Show = dsv.Show;

                if(dsv.ModificationInfo?.Timestamp != null)
                    model.SystemDate = (DateTime)dsv.ModificationInfo?.Timestamp;


                if (dsv.Tag != null)
                {
                    model.TagId = dsv.Tag.Id;
                    model.TagNr = dsv.Tag.Nr;
                    model.ReleaseDate = dsv.Tag.ReleaseDate;
                    model.Publish = dsv.Tag.Final;
                }
            }

            return model;
        }



        public List<TagInfoViewModel> GetViews(List<DatasetVersion> versions, DatasetManager datasetManager)
        {
            List<TagInfoViewModel> models = new List<TagInfoViewModel>();

            List<double> tags = versions.Where(v=>v.Tag!=null).Select(v => v.Tag.Nr).Distinct().ToList();

            foreach (var nr in tags)
            {
                var tagVersions = versions.OrderByDescending(o=>o.Id).Where(v => v.Tag !=null && v.Tag.Nr.Equals(nr) && v.Show && v.Tag.Final).ToList();
                if (tagVersions.Any())
                {
                    models.Add(new TagInfoViewModel()
                    {
                        Version = nr,
                        ReleaseDate = tagVersions.FirstOrDefault().Tag.ReleaseDate,
                        ReleaseNotes = tagVersions.Select(v => v.ChangeDescription).ToList()
                    });
                }

            }

            return models;
        }


    }
}