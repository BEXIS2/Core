namespace BExIS.Utils.Models
{
    //ToDo set this enitiy to a other place because of reading a metadata structure ad create a list of all simple attribute with xpath and displayname is also important for other places as search designer
    public class SearchMetadataNode
    {
        public SearchMetadataNode(string metadataStructureName, string xPath)
        {
            this.MetadataStructureName = metadataStructureName;
            this.XPath = xPath;
            DisplayName = generateDisplayName(xPath);
            DisplayNameLong = "(" + MetadataStructureName + ") " + generateDisplayName(xPath);
        }

        public string DisplayName { get; set; }
        public string DisplayNameLong { get; set; }
        public string MetadataStructureName { get; set; }
        public string XPath { get; set; }

        private string generateDisplayName(string xPath)
        {
            string tmp = "";
            string[] tempArray = xPath.Split('/');

            for (int i = 1; i < tempArray.Length; i = i + 2)
            {
                tmp += tempArray[i] + "/";
            }

            return tmp.Remove(tmp.Length - 1);
        }
    }
}