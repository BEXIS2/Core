using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Models
{
    public class CreateJwtModel
    {
        public int Validity { get; set; }
    }

    public class ReadJwtModel
    {
        public string Jwt { get; set; }
    }
}