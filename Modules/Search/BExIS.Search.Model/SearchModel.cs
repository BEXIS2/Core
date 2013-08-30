using System.Collections.Generic;

namespace BExIS.Search.Model
{
    public class SearchModel
    {
        public SearchComponent SearchComponent { get; set; }
        public SearchCriteria CriteriaComponent { get; set; }
        public SearchResult ResultComponent { get; set; }

        public SearchModel()
        {
            this.SearchComponent = new SearchComponent();
            this.CriteriaComponent = new SearchCriteria();
            this.ResultComponent = new SearchResult();
        }

        public void UpdateSearchCriteria(string name, string value, SearchComponentBaseType typeOf, bool multiSelect = false, bool range = false, string valueSearchOperation = "OR")
        {
            if (value != "" && value !=null)
            {
                this.CriteriaComponent.Update(this.SearchComponent.GetSearchComponent(name, typeOf), value, multiSelect, valueSearchOperation);
            }
        }

        public void UpdateSearchCriteria(string name, List<string> values,SearchComponentBaseType typeOf, bool multiSelect = false, bool range = false, string valueSearchOperation = "OR")
        {

            this.CriteriaComponent.Update(this.SearchComponent.GetSearchComponent(name, typeOf), values, multiSelect, valueSearchOperation);
        }


    }
}
