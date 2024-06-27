using BExIS.Dim.Entities.Publications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Models
{
    public class CreatePublicationModel
    {
        public long BrokerId { get; set; }
        public string Status { get; set; }

    }

    public class UpdatePublicationModel
    {
        public long Id { get; set; }

        public static UpdatePublicationModel Convert(Publication publication)
        {
            return new UpdatePublicationModel()
            {
                Id = publication.Id
            };
        }
    }

    public class ReadPublicationModel
    {
        public long Id { get; set; }

        public static ReadPublicationModel Convert(Publication publication)
        {
            return new ReadPublicationModel()
            {
                Id = publication.Id
            };
        }
    }
}