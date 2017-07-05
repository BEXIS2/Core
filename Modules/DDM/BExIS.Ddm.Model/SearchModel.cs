using System.Collections.Generic;

/// <summary>
///
/// </summary>        
namespace BExIS.Utils.Models
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class SearchModel
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public SearchComponent SearchComponent { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public SearchCriteria CriteriaComponent { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public SearchResult ResultComponent { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public SearchModel()
        {
            this.SearchComponent = new SearchComponent();
            this.CriteriaComponent = new SearchCriteria();
            this.ResultComponent = new SearchResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="typeOf"></param>
        /// <param name="multiSelect"></param>
        /// <param name="range"></param>
        /// <param name="valueSearchOperation"></param>
        public void UpdateSearchCriteria(string name, string value, SearchComponentBaseType typeOf, bool multiSelect = false, bool range = false, string valueSearchOperation = "OR")
        {
            //if (value != "" && value != null)
            if (value != null)
            {
                this.CriteriaComponent.Update(this.SearchComponent.GetSearchComponent(name, typeOf), value, multiSelect, valueSearchOperation);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <param name="values"></param>
        /// <param name="typeOf"></param>
        /// <param name="multiSelect"></param>
        /// <param name="range"></param>
        /// <param name="valueSearchOperation"></param>
        public void UpdateSearchCriteria(string name, List<string> values, SearchComponentBaseType typeOf, bool multiSelect = false, bool range = false, string valueSearchOperation = "OR")
        {

            this.CriteriaComponent.Update(this.SearchComponent.GetSearchComponent(name, typeOf), values, multiSelect, valueSearchOperation);
        }


    }
}
