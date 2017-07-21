using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.SAM.Models
{
    public class PartyAccount
    {
        public AccountRegisterModel AccountRegisterModel { get; set; }
        public IEnumerable<BExIS.Dlm.Entities.Party.PartyType> PartyTypes { get; set; }
        public IEnumerable<BExIS.Dlm.Entities.Party.PartyCustomAttributeValue> PartyCustomAttributeValues { get; set; }
    }
}