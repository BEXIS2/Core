using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.IO.Transform.Validation.ValueCheck;
using BExIS.Utils.Helpers;
using F23.StringSimilarity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Entites = BExIS.Dlm.Entities.DataStructure;

namespace BExIS.IO.Transform.Input
{
    public class StructureAnalyser
    {
        private List<DataTypeCheck> checks = null;

        /// <summary>
        /// read 2 rows to suggest a TextSeperator
        /// </summary>
        /// <param name="rowA"></param>
        /// <param name="rowB"></param>
        /// <param name="textMarker"></param>
        /// <returns>TextSeperator</returns>
        public TextSeperator SuggestDelimeter(string rowA, string rowB, TextMarker textMarker = TextMarker.doubleQuotes)
        {
            if (string.IsNullOrEmpty(rowA)) throw new ArgumentNullException(nameof(rowA), "row has no content to suggest.");
            if (string.IsNullOrEmpty(rowB)) throw new ArgumentNullException(nameof(rowB), "row has no content to suggest."); ;

            Dictionary<TextSeperator, int> delimeterCounter = new Dictionary<TextSeperator, int>();

            // if textmarker exist, use this regex expression to detect the content an d replace it with abcd
            // so it remove all seperator in a text
            // (["'])(?:(?=(\\?))\2.)*?\1
            string pattern = RegExHelper.BETWEEN_QUOTES;

            rowA = Regex.Replace(rowA, pattern, "replaced");
            rowB = Regex.Replace(rowB, pattern, "replaced");

            // read row for all textseperators and count them
            foreach (TextSeperator textSeperator in Enum.GetValues(typeof(TextSeperator)))
            {
                char seperator = AsciiFileReaderInfo.GetSeperator(textSeperator);
                char textmarker = AsciiFileReaderInfo.GetTextMarker(textMarker);

                int countA = rowA.Count(c => c == seperator);
                int countB = rowB.Count(c => c == seperator);

                // only chars with same count and more then zero go to dictionary
                if (countA > 0 && countA == countB)
                {
                    delimeterCounter.Add(textSeperator, countA);
                }
            }

            if (delimeterCounter.Count == 0) throw new Exception("the guessing of the operator came to no result.");

            // only one exist, return textseperator
            if (delimeterCounter.Count == 1) return delimeterCounter.First().Key;

            // if more then one exist, return the more expected
            // tab, semicolon, comma, space
            if (delimeterCounter.ContainsKey(TextSeperator.tab)) return TextSeperator.tab;
            else if (delimeterCounter.ContainsKey(TextSeperator.semicolon)) return TextSeperator.semicolon;
            else if (delimeterCounter.ContainsKey(TextSeperator.comma)) return TextSeperator.comma;
            else if (delimeterCounter.ContainsKey(TextSeperator.space)) return TextSeperator.space;

            throw new Exception("the guessing of the operator came to no result.");
        }

