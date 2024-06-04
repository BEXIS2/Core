using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Models;
using BExIS.Security.Entities.Authorization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

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
            using (PartyManager partyManager = new PartyManager())
            {
                var cnt = partyManager.PartyRelationshipRepository.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationshipType.Id)
                                            && (item.SourceParty != null && (item.SourceParty.Id == sourcePartyId) || (item.TargetParty.Id == sourcePartyId))
                                            && (item.EndDate >= DateTime.Now)).Count();
                return cnt;
            }
        }

        internal static DataTable getPartyDataTable(PartyType partyType, List<Party> parties)
        {
            using (PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager())
            using (PartyManager partyManager = new PartyManager())
            {
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
                    var partyRelationships = partyManager.PartyRelationshipRepository.Get(cc => (cc.SourceParty.Id == party.Id && !cc.PartyTypePair.TargetPartyType.SystemType));
                    foreach (var partyRelationship in partyRelationships)
                    {
                        if (partyCustomGridColumnsRepository.Any(cc => cc.TypePair != null && cc.TypePair.Id == (partyRelationship.PartyTypePair.Id)))
                            row[partyRelationship.PartyTypePair.Title.Replace(" ", "_")] += "[" + partyRelationship.TargetParty.Name + "] ";
                    }

                    table.Rows.Add(row);
                }
                return table;
            }
        }

        public static string GetDisplayName(Dlm.Entities.Party.PartyRelationshipType partyRelatinshipType)
        {
            return (string.IsNullOrWhiteSpace(partyRelatinshipType.DisplayName) ? partyRelatinshipType.Title : partyRelatinshipType.DisplayName);
        }

        public static String ValidateRelationships(long partyId)
        {
            using (var partyManager = new PartyManager())
            {
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
        }

        internal static Party EditParty(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues, IList<PartyRelationship> systemPartyRelationships)
        {
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            using (PartyManager partyManager = new PartyManager())
            using (PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager())
            {
                var party = new Party();

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
                    string value = string.IsNullOrEmpty(partyCustomAttributeValueString.Value) ? "" : partyCustomAttributeValueString.Value.Trim();
                    newAddPartyCustomAttrValues.Add(partyCustomAttribute, value);
                }
                party.CustomAttributeValues = partyManager.AddPartyCustomAttributeValues(party, partyCustomAttributeValues.ToDictionary(cc => long.Parse(cc.Key), cc => cc.Value.Trim())).ToList();

                return party;
            }
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
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            using (PartyManager partyManager = new PartyManager())
            using (PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager())
            {
                var newParty = new Party();
                PartyType partyType = partyTypeManager.PartyTypeRepository.Get(partyTypeId);
                var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
                // save party as temp if the reationships are required
                var requiredPartyRelationTypes = partyRelationshipTypeManager.GetAllPartyRelationshipTypes(partyType.Id).Where(cc => cc.MinCardinality > 0);
                //Create party
                newParty = partyManager.Create(partyType, "", description, startDate, endDate, partyStatusType, requiredPartyRelationTypes.Any());
                partyManager.AddPartyCustomAttributeValues(newParty, toPartyCustomAttributeValues(partyCustomAttributeValuesDict, partyTypeId));
                // partyManager.AddPartyRelationship(null,TargetPartyId,PartyTypePairid)
                //var systemPartyTypePairs = GetSystemTypePairs(newParty.PartyType.Id);
                ////add relationship to the all targets
                //foreach (var systemPartyTypePair in systemPartyTypePairs)
                //{
                //    foreach (var targetParty in systemPartyTypePair.TargetPartyType.Parties)
                //    {
                //        PartyTypePair partyTypePair = partyRelationshipTypeManager.PartyTypePairRepository.Reload(systemPartyTypePair);
                //        partyManager.AddPartyRelationship(partyManager.PartyRepository.Reload(newParty), targetParty,  "system", "", systemPartyTypePair, permission: systemPartyTypePair.PermissionTemplate);
                //    }
                //}

                return newParty;
            }
        }

        internal static Dictionary<PartyCustomAttribute, string> toPartyCustomAttributeValues(Dictionary<string, string> partyCustomAttributeValuesDict, long partyTypeId = 0)
        {
            var partyCustomAttributeValues = new Dictionary<PartyCustomAttribute, string>();
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
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
                var typePairs = partTypeManager.PartyTypePairRepository.Get(cc => cc.SourcePartyType.Id == partyTypeId && cc.TargetPartyType.SystemType);
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