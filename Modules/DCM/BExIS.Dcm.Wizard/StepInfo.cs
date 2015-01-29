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
    public class StepInfo
    {

        public string title;
        public bool valid = false;
        public StepStatus stepStatus;
        public bool notExecuted = true;
        public Dictionary<string, object> parameters;

        public bool Root { get; set;}
        public bool IsInstanze { get; set; }
        public int Id { get; set; }
        public StepInfo Parent { get; set; }
        public List<StepInfo> Children { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public ActionInfo GetActionInfo { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public ActionInfo PostActionInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="title"></param>            
        public StepInfo(string title)
        {
            this.title = title;
            this.stepStatus = StepStatus.none;
            this.GetActionInfo = new ActionInfo();
            this.PostActionInfo = new ActionInfo();
            this.Children = new List<StepInfo>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param> 
        /// <returns></returns>
        public Tuple<string, StepStatus> GetStatusWithName()
        {
            return new Tuple<string, StepStatus>(title, stepStatus);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="status"></param>
        public void SetStatus(StepStatus status)
        {
            this.stepStatus = status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public bool IsEmpty()
        {
            if (String.IsNullOrEmpty(title)) return true;
            else return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>   
        /// <returns></returns>
        public bool IsValid()
        {
            return valid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="valid"></param>
        public void SetValid(bool valid)
        {
            this.valid = valid;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum StepStatus
    { 
        none,
        success,
        error,
        exit,
        inProgress,
    }
}
