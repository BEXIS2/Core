using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class TestModel
    {
        [Remote("RemoteTest", "SubmitSpecifyDataset")]
        [Required]
        [StringLength(50, ErrorMessage = "The user name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string Username { get; set; }
    }
}