        /// <summary>
        /// read 2 rows to suggest a DecimalCharacter
        /// </summary>
        /// <param name="rowA"></param>
        /// <param name="rowB"></param>
        /// <returns>DecimalCharacter</returns>
        public DecimalCharacter SuggestDecimal(string rowA, string rowB, TextSeperator textSeperator, TextMarker textMarker = TextMarker.doubleQuotes)
        {
            if (string.IsNullOrEmpty(rowA)) throw new ArgumentNullException(nameof(rowA), "row has no content to suggest.");
            if (string.IsNullOrEmpty(rowB)) throw new ArgumentNullException(nameof(rowB), "row has no content to suggest."); ;

            // if textseperator == comma then only point is for decimal so return it
            if (textSeperator.Equals(TextSeperator.comma)) return DecimalCharacter.point;

            Dictionary<DecimalCharacter, int> decimalCounter = new Dictionary<DecimalCharacter, int>();

            // read row for all textseperators and count them
            foreach (DecimalCharacter decimalCharacter in Enum.GetValues(typeof(DecimalCharacter)))
            {
                char decimalChar = AsciiFileReaderInfo.GetDecimalCharacter(decimalCharacter);
                char seperator = AsciiFileReaderInfo.GetSeperator(textSeperator);
                char textmarker = AsciiFileReaderInfo.GetTextMarker(textMarker);

                // missing
                // 1.000.000,23
                // 1.000.000,23243546246352465
                // 2,000.345
                // 1.001 - 1.001,00
                // 1.00 - 2,00

                // if textmarker exist, use this regex expression to detect the content an d replace it with replaced
                // so it remove all seperator in a text
                // (["'])(?:(?=(\\?))\2.)*?\1
                string pattern = RegExHelper.BETWEEN_QUOTES;

                rowA = Regex.Replace(rowA, pattern, "replaced");
                rowB = Regex.Replace(rowB, pattern, "replaced");

                List<string> rows = new List<string>();
                rows.Add(rowA);
                rows.Add(rowB);

                int countTotal = 0;

                foreach (var r in rows)
                {
                    // go throw each cell and tyr to find the char
                    foreach (var v in r.Split(seperator))
                    {
                        // count decimalChar in value
                        int c = v.Count(x => x == decimalChar);

                        if (c > 1) break; // if char is more then once it cant be a decimal char
                        else if (c == 1) // char exist once
                        {
                            // get position of char , get rest of teh string and check if only numbers are tehre
                            int position = v.IndexOf(decimalChar);
                            string rest = v.Substring(position + 1);
                            // check if rest are only numbers
                            string converted = Regex.Match(rest, @"\d+").Value;

                            // compare both, if there are the same
                            // only numbers are there and the char can be a decimal
                            if (rest.Equals(converted))
                            {
                                countTotal++;  // its a decimal candidate
                            }
                        }
                    }
                }

                if (countTotal > 0) decimalCounter.Add(decimalCharacter, countTotal);
            }

            // if no decimalCounter is detected, then select point as default
            if (decimalCounter.Count == 0) throw new Exception("the guessing of the operator came to no result.");

            //// only one exist, return textseperator
            if (decimalCounter.Count == 1) return decimalCounter.First().Key;

            // if more then one exist, return the more counted match for a decimal
            // point, comma
            var highCountedDecimalChar = decimalCounter.OrderByDescending(k => k.Value).First();

            return highCountedDecimalChar.Key;

            throw new Exception("the guessing of the operator came to no result.");
        }

        /// <summary>
        /// read 2 rows to suggest a TextMarker
        /// </summary>
        /// <param name="rowA"></param>
        /// <param name="rowB"></param>
        /// <returns>TextMarker</returns>
        public TextMarker SuggestTextMarker(string rowA, string rowB)
        {
            if (string.IsNullOrEmpty(rowA)) throw new ArgumentNullException(nameof(rowA), "row has no content to suggest.");
            if (string.IsNullOrEmpty(rowB)) throw new ArgumentNullException(nameof(rowB), "row has no content to suggest.");

            Dictionary<TextMarker, int> markerCounter = new Dictionary<TextMarker, int>();

            // read row for all textseperators and count them
            foreach (TextMarker marker in Enum.GetValues(typeof(TextMarker)))
            {
                // "hallo \" gsfahgdafhg"
                // with this pattern detect all backslash marked quotes and remove them
                char m = AsciiFileReaderInfo.GetTextMarker(marker);
                string pattern = @"\\" + m;

                rowA = Regex.Replace(rowA, pattern, "");
                rowB = Regex.Replace(rowB, pattern, "");

                int countA = rowA.Count(c => c == m);
                int countB = rowB.Count(c => c == m);
                int total = countA + countB;

                // only chars with same count and more then zero go to dictionary
                if (total > 0 && (total % 2 == 0))
                {
                    markerCounter.Add(marker, countA);
                }
            }

            // return default
            if (markerCounter.Count == 0) return TextMarker.doubleQuotes;

            // only one exist, return textseperator
            if (markerCounter.Count == 1) return markerCounter.First().Key;

            // if more then one exist, return the more expected
            // double quotes, quotes
            if (markerCounter.ContainsKey(TextMarker.doubleQuotes)) return TextMarker.doubleQuotes;
            else if (markerCounter.ContainsKey(TextMarker.quotes)) return TextMarker.quotes;

            throw new Exception("the guessing of the operator came to no result.");
        }

        /// <summary>
        /// suggest a datatype from database based on a system type
        /// </summary>
        /// <param name="systemType"></param>
        /// <returns>DataType</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public List<Entites.DataType> SuggestDataType(string systemType)
        {
            if (string.IsNullOrEmpty(systemType)) throw new ArgumentNullException(nameof(systemType), "system type should not be empty.");

            List<Entites.DataType> result = new List<Entites.DataType>();

            using (var dataTypeManager = new DataTypeManager())
            {
                result = dataTypeManager.Repo.Query(d => d.SystemType.Equals(systemType)).ToList();
            }

            return result;
        }

        /// <summary>
        /// Suggest a unit based on input, datatype
        /// </summary>
        /// <param name="input">abbreviation or name</param>
        /// <param name="datatype"></param>
        /// <param name="similarity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public List<Entites.Unit> SuggestUnit(string input, string variable, string dataType, double similarity = 0.7)
        {
            if (string.IsNullOrEmpty(input) && string.IsNullOrEmpty(dataType)) throw new ArgumentNullException("input and data type should not be empty.");

            using (var dataTypeManager = new DataTypeManager())
            using (var unitManager = new UnitManager())
            {
                List<Unit> units = new List<Unit>();

                if (string.IsNullOrEmpty(dataType)) // no datatype exist
                {
                    units = unitManager.Repo.Get().ToList();
                }
                else // datatype is not empty
                {
                    var dt = dataTypeManager.Repo.Query(d => d.Name.ToLower().Equals(dataType)).FirstOrDefault();
                    if (dt != null) units = unitManager.Repo.Query(u => u.AssociatedDataTypes.Contains(dt)).ToList();
                }

                Dictionary<Unit, double> matches = new Dictionary<Unit, double>();

                foreach (var unit in units)
                {
                    var ro = new RatcliffObershelp();
                    var varSimularity = ro.Similarity(variable, unit.Name);
                    var varAbbrSimularity = ro.Similarity(variable, unit.Abbreviation);
                    var nameSimularity = ro.Similarity(input, unit.Name);
                    var abbrSimularity = ro.Similarity(input, unit.Abbreviation);

                    // compare highest for var
                    var highestVariable = Math.Max(varSimularity, varAbbrSimularity);

                    // compare highest for var
                    var highestInput = Math.Max(nameSimularity, abbrSimularity);

                    // highest from both
                    var highest = Math.Max(highestVariable, highestInput);

                    if (string.IsNullOrEmpty(input) || highest >= similarity)
                    {
                        matches.Add(unit, highest);
                    }
                }

                return matches.OrderByDescending(x => x.Value).Select(u => u.Key).ToList();
            }
        }

        public List<Entites.VariableTemplate> SuggestTemplate(string input, long unitId, long dataTypeId, double similarity = 0.7)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input), "input should not be empty.");

            Dictionary<VariableTemplate, double> matches = new Dictionary<VariableTemplate, double>();
            /*
             * in the suggestion of the templates, there are 5 factors that must be considered.
             * all three are checked independently of each other and the results are sorted so that the most matches are at the top.
             *
             * name
             * meaning
             * unit
             * dimension
             * datatype
             */

            using (var variableManager = new VariableManager())
            using (var unitManager = new UnitManager())
            {
                var allTemplates = variableManager.VariableTemplateRepo.Get();

                // var template matches by name
                var byNames = new List<VariableTemplate>();

                if (string.IsNullOrEmpty(input)) // if a varaible name exist as input
                {

                    // name
                    foreach (var template in allTemplates)
                    {
                        var ro = new RatcliffObershelp();
                        var nameSimularity = ro.Similarity(input.ToLower(), template.Label.ToLower());

                        if (string.IsNullOrEmpty(input) || nameSimularity >= similarity)
                        {
                            matches.Add(template, nameSimularity);
                        }
                    }

                    // meaning
                    foreach (var template in allTemplates)
                    {
                        if (template.Meanings.Any())
                        {
                            foreach (var meaning in template.Meanings)
                            {
                                var ro = new RatcliffObershelp();
                                var nameSimularity = ro.Similarity(input.ToLower(), meaning.Name.ToLower());

                                if (string.IsNullOrEmpty(input) || nameSimularity >= similarity)
                                {
                                    if (matches.ContainsKey(template)) matches[template] += nameSimularity;
                                    else matches.Add(template, nameSimularity);
                                }
                            }
                        }
                    }
                }

                if (unitId > 0)
                {
                    // var template matches with units
                    var byUnit = allTemplates.Where(t => t.Unit.Id.Equals(unitId)).ToList();

                    var unit = unitManager.Repo.Get(unitId);
                    List<VariableTemplate> byDimension = new List<VariableTemplate>();
                    if (unit != null && unit.Dimension != null)
                        byDimension = allTemplates.Where(t => t.Unit.Dimension.Id.Equals(unit.Dimension.Id)).ToList();

                    //add units results to matches
                    foreach (var item in byUnit)
                    {
                        // if variableTemplate allready exist in the matches increase the value by 1
                        if (matches.ContainsKey(item)) matches[item] += 1;
                        else matches.Add(item, 1);
                    }

                    //add dimenions results to matches
                    foreach (var item in byDimension)
                    {
                        // if variableTemplate allready exist in the matches increase the value by 1
                        if (matches.ContainsKey(item)) matches[item] += 0.9;
                        else matches.Add(item, 0.9);
                    }

                    // var template matches with datatype
                    var byDatype = allTemplates.Where(t => t.DataType.Id.Equals(dataTypeId)).ToList();

                    //add datatypes results to matches
                    foreach (var item in byDatype)
                    {
                        // if variableTemplate allready exist in the matches increase the value by 0.5
                        if (matches.ContainsKey(item)) matches[item] += 0.5;
                        else matches.Add(item, 0.5);
                    }
                }
            }

            return matches.OrderByDescending(x => x.Value).Select(u => u.Key).ToList();
        }

        public List<string> GetRandowRows(List<string> rows, int numberOfRows)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, Type> SuggestSystemTypes(List<string> rows, TextSeperator delimeter, DecimalCharacter decimalCharacter, List<string> missingValues)
        {
            Dictionary<int, List<Type>> source = new Dictionary<int, List<Type>>();
            Dictionary<int, Type> result = new Dictionary<int, Type>();

            // get type checks
            checks = getDataTypeChecks(decimalCharacter);

            // create dictionary with types per column
            char seperator = AsciiFileReaderInfo.GetSeperator(delimeter);
            int numberOfRow = rows.First().Split(seperator).Count();

            for (int i = 0; i < numberOfRow; i++)
            {
                source.Add(i, getDefaultDataTypes());
            }

            // go throw each row
            foreach (var row in rows)
            {
                //split row based on sepeartor
                foreach (var cell in row.Split(seperator).Select((x, i) => new { Value = x, Index = i }))
                {
                    //update possible list of types by check the value against the existing list
                    source[cell.Index] = checkValue(cell.Value, source[cell.Index], missingValues);
                }
            }

            // result preparation -
            foreach (var kvp in source)
            {
                result.Add(kvp.Key, getMostRestructiveType(kvp.Value));
            }

            return result;
        }

        private List<Type> checkValue(string value, List<Type> types, List<string> missingValues)
        {
            value = value.Trim();
            // if list has no entries - skip
            if (types.Count == 0) return types;

            // check missing values
            if (missingValues.Contains(value)) return types;

            List<Type> approvedTypes = new List<Type>();

            // check value
            foreach (var type in types)
            {
                if (type != null)
                {
                    var checks = getChecksBasedOnType(type);

                    foreach (var check in checks)
                    {
                        //get typecheck
                        var result = check.Execute(value, 0);

                        // if result is an error, skip it
                        if (!result.GetType().Equals(typeof(Error)) && !approvedTypes.Contains(type))
                        {
                            approvedTypes.Add(type);
                        }
                    }
                }
            }

            return approvedTypes;
        }

        /// <summary>
        /// get list of possibale datatypechecks based on default list of types
        /// </summary>
        /// <param name="decimalCharacter"></param>
        /// <returns></returns>
        private List<DataTypeCheck> getDataTypeChecks(DecimalCharacter decimalCharacter)
        {
            List<DataTypeCheck> typeChecks = new List<DataTypeCheck>();
            List<Type> types = getDefaultDataTypes();

            foreach (var type in types)
            {
                // add default datatype check without displaypattern
                typeChecks.Add(new DataTypeCheck("", type.Name, decimalCharacter));

                // if display pattern exist then also add a typecheck per type * displaypattern
                var displayPatterns = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.ToString().Equals(Type.GetTypeCode(type).ToString()));
                foreach (var dp in displayPatterns)
                {
                    typeChecks.Add(new DataTypeCheck("", type.Name, decimalCharacter, dp.DisplayPattern));
                }
            }

            return typeChecks;
        }

        /// <summary>
        /// get default list of possible datatypes
        /// </summary>
        /// <returns></returns>
        private List<Type> getDefaultDataTypes()
        {
            List<Type> types = new List<Type>();

            types.Add(typeof(Boolean));
            types.Add(typeof(DateTime));
            types.Add(typeof(Decimal));
            types.Add(typeof(Double));
            types.Add(typeof(Int64));
            types.Add(typeof(UInt32));

            return types;
        }

        private IEnumerable<DataTypeCheck> getChecksBasedOnType(Type type)
        {
            return checks.Where(c => c.DataType.Equals(type.Name));
        }

        private Type getMostRestructiveType(List<Type> types)
        {
            if (types.Any())
            {
                if (types.Contains(typeof(UInt32))) return typeof(UInt32);
                if (types.Contains(typeof(Int64))) return typeof(Int64);
                if (types.Contains(typeof(Double))) return typeof(Double);
                if (types.Contains(typeof(Decimal))) return typeof(Decimal);
                if (types.Contains(typeof(DateTime))) return typeof(DateTime);
                if (types.Contains(typeof(Boolean))) return typeof(Boolean);
            }

            return typeof(String);
        }

        /// <summary>
        /// return number of rows that should be used for analyse.
        /// based on settings min, max and percentage is set
        /// </summary>
        /// <param name="total">number of data rows</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public long GetNumberOfRowsToAnalyse(int min, int max, int percentage, long totaldata)
        {
            if (min == 0) throw new ArgumentNullException(nameof(min), "min should be greater then 0");
            if (max == 0) throw new ArgumentNullException(nameof(max), "max should be greater then 0");
            if (percentage == 0) throw new ArgumentNullException(nameof(percentage), "min should be greater then 0");
            if (totaldata == 0) throw new ArgumentNullException(nameof(totaldata), "min should be greater then 0");
            if (min > max) throw new ArgumentNullException(nameof(max), "min should be greater then max");

            if (totaldata < min) return totaldata;

            // between min and max - check percentage
            if (totaldata > min && totaldata < max)
                return (totaldata * percentage) / 100;

            if (totaldata > max) return max;

            return totaldata;
        }
    }
}