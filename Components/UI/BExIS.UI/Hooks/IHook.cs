using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.UI.Hooks
{
    public interface IHook
    {
        /// <summary>
        /// This function checks if the existing information about an entity changes the
        /// hookStatus or the start action of the hook.
        /// </summary>
        void Check(long datasetId, string username);

        /// <summary>
        ///
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        bool UpdateCache(params object[] arguments);
    }
}