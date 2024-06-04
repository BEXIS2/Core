using System;
using System.Collections.Generic;

/// <summary>
///
/// </summary>
namespace BExIS.Dcm.Wizard
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class ActionInfo
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string AreaName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string ControllerName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string ActionName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public Dictionary<String, object> Parameters { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public ActionInfo()
        {
            AreaName = "";
            ControllerName = "";
            ActionName = "";
            Parameters = new Dictionary<String, object>();
        }

        /// <summary>
        /// Get true back if all needed parameter are set
        /// </summary>
        /// <returns></returns>
        public bool IsComplete()
        {
            //if (String.IsNullOrEmpty(AreaName))
            //    return false;

            if (String.IsNullOrEmpty(ControllerName))
                return false;

            if (String.IsNullOrEmpty(ActionName))
                return false;

            return true;
        }
    }
}