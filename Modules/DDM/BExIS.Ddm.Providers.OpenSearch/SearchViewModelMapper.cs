using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Providers.OpenSearch.Config;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Utils.Models;
using DocumentFormat.OpenXml.Bibliography;

namespace BExIS.Ddm.Providers.OpenSearch
{
    public static class SearchViewModelMapper
    {
        public static SearchAttribute ToSearchAttribute(int idx, GlobalComponent globalComponent, LocalComponent localComponent, SearchComponentBaseType componentType, long entityTemplateId, string entityTemplateName)
        {
            return new SearchAttribute
            {
                id = idx,
                displayName = globalComponent.ComponentName,
                sourceName = globalComponent.ComponentName,
                searchType = componentType,
                headerItem = globalComponent.HeaderItem,
                defaultHeaderItem = globalComponent.DefaultHeaderItem,
                placeholder = globalComponent.Placeholder,

                // local Components
                dataType = ToTypeCode(localComponent.DataTypeId),
                metadataName = string.Join(",", localComponent.MetadataNodes),
                EntityTemplateId = entityTemplateId,
                EntityTemplateName = entityTemplateName,

            };
        }

        public static SearchConfig ToSearchConfig(List<SearchAttribute> searchAttributeList)
        {
            var searchConfig = new SearchConfig();
            GlobalConfig global = new GlobalConfig();
            global.SearchComponents = new GlobalSearchComponent();

            foreach (SearchAttribute searchAttribute in searchAttributeList)
            {
                // check if already exists
                foreach (var localComp in searchConfig.Local)
                {
                    if (searchAttribute.searchType.Equals(SearchComponentBaseType.Facet))
                    {
                        //foreach (LocalConfig localFacet in localComp.SearchComponents.Facets)
                        //{

                        //}
                    }

                }

                LocalConfig local = new LocalConfig();

                if (searchAttribute.searchType.Equals(SearchComponentBaseType.Facet))
                {
                    var globalComp = new GlobalComponent {
                        Id = searchAttribute.id,
                        ComponentName = searchAttribute.displayName,
                        Placeholder = searchAttribute.placeholder,
                        DefaultHeaderItem = searchAttribute.defaultHeaderItem,
                        HeaderItem = searchAttribute.headerItem,
                    };

                    local.EntityTemplateId = 00;
                    local.SearchComponents.Facets.Add(new LocalComponent
                    {
                        GlobalId = globalComp.Id,
                        DataTypeId = FromTypeCode(searchAttribute.dataType),
                        MetadataNodes = searchAttribute.metadataName
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToList()
                    });

                    global.SearchComponents.Facets.Add(globalComp);
                    searchConfig.Local.Add(local);


                }
                else if (searchAttribute.searchType.Equals(SearchComponentBaseType.Category))
                {
                    global.SearchComponents.Categories.Add(new GlobalComponent
                    {
                        Id = searchAttribute.id,
                        ComponentName = searchAttribute.displayName,
                        Placeholder = searchAttribute.placeholder,
                        DefaultHeaderItem = searchAttribute.defaultHeaderItem,
                        HeaderItem = searchAttribute.headerItem,
                    });
                }
                else if (searchAttribute.searchType.Equals(SearchComponentBaseType.General))
                {
                    global.SearchComponents.General.Add(new GlobalComponent
                    {
                        Id = searchAttribute.id,
                        ComponentName = searchAttribute.displayName,
                        Placeholder = searchAttribute.placeholder,
                        DefaultHeaderItem = searchAttribute.defaultHeaderItem,
                        HeaderItem = searchAttribute.headerItem,
                    });
                }
                else if (searchAttribute.searchType.Equals(SearchComponentBaseType.Property))
                {
                    global.SearchComponents.Properties.Add(new GlobalComponent
                    {
                        Id = searchAttribute.id,
                        ComponentName = searchAttribute.displayName,
                        Placeholder = searchAttribute.placeholder,
                        DefaultHeaderItem = searchAttribute.defaultHeaderItem,
                        HeaderItem = searchAttribute.headerItem,
                    });
                }
                else
                {
                    throw new InvalidEnumArgumentException("Search component base type is not supported: " + searchAttribute.searchType);

                }
            }



            return searchConfig;
        }

        /// <summary>
        /// Convert Enum DataTypeId (OpenSearch supported datatypes) into TypeCode (whatever that's good for)
        /// </summary>
        /// <param name="dataTypeId"></param>
        /// <returns></returns>
        public static TypeCode ToTypeCode(DataTypeId dataTypeId)
        {
            switch (dataTypeId)
            {
                case DataTypeId.Byte:
                    return TypeCode.Byte;

                case DataTypeId.Short:
                    return TypeCode.Int16;

                case DataTypeId.Integer:
                    return TypeCode.Int32;

                case DataTypeId.Long:
                    return TypeCode.Int64;

                case DataTypeId.Float:
                case DataTypeId.HalfFloat:
                    return TypeCode.Single;

                case DataTypeId.Double:
                    return TypeCode.Double;

                case DataTypeId.ScaledFloat:
                    return TypeCode.Decimal;

                case DataTypeId.Boolean:
                    return TypeCode.Boolean;

                case DataTypeId.Date:
                case DataTypeId.DateNanos:
                    return TypeCode.DateTime;

                case DataTypeId.Text:
                case DataTypeId.Keyword:
                case DataTypeId.Ip:
                case DataTypeId.Version:
                    return TypeCode.String;

                case DataTypeId.GeoPoint:
                case DataTypeId.GeoShape:
                case DataTypeId.Object:
                case DataTypeId.Nested:
                case DataTypeId.Binary:
                    return TypeCode.Object;

                default:
                    return TypeCode.Object;
            }
        }

        public static DataTypeId FromTypeCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Byte:
                    return DataTypeId.Byte;

                case TypeCode.Int16:
                    return DataTypeId.Short;

                case TypeCode.Int32:
                    return DataTypeId.Integer;

                case TypeCode.Int64:
                    return DataTypeId.Long;

                case TypeCode.Single:
                    return DataTypeId.Float; // HalfFloat wird vernachlässigt

                case TypeCode.Double:
                    return DataTypeId.Double;

                case TypeCode.Decimal:
                    return DataTypeId.ScaledFloat;

                case TypeCode.Boolean:
                    return DataTypeId.Boolean;

                case TypeCode.DateTime:
                    return DataTypeId.Date; // DateNanos wird vernachlässigt

                case TypeCode.String:
                    return DataTypeId.Text; // Keyword/Ip/Version werden hier als Text abgebildet

                case TypeCode.Object:
                default:
                    return DataTypeId.Object; // Alle komplexen Typen
            }
        }




    }
}
