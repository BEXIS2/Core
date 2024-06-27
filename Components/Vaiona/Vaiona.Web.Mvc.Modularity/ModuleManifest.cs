using System.Xml.Linq;

namespace Vaiona.Web.Mvc.Modularity
{
    public class ModuleManifest
    {
        private XElement xManifest; //maybe not needed

        public ModuleManifest(string manifestPath) :
            this(XElement.Load(manifestPath))
        {
            // nothing is needed.
        }

        public ModuleManifest(XElement manifestElement)
        {
            xManifest = manifestElement;
            // poplulate the manifest property using the manifest and catalog information
            Name = xManifest.Attribute("moduleId").Value;
            Version = xManifest.Attribute("version").Value;
            Builtin = bool.Parse(xManifest.Attribute("builtin").Value);

            Description = xManifest.Element("Description").Value;
        }

        public XElement ManifestDoc
        { get { return xManifest; } }

        public string Version { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public bool Builtin { get; set; }
    }
}