using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Search.Model;

namespace BExIS.Search.Providers.DummyProvider.Helpers.Helpers.Search
{
    public class ResultObjectBuilder
    {
        public static SearchResult ConvertListOfMetadataToSearchResultObject(List<Metadata> metadataList, int pageSize,int currentPage){

            SearchResult sro = new SearchResult();
            
            sro.PageSize = pageSize;
            sro.CurrentPage = currentPage;
            sro.NumberOfHits = metadataList.Count();

            List<HeaderItem> Header = new List<HeaderItem>();
            List<HeaderItem> DefaultHeader = new List<HeaderItem>();
            List<Row> RowList = new List<Row>();

            foreach (Metadata m in metadataList)
            {
                if(metadataList.First()==m)
                {
         
                    HeaderItem hi = new HeaderItem();

                    hi = new HeaderItem();
                    hi.Name = m.GetValueFromNode("datasetid").Item(0).Name;
                    hi.DisplayName = hi.Name.Titleize();
                    Header.Add(hi);
                    DefaultHeader.Add(hi);
                    sro.Id = hi;

                    hi = new HeaderItem();
                    hi.Name = m.GetValueFromNode("title").Item(0).Name;
                    hi.DisplayName = hi.Name.Titleize();
                    Header.Add(hi);
                    DefaultHeader.Add(hi);

                    hi = new HeaderItem();
                    hi.Name = m.GetValueFromNode("owner").Item(0).Name;
                    hi.DisplayName = hi.Name.Titleize();
                    Header.Add(hi);
                    DefaultHeader.Add(hi);

                    hi = new HeaderItem();
                    hi.Name = "PrimaryData";
                    hi.DisplayName = "Primary Data";
                    Header.Add(hi);
                    DefaultHeader.Add(hi);
                    
                    hi = new HeaderItem();
                    hi.Name = m.GetValueFromNode("versionID").Item(0).Name;
                    hi.DisplayName = hi.Name.Titleize();
                    Header.Add(hi);
                    DefaultHeader.Add(hi);

                    hi = new HeaderItem();
                    hi.Name = m.GetValueFromNode("fileType").Item(0).Name;
                    hi.DisplayName = hi.Name.Titleize();
                    Header.Add(hi);

                    hi = new HeaderItem();
                    hi.Name = m.GetValueFromNode("dateLastModified").Item(0).Name;
                    hi.DisplayName = hi.Name.Titleize();
                    Header.Add(hi);

                    hi = new HeaderItem();
                    hi.Name = m.GetValueFromNode("projectLeader").Item(0).Name;
                    hi.DisplayName = hi.Name.Titleize();
                    Header.Add(hi);

                    hi = new HeaderItem();
                    hi.Name = m.GetValueFromNode("dateEntry").Item(0).Name;
                    hi.DisplayName = hi.Name.Titleize();
                    Header.Add(hi);

                    hi = new HeaderItem();
                    hi.Name = m.GetValueFromNode("qualityLevel").Item(0).Name;
                    hi.DisplayName = hi.Name.Titleize();
                    Header.Add(hi);

                    hi = new HeaderItem();
                    hi.Name = m.GetValueFromNode("dataStatus").Item(0).Name;
                    hi.DisplayName = hi.Name.Titleize();
                    Header.Add(hi);
                }
               


                Row r = new Row();
                List<object> ValueList = new List<object>();

                   ValueList = new List<object>();
                   ValueList.Add(Convert.ToInt32(m.GetValueFromNode("datasetid").Item(0).InnerText));
                   ValueList.Add(m.GetValueFromNode("title").Item(0).InnerText);
                   ValueList.Add(m.GetValueFromNode("owner").Item(0).InnerText);
                   ValueList.Add("YES");
                   ValueList.Add(m.GetValueFromNode("versionID").Item(0).InnerText);
                   ValueList.Add(m.GetValueFromNode("fileType").Item(0).InnerText);
                   ValueList.Add(m.GetValueFromNode("dateLastModified").Item(0).InnerText);
                   ValueList.Add(m.GetValueFromNode("projectLeader").Item(0).InnerText);
                   ValueList.Add(m.GetValueFromNode("dateEntry").Item(0).InnerText);
                   ValueList.Add(m.GetValueFromNode("qualityLevel").Item(0).InnerText);
                   ValueList.Add(m.GetValueFromNode("dataStatus").Item(0).InnerText);

                   r.Values = ValueList;

                   r.PreviewItem = GetPrevieItem();

                 RowList.Add(r);
            }

            sro.Header = Header;
            sro.Rows = RowList;
            sro.DefaultVisibleHeaderItem = DefaultHeader;
            sro.PageSize = pageSize;
            sro.CurrentPage = currentPage;

            return sro;
        }

