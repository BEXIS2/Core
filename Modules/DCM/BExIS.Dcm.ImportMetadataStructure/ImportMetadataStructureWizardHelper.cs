using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Dcm.ImportMetadataStructureWizard
{
    public class ImportMetadataStructureWizardHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetExtentionList()
        {
            return new List<string>()
            {
                ".xsd"

            };
        }
    }
}
