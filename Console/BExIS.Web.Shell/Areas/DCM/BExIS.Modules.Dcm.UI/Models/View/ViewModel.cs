using BExIS.Dim.Helpers.Models;
using BExIS.Modules.Dim.UI.Models.Download;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BExIS.Modules.Dcm.UI.Models.View
{
    public class ViewModel:ApiDatasetModel
    {
        public ViewSettings Settings { get; set; }
        public string EntityName { get; set; }
        public bool HasData { get; set; }
        public int Count { get; set; }
        public bool IsValid { get; set; }
        public bool DownloadAccess { get; set; }
        public bool RequestExist { get; set; }
        public bool RequestAble { get; set; }
        public bool HasRequestRight { get; set; }
        public bool HasEditRight { get; set; }


        public Dictionary<string, string> Labels { get; set; }

        public ViewModel()
        {
            Id = 0;
            Version = 0;
            VersionId = 0;
            Tag = 0;
            Title = "";
            HasData = false;
            HasEditRight = false;
            Labels = new Dictionary<string, string>();
            Settings = new ViewSettings();
        }

        public static ViewModel Map(ApiDatasetModel source)
        {
            ViewModel target = new ViewModel();

            // 1. Hole alle öffentlichen Instanz-Eigenschaften des Quell-Typs
            PropertyInfo[] sourceProperties = typeof(ApiDatasetModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 2. Durchlaufe die Eigenschaften in einer Schleife
            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                // 3. Suche die entsprechende Eigenschaft im Ziel-Typ
                PropertyInfo targetProperty = typeof(GeneralMetadataModel).GetProperty(sourceProperty.Name);

                // 4. Überprüfe, ob die Eigenschaft existiert und gesetzt werden kann
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    // 5. Überprüfe, ob die Typen übereinstimmen (empfohlen)
                    if (targetProperty.PropertyType == sourceProperty.PropertyType)
                    {
                        // 6. Lese den Wert aus der Quelle
                        object value = sourceProperty.GetValue(source);

                        // 7. Setze den Wert im Ziel
                        targetProperty.SetValue(target, value);
                    }
                }
            }

            return target;
        }
    }

    public class ViewSettings
    {
        public bool UseTags { get; set; } // use tags, e.g., 1.0, 2.0, 3.0
        public bool UseMinor { get; set; } // use minor tags, e.g., 1.1, 1.2, 1.3
        public string DataAggrement { get; set; }

        public string Entity { get; set; }

        public List<Hook> Hooks { get; set; }

        public ViewSettings()
        {
            UseTags = false;
            UseMinor = false;
            Hooks = new List<Hook>();
            Entity = "";
        }
    }

    public class TagInfoViewModel
    {
        [JsonProperty("version")]
        public double Version { get; set; }
        [JsonProperty("releaseNotes")]
        public List<string> ReleaseNotes { get; set; }
        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }

        public TagInfoViewModel()
        {
            Version = 0;
            ReleaseNotes = new List<string>();
        }

    }

    public class TagInfoEditModel
    {
        [JsonProperty("versionId")]
        public long VersionId { get; set; }

        [JsonProperty("versionNr")]
        public long VersionNr { get; set; }


        [JsonProperty("releaseNote")]
        public string ReleaseNote { get; set; }
        [JsonProperty("show")]
        public bool Show { get; set; }

        [JsonProperty("tagId")]
        public long TagId { get; set; }

        [JsonProperty("tagNr")]
        public double TagNr { get; set; }

        [JsonProperty("publish")]
        public bool Publish { get; set; }

        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }
        [JsonProperty("systemDescription")]
        public string SystemDescription { get; set; }
        [JsonProperty("systemAuthor")]
        public string SystemAuthor { get; set; }
        [JsonProperty("systemDate")]
        public DateTime SystemDate { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        public TagInfoEditModel()
        {
            VersionId = 0;
            VersionNr = 0;
            TagId = 0;
            TagNr = 0;
            ReleaseNote = "";
            SystemDescription = "";
            SystemAuthor = "";
            Link = "";
        }

        public TagInfoEditModel(long versionId, long versionNr, double tagNr, string releaseNote, DateTime releaseDate, string systemDescription, string systemAuthor, DateTime systemDate, string link)
        {
            VersionId = versionId;
            VersionNr = versionNr;
            TagNr = tagNr;
            ReleaseNote = releaseNote;
            ReleaseDate = releaseDate;
            if (!string.IsNullOrEmpty(systemAuthor)) SystemAuthor = systemAuthor;
            if (!string.IsNullOrEmpty(systemDescription)) SystemAuthor = systemDescription;
            SystemDate = systemDate;
            Link = link;
        }
    }

    public class VersionListeItem : ListItem
    {
        public string Date { get; set; }
    }

    public class DeletedModel
    {
        public long Id { get; set; }
        public string Title { get; set; }

        public LinksOverview Links { get; set; }
        
        public DeletedModel()
        {
            Id = 0;
            Title = "";
            Links = new LinksOverview();
        }
    }

    public class AttachtmentsViewModel
    {
        public long Id { get; set; }
        
        public List<FileInfo> Files { get; set; }
    }

}