        private static SearchResultPreview GetPrevieItem()
        {
            SearchResultPreview srp = new SearchResultPreview();

            List<HeaderItem> Header = new List<HeaderItem>();
            List<Row> RowList = new List<Row>();

            HeaderItem hi;

            hi = new HeaderItem();
            hi.Name = "VariableName";
            Header.Add(hi);

            hi = new HeaderItem();
            hi.Name = "VariableParameter";
            Header.Add(hi);

            hi = new HeaderItem();
            hi.Name = "Unit";
            Header.Add(hi);

            hi = new HeaderItem();
            hi.Name = "Description";
            Header.Add(hi);



            //-----------------------------------------
            Row r = new Row();
            List<object> ValueList = new List<object>();

            ValueList.Add("Species");
            ValueList.Add("No");
            ValueList.Add("No");
            ValueList.Add("Name of species");
            r.Values = ValueList;

            RowList.Add(r);

            //-----------------------------------------
            r = new Row();
            ValueList = new List<object>();

            ValueList.Add("Plot");
            ValueList.Add("No");
            ValueList.Add("No");
            ValueList.Add("Plot ID");
            r.Values = ValueList;

            RowList.Add(r);

            //-----------------------------------------

            r = new Row();
            ValueList = new List<object>();

            ValueList.Add("Temperature");
            ValueList.Add("No");
            ValueList.Add("°C");
            ValueList.Add("-");
            r.Values = ValueList;

            RowList.Add(r);

            //-----------------------------------------

            r = new Row();
            ValueList = new List<object>();

            ValueList.Add("Date");
            ValueList.Add("No");
            ValueList.Add("No");
            ValueList.Add("Date of Observation");
            r.Values = ValueList;

            RowList.Add(r);

            //-----------------------------------------

            r = new Row();
            ValueList = new List<object>();

            ValueList.Add("Female perc");
            ValueList.Add("No");
            ValueList.Add("%");
            ValueList.Add("Female species in %");
            r.Values = ValueList;

            RowList.Add(r);

            //-----------------------------------------
            r = new Row();
            ValueList = new List<object>();

            ValueList.Add("Male perc");
            ValueList.Add("No");
            ValueList.Add("%");
            ValueList.Add("Male species in %");
            r.Values = ValueList;

            RowList.Add(r);

            //-----------------------------------------

            r = new Row();
            ValueList = new List<object>();

            ValueList.Add("Male perc");
            ValueList.Add("No");
            ValueList.Add("%");
            ValueList.Add("Male species in %");
            r.Values = ValueList;

            RowList.Add(r);

            //-----------------------------------------
             r = new Row();
            ValueList = new List<object>();

            ValueList.Add("Season");
            ValueList.Add("No");
            ValueList.Add("No");
            ValueList.Add("Define season like summer");
            r.Values = ValueList;

            RowList.Add(r);

            
            //-----------------------------------------
             r = new Row();
            ValueList = new List<object>();

            ValueList.Add("Weather");
            ValueList.Add("No");
            ValueList.Add("No");
            ValueList.Add("Define weather like rainy day");
            r.Values = ValueList;

            RowList.Add(r);

            //-----------------------------------------


            srp.Header = Header;
            srp.Rows = RowList;

            return srp;
        }

    }
}
