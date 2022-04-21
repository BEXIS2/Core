using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.IO.Transform.Validation.ValueCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <returns>TextSeperator</returns>
        public TextSeperator SuggestDelimter(string rowA, string rowB)
        {
            if (string.IsNullOrEmpty(rowA)) throw new ArgumentNullException(nameof(rowA),"row has no content to suggest.");
            if (string.IsNullOrEmpty(rowB)) throw new ArgumentNullException(nameof(rowB), "row has no content to suggest."); ;

            Dictionary<TextSeperator,int> delimeterCounter = new Dictionary<TextSeperator,int>();

            // read row for all textseperators and count them
            foreach (TextSeperator textSeperator in Enum.GetValues(typeof(TextSeperator)))
            {
                char seperator  = AsciiFileReaderInfo.GetSeperator(textSeperator) ;

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
            if (delimeterCounter.Count==1) return delimeterCounter.First().Key;

            // if more then one exist, return the more expected
            // tab, semicolon, comma, space
            if (delimeterCounter.ContainsKey(TextSeperator.tab)) return TextSeperator.tab;
            else if (delimeterCounter.ContainsKey(TextSeperator.semicolon)) return TextSeperator.semicolon;
            else if (delimeterCounter.ContainsKey(TextSeperator.comma)) return TextSeperator.comma;
            else if (delimeterCounter.ContainsKey(TextSeperator.space)) return TextSeperator.space;

            throw new Exception("the guessing of the operator came to no result."); ;
        }

        public List<string> GetRandowRows(List<string> rows, int numberOfRows)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, Type> SuggestDataTypes(List<string> rows, TextSeperator delimeter, DecimalCharacter decimalCharacter, List<string> missingValues)
        {
            Dictionary<int, List<Type>> source = new Dictionary<int, List<Type>>();
            Dictionary<int, Type> result = new Dictionary<int, Type>();

            // get teyp checks
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

            // check value
            for (int i = 0; i < types.Count; i++)
            {
                var type = types.ElementAt(i);
                if (type != null)
                {
                    //get typecheck
                    var check = getCheckBasedOnType(type);
                    var result = check.Execute(value,0);

                    // if result is an error, skip it
                    if (result.GetType().Equals(typeof(Error)))
                    {
                        types.RemoveAt(i);
                    }
                }
            }

            return types;
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
                typeChecks.Add(new DataTypeCheck("", type.Name, decimalCharacter));
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

        private DataTypeCheck getCheckBasedOnType(Type type)
        {
            return checks.Where(c => c.DataType.Equals(type.Name)).FirstOrDefault();
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

    }


}
