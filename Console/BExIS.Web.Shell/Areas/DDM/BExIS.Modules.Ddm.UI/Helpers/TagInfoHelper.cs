using BExIS.Dlm.Entities.Data;
using BExIS.Modules.Ddm.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Ddm.UI.Helpers
{
    public class TagInfoHelper
    {
        public List<TagInfoModel> ConvertTo(List<DatasetVersion> versions)
        {
            List<TagInfoModel> models = new List<TagInfoModel>();

            foreach (var version in versions)
            {
                models.Add(ConvertTo(version));
            }

            return models;
        }

        public TagInfoModel ConvertTo(DatasetVersion dsv)
        {
            TagInfoModel model = new TagInfoModel();

            if (dsv != null)
            {
                model.VersionId = dsv.Id;
                model.VersionNr = dsv.VersionNo;
                model.ReleaseNote = dsv.ChangeDescription;
                model.SystemDescription = dsv.ModificationInfo?.Comment;
                model.SystemAuthor = dsv.ModificationInfo?.Performer;
                model.SystemDate = (DateTime)dsv.ModificationInfo?.Timestamp;


                if (dsv.Tag != null)
                {
                    model.TagId = dsv.Tag.Id;
                    model.TagNr = dsv.Tag.Nr;
                    model.ReleaseDate = dsv.Tag.ReleaseDate;
                    model.Show = dsv.Tag.Show;
                    model.Publish = dsv.Tag.Final;
                }
            }

            return model;
        }

        public Tag Update(TagInfoModel model, Tag tag)
        { 
            if (model != null && tag != null)
            { 
               tag.Nr = model.TagNr;
               tag.ReleaseDate = model.ReleaseDate;
               tag.Show = model.Show;
               tag.Final = model.Publish;
            }

            return tag;
        }
    }
}