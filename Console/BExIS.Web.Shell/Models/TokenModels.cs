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