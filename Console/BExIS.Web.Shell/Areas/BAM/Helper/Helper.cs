using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using BExIS.Modules.Bam.UI.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data;
using BExIS.Security.Entities.Authorization;
using System.Diagnostics.Contracts;
using BExIS.Security.Services.Subjects;

namespace BExIS.Modules.Bam.UI.Helpers
{
    public static class forDataPivot
    {
        public static DataTable ToPivotTable<T, TColumn, TRow, TData>(
      this IEnumerable<T> source,
      Func<T, TColumn> columnSelector,
      Expression<Func<T, TRow>> rowSelector,
      Func<IEnumerable<T>, TData> dataSelector)
        {
            DataTable table = new DataTable();
            var rowName = ((MemberExpression)rowSelector.Body).Member.Name;
            table.Columns.Add(new DataColumn(rowName));
            var columns = source.Select(columnSelector).Distinct();

            foreach (var column in columns)
                table.Columns.Add(new DataColumn(column.ToString()));

            var rows = source.GroupBy(rowSelector.Compile())
                             .Select(rowGroup => new
                             {
                                 Key = rowGroup.Key,
                                 Values = columns.GroupJoin(
                                     rowGroup,
                                     c => c,
                                     r => columnSelector(r),
                                     (c, columnGroup) => dataSelector(columnGroup))
                             });

            foreach (var row in rows)
            {
                var dataRow = table.NewRow();
                var items = row.Values.Cast<object>().ToList();
                items.Insert(0, row.Key);
                dataRow.ItemArray = items.ToArray();
                table.Rows.Add(dataRow);
            }

            return table;
        }

    }

    public class Helper
    {
        public static int CountRelations(long sourcePartyId, PartyRelationshipType partyRelationshipType)
        {
            PartyManager partyManager = null;
            try
            {
                partyManager = new PartyManager();
                var cnt = partyManager.PartyRelationshipRepository.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationshipType.Id)
                                          && (item.SourceParty != null && (item.SourceParty.Id == sourcePartyId) || (item.TargetParty.Id == sourcePartyId))
                                           && (item.EndDate >= DateTime.Now)).Count();
                return cnt;
            }
            finally { partyManager?.Dispose(); }
        }

        internal static DataTable getPartyDataTable(PartyType partyType, List<Party> parties)
        {
            PartyRelationshipTypeManager partyRelationshipTypeManager = null;
            PartyManager partyManager = null;
            try
            {
                partyRelationshipTypeManager = new PartyRelationshipTypeManager();
                partyManager = new PartyManager();
                DataTable table = new DataTable();
                table.Columns.Add("PartyId");
                table.Columns.Add("PartyName");
                table.Columns.Add("PartyTypeTitle");
                table.Columns.Add("StartDate");
                table.Columns.Add("EndDate");
                table.Columns.Add("IsTemp");
                var partyCustomGridColumnsRepository = partyManager.GetPartyCustomGridColumns(partyType.Id);
                foreach (var partyCustomGridColumn in partyCustomGridColumnsRepository)
                {
                    if (partyCustomGridColumn.CustomAttribute != null)
                        table.Columns.Add(partyCustomGridColumn.CustomAttribute.DisplayName.Replace(" ", "_"));
                    else
                        table.Columns.Add(partyCustomGridColumn.TypePair.Title.Replace(" ", "_"));

                }

                for (int i = 0; i < parties.Count(); i++)
                {
                    DataRow row = table.NewRow();
                    var party = parties[i];
                    //var pivotTable = forDataPivot.ToPivotTable(party.CustomAttributeValues,
                    //       item => item.CustomAttribute.Name,
                    //       item => item.Party.Id,
                    //       items => items.Any() ? items.Sum(x => x.VersionNo) : 0);

                    row["PartyId"] = party.Id;
                    row["PartyName"] = party.Name;
                    row["PartyTypeTitle"] = party.PartyType.DisplayName;
                    row["StartDate"] = (party.StartDate != null && party.StartDate < new DateTime(1000, 1, 1) ? "" : party.StartDate.ToShortDateString());
                    row["EndDate"] = (party.EndDate != null && party.EndDate > new DateTime(3000, 1, 1) ? "" : party.EndDate.ToShortDateString());
                    row["IsTemp"] = party.IsTemp;
                    foreach (var customAttributeValue in party.CustomAttributeValues)
                        if (partyCustomGridColumnsRepository.Any(cc => cc.CustomAttribute != null && (cc.CustomAttribute.Id == (customAttributeValue.CustomAttribute.Id))))
                            row[customAttributeValue.CustomAttribute.DisplayName.Replace(" ", "_")] = customAttributeValue.Value;
                    var partyRelationships = partyManager.PartyRelationshipRepository.Get(cc => (cc.SourceParty.Id == party.Id));
                    foreach (var partyRelationship in partyRelationships)
                    {
                        if (partyCustomGridColumnsRepository.Any(cc => cc.TypePair != null && cc.TypePair.Id == (partyRelationship.PartyTypePair.Id)))
                            row[partyRelationship.PartyTypePair.Title.Replace(" ", "_")] += partyRelationship.TargetParty.Name + "<br/>";
                    }

                    table.Rows.Add(row);
                }
                return table;
            }
            finally
            {
                partyManager.Dispose();
                partyRelationshipTypeManager.Dispose();
            }
        }

        public static string GetDisplayName(Dlm.Entities.Party.PartyRelationshipType partyRelatinshipType)
        {
            return (string.IsNullOrWhiteSpace(partyRelatinshipType.DisplayName) ? partyRelatinshipType.Title : partyRelatinshipType.DisplayName);
        }

        public static String ValidateRelationships(long partyId)
        {
            var partyManager = new PartyManager();
            var validations = partyManager.ValidateRelationships(partyId);
            string messages = "";
            foreach (var validation in validations)
            {
                messages += (String.Format("<br/>{0} relationship type '{1}'.", validation.Value, validation.Key.DisplayName));
            }
            if (!string.IsNullOrEmpty(messages))
                messages = "These relationship types are required : " + messages;
            return messages;
        }

        internal static Party EditParty(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues, IList<PartyRelationship> systemPartyRelationships)
        {
            PartyTypeManager partyTypeManager =null;
            PartyManager partyManager = null;
            PartyRelationshipTypeManager partyRelationshipTypeManager = null;
            var party = new Party();
            try
            {
                partyTypeManager = new PartyTypeManager();
                partyManager = new PartyManager();
                partyRelationshipTypeManager = new PartyRelationshipTypeManager();
                var newAddPartyCustomAttrValues = new Dictionary<PartyCustomAttribute, string>();
                party = partyManager.Find(partyModel.Id);
                //Update some fields
                party.Description = partyModel.Description;
                party.StartDate = partyModel.StartDate.HasValue ? partyModel.StartDate.Value : DateTime.MinValue;
                party.EndDate = partyModel.EndDate.HasValue ? partyModel.EndDate.Value : DateTime.MaxValue;
                party = partyManager.Update(party);
                foreach (var partyCustomAttributeValueString in partyCustomAttributeValues)
                {
                    PartyCustomAttribute partyCustomAttribute = partyTypeManager.PartyCustomAttributeRepository.Get(int.Parse(partyCustomAttributeValueString.Key));
                    string value = string.IsNullOrEmpty(partyCustomAttributeValueString.Value) ? "" : partyCustomAttributeValueString.Value;
                    newAddPartyCustomAttrValues.Add(partyCustomAttribute, value);
                }
                partyManager.AddPartyCustomAttributeValues(party, partyCustomAttributeValues.ToDictionary(cc => long.Parse(cc.Key), cc => cc.Value));
                //if (systemPartyRelationships != null)
                //{
                //    foreach (var systemPartyRel in systemPartyRelationships.Where(item => item.Id != long.MaxValue))
                //    {
                //        var firstParty = partyManager.PartyRepository.Reload(party);
                //        var secondParty = partyManager.PartyRepository.Get(systemPartyRel.TargetParty.Id);
                //        var partyTypePair = partyRelationshipTypeManager.PartyTypePairRepository.Get(systemPartyRel.PartyTypePair.Id);
                //        //update
                //        if (systemPartyRel.Id > 0)
                //            partyManager.UpdatePartyRelationship(systemPartyRel.Id, permission: systemPartyRel.Permission);
                //        else if (systemPartyRel.Id == 0)
                //            partyManager.AddPartyRelationship(firstParty, secondParty, "system", "", partyTypePair, permission: systemPartyRel.Permission);
                //        else {
                //            PartyRelationship partyRelationship = partyManager.PartyRelationshipRepository.Get(-1 * systemPartyRel.Id);
                //            //remove if id is negative
                //            partyManager.RemovePartyRelationship(partyRelationship);
                //        }
                //    }
                //}
            }
            finally
            {
                partyTypeManager?.Dispose();
                partyManager?.Dispose();
                partyRelationshipTypeManager?.Dispose();
            }
            return party;
        }
        internal static Party CreateParty(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues)
        {
            return CreateParty(partyModel.StartDate, partyModel.EndDate, partyModel.Description, partyModel.PartyType.Id, partyCustomAttributeValues);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="description"></param>
        /// <param name="partyTypeId"></param>
        /// <param name="partyCustomAttributeValues">CustomAttributeName or Id as key</param>
        /// <returns></returns>
        internal static Party CreateParty(DateTime? startDate, DateTime? endDate, string description, long partyTypeId, Dictionary<string, string> partyCustomAttributeValuesDict)
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();
            PartyRelationshipTypeManager partyRelationshipTypeManager = null;
            var newParty = new Party();
            try
            {
                partyTypeManager = new PartyTypeManager();
                partyManager = new PartyManager();
                partyRelationshipTypeManager = new PartyRelationshipTypeManager();
                PartyType partyType = partyTypeManager.PartyTypeRepository.Get(partyTypeId);
                var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
                // save party as temp if the reationships are required
                var requiredPartyRelationTypes = new PartyRelationshipTypeManager().GetAllPartyRelationshipTypes(partyType.Id).Where(cc => cc.MinCardinality > 0);
                //Create party
                newParty = partyManager.Create(partyType, "", description, startDate, endDate, partyStatusType, requiredPartyRelationTypes.Any());
                partyManager.AddPartyCustomAttributeValues(newParty, toPartyCustomAttributeValues(partyCustomAttributeValuesDict, partyTypeId));
                // partyManager.AddPartyRelationship(null,secondpartyId,PartyTypePairid)
                //var systemPartyTypePairs = GetSystemTypePairs(newParty.PartyType.Id);
                ////add relationship to the all targets
                //foreach (var systemPartyTypePair in systemPartyTypePairs)
                //{
                //    foreach (var targetParty in systemPartyTypePair.TargetType.Parties)
                //    {
                //        PartyTypePair partyTypePair = partyRelationshipTypeManager.PartyTypePairRepository.Reload(systemPartyTypePair);
                //        partyManager.AddPartyRelationship(partyManager.PartyRepository.Reload(newParty), targetParty,  "system", "", systemPartyTypePair, permission: systemPartyTypePair.PermissionTemplate);
                //    }
                //}
            }
            finally
            {
                partyTypeManager?.Dispose();
                partyManager?.Dispose();
            }
            return newParty;
        }
        internal static Dictionary<PartyCustomAttribute, string> toPartyCustomAttributeValues(Dictionary<string, string> partyCustomAttributeValuesDict, long partyTypeId = 0)
        {
            var partyCustomAttributeValues = new Dictionary<PartyCustomAttribute, string>();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            try
            {
                foreach (var partyCustomAttributeValueDict in partyCustomAttributeValuesDict)
                {
                    long id = 0;
                    var partyCustomAttribute = new PartyCustomAttribute();
                    if (!long.TryParse(partyCustomAttributeValueDict.Key, out id))
                    {
                        if (partyTypeId == 0)
                            throw new Exception("Party type id should not be zero when you use custom attribute name");
                        partyCustomAttribute = partyTypeManager.PartyCustomAttributeRepository.Get(cc => cc.Name.ToLower() == partyCustomAttributeValueDict.Key.ToLower() && cc.PartyType.Id == partyTypeId).FirstOrDefault();
                        if (partyCustomAttribute == null)
                            throw new Exception(partyCustomAttributeValueDict.Key + " is not exist in custom attributes.");
                    }
                    else
                        partyCustomAttribute = partyTypeManager.PartyCustomAttributeRepository.Get(id);
                    if (partyCustomAttribute == null)
                        throw new Exception("No  custom attributes with this Id " + id);
                    partyCustomAttributeValues.Add(partyCustomAttribute, partyCustomAttributeValueDict.Value);
                }
                return partyCustomAttributeValues;
            }
            finally { partyTypeManager?.Dispose(); }
        }

        /// <summary>
        /// convert string to a number for permission
        /// </summary>
        /// <param name="permissionsTemplate"></param>
        /// <returns></returns>
        internal static int GetPermissionValue(string permissionsTemplate)
        {
            int value = 0;
            foreach (var permissionTemplate in permissionsTemplate.Split(','))
            {
                if (!string.IsNullOrWhiteSpace(permissionTemplate))
                    value += ((int)Enum.Parse(typeof(RightType), permissionTemplate.Trim(), true));
            }
            return value;
        }
        public static IList<PartyTypePair> GetSystemTypePairs(long partyTypeId)
        {
            PartyRelationshipTypeManager partTypeManager = new PartyRelationshipTypeManager();
            try
            {
                var typePairs = partTypeManager.PartyTypePairRepository.Get(cc => cc.SourceType.Id == partyTypeId && cc.TargetType.SystemType);
                return typePairs;
            }
            finally
            {
                partTypeManager?.Dispose();
            }
        }
        
        public static RightType[] GetRightTypes(int permissions)
        {
            var rightTypes = new List<RightType>();
            foreach (RightType rightType in Enum.GetValues(typeof(RightType)))
            {
                if ((permissions & (int)rightType) > 0)
                    rightTypes.Add(rightType);
            }
            return rightTypes.ToArray();
        }
    }
}