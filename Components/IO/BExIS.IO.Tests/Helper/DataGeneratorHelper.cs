using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO.DataType.DisplayPattern;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.IO.Tests.Helper
{
    public class DataGeneratorHelper
    {
        public List<List<string>> GenerateRowsWithRandomValuesBasedOnDatastructure(StructuredDataStructure dataStructure, long numberOfTuples)
        {
            List<List<string>> rows = new List<List<string>>();

            numberOfTuples.Should().BeGreaterThan(0);

            var r = new Random();

            try
            {
                for (int i = 0; i < numberOfTuples; i++)
                {
                    List<string> row = new List<string>();
                    row.Add(r.Next().ToString());
                    row.Add("Test");
                    row.Add(Convert.ToDouble(r.Next()).ToString());
                    row.Add(true.ToString());
                    row.Add(DateTime.Now.ToString());

                    rows.Add(row);
                }

                return rows;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> GenerateRowsWithRandomValuesBasedOnDatastructure(StructuredDataStructure dataStructure, string seperator, long numberOfTuples, bool withQuotes)
        {
            var datePattern = DataTypeDisplayPattern.Get(dataStructure.Variables.ElementAt(4).DisplayPatternId);

            List<string> rows = new List<string>();

            numberOfTuples.Should().BeGreaterThan(0);

            var r = new Random();

            try
            {
                for (int i = 0; i < numberOfTuples; i++)
                {
                    string row = r.Next().ToString();

                    if (withQuotes) row += seperator.ToString() + "\"Test\"";
                    else row += seperator.ToString() + "Test";

                    row += seperator.ToString() + Convert.ToDouble(r.Next()).ToString();
                    row += seperator.ToString() + true.ToString();
                    row += seperator.ToString() + DateTime.Now.ToString(datePattern.DisplayPattern);

                    rows.Add(row);
                }

                return rows;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> GenerateRowsWithRandomValuesBasedOnDatastructureWithErrors(StructuredDataStructure dataStructure, string seperator, long numberOfTuples, bool withQuotes)
        {
            List<string> rows = new List<string>();

            numberOfTuples.Should().BeGreaterThan(0);

            var r = new Random();

            try
            {
                for (int i = 0; i < numberOfTuples; i++)
                {
                    string row = r.Next().ToString();

                    if (withQuotes) row += seperator.ToString() + "\"Test\"";
                    else row += seperator.ToString() + "Test";

                    row += seperator.ToString() + "abc";//Convert.ToDouble(r.Next()).ToString();
                    row += seperator.ToString() + true.ToString();
                    row += seperator.ToString() + DateTime.Now.ToString();

                    rows.Add(row);
                }

                return rows;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}