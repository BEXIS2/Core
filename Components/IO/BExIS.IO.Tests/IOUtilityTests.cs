using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.IO.Transform.Validation.ValueCheck;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BExIS.IO.Tests
{
    [TestFixture()]
    public class IOUtilityTests
    {
        private List<DateTimeHelperUTObject> DateTimeCases = new List<DateTimeHelperUTObject>();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DateTimeCases = generateDateTimeCases();
        }

        [SetUp]
        protected void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [TestCase("14", "MM", "10/1/0001 12:00:00 AM", false)]
        [TestCase("25", "hh", "1/1/0001 10:00:00 AM", false)]
        [TestCase("2024", "yyyy", "1/1/2024 12:00:00 AM", true)]
        [TestCase("2010", "yyyy", "1/1/2010 12:00:00 AM", true)]
        [TestCase("10", "MM", "10/1/0001 12:00:00 AM", true)]
        [TestCase("10", "hh", "1/1/0001 10:00:00 AM", true)]
        [TestCase("10", "mm", "1/1/0001 12:10:00 AM", true)]
        [TestCase("10", "ss", "1/1/0001 12:00:10 AM", true)]
        [TestCase("2017-10-24 11:00:00", "yyyy-MM-dd hh:mm:ss", "10/24/2017 11:00:00 AM", true)]
        [TestCase("24.10.2017 11:00:00", "dd.MM.yyyy hh:mm:ss", "10/24/2017 11:00:00 AM", true)]
        [TestCase("5/10/2014 12:00:00 AM", "d/M/yyyy hh:mm:ss tt", "10/5/2014 12:00:00 AM", true)]
        [TestCase("7/1/2023 2:00:00 AM", "d/M/yyyy h:mm:ss tt", "1/7/2023 2:00:00 AM", true)]
        [TestCase("3/2/2019 7:31", "M/d/yyyy h:m", "3/2/2019 7:31:00 AM", true)]
        [TestCase("03/02/2019 7:31", "MM/dd/yyyy h:m", "3/2/2019 7:31:00 AM", true)]
        [TestCase("2017-10-24T11:00:00", "yyyy-MM-ddThh:mm:ss", "10/24/2017 11:00:00 AM", true)]
        [TestCase("2017-10-24T13:00:00", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", true)]
        [TestCase("2017-10-24T1:00:00", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", false)]
        [TestCase("5/10/2014", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", false)]
        [TestCase("2014/12/20", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", false)]
        [TestCase("2017-10-24", "yyyy-MM-dd", "10/24/2017 12:00:00 AM", true)]
        [TestCase("10/24/2017", "MM/dd/yyyy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("24/24/2017", "MM/dd/yyyy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("10/24/17", "MM/dd/yy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("24/24/17", "MM/dd/yy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("24/10/2017", "dd/MM/yyyy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("10/24/2017", "dd/MM/yyyy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("24/10/17", "dd/MM/yy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("10/24/17", "dd/MM/yy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("24.10.2017", "dd.MM.yyyy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("10.24.2017", "dd.MM.yyyy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("24.10.17", "dd.MM.yy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("10.24.17", "dd.MM.yy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("23:00:00", "HH:mm:ss", "1/1/0001 11:00:00 PM", true)]
        [TestCase("11:00:00", "HH:mm:ss", "1/1/0001 11:00:00 PM", false)]
        [TestCase("23:00", "HH:mm", "1/1/0001 11:00:00 PM", true)]
        [TestCase("11:00", "HH:mm", "1/1/0001 11:00:00 PM", false)]
        [TestCase("11:00:11", "HH:mm", "1/1/0001 11:00:00 PM", false)]
        [TestCase("11:00:00 PM", "hh:mm:ss tt", "1/1/0001 11:00:00 PM", true)]
        [TestCase("13:00:00 AM", "hh:mm:ss tt", "1/1/0001 11:00:00 PM", false)]
        [TestCase("11:00 PM", "hh:mm tt", "1/1/0001 11:00:00 PM", true)]
        [TestCase("13:00 AM", "hh:mm tt", "1/1/0001 11:00:00 PM", false)]
        [TestCase("2017", "yyyy", "1/1/2017 12:00:00 AM", true)]
        [TestCase("1", "MM", "1/1/0001 12:00:00 AM", true)]
        [TestCase("jan", "MMM", "1/1/2025 12:00:00 AM", true)]
        [TestCase("01", "MM", "1/1/0001 12:00:00 AM", true)]
        [TestCase("january", "MMMM", "1/1/2025 12:00:00 AM", true)]
        [TestCase("Januar", "MMMM", "1/1/2025 12:00:00 AM", true, "de-de")]
        [TestCase("24/10/2017", "MM/dd/yyyy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("2006-2-2", "yyyy-M-d", "2/2/2006 12:00:00 AM", true)]
        [TestCase("2006-02-02", "yyyy-MM-dd", "2/2/2006 12:00:00 AM", true)]
        [TestCase("2006-2-2", "yyyy-d-M", "2/2/2006 12:00:00 AM", true)]
        [TestCase("2006-02-02", "yyyy-dd-MM", "2/2/2006 12:00:00 AM", true)]
        [TestCase("5/10/2014 12:00:00 AM", "M/d/yyyy hh:mm:ss tt", "5/10/2014 12:00:00 AM", true)]
        public void ConvertStringToDateTimeWithpatternTest(string input, string pattern, string output, bool itMatch, string culture = "")
        {
            IOUtility iOUtility = new IOUtility();
            CultureInfo cultureInfo = null;

            if (!string.IsNullOrEmpty(culture))
            {
                cultureInfo = new CultureInfo(culture, false);
            }


            DateTime result;
            iOUtility.ConvertToDate(input, pattern, out result, cultureInfo);
   
            if (itMatch)
            {
                result.ToString( new CultureInfo("en-US", false)).Should().NotBeNull("can´t convert : " + input + " with pattern:" + pattern);
                result.ToString( new CultureInfo("en-US", false)).Should().Match(output, "not match :" + result + " - " + output);
            }
            else
            {
                if (result != null) result.ToString( new CultureInfo("en-US", false)).Should().NotMatch(output, "should not match :" + result + " - " + output);
                else result.ToString( new CultureInfo("en-US", false)).Should().BeNullOrEmpty();
            }
            
        }

        [TestCase("14", "MM", "10/1/0001 12:00:00 AM", false)]
        [TestCase("25", "hh", "1/1/0001 10:00:00 AM", false)]
        [TestCase("2024", "yyyy", "1/1/2024 12:00:00 AM", true)]
        [TestCase("2010", "yyyy", "1/1/2010 12:00:00 AM", true)]
        [TestCase("10", "MM", "10/1/0001 12:00:00 AM", true)]
        [TestCase("10", "hh", "1/1/0001 10:00:00 AM", true)]
        [TestCase("10", "mm", "1/1/0001 12:10:00 AM", true)]
        [TestCase("10", "ss", "1/1/0001 12:00:10 AM", true)]
        [TestCase("2017-10-24 11:00:00", "yyyy-MM-dd hh:mm:ss", "10/24/2017 11:00:00 AM", true)]
        [TestCase("24.10.2017 11:00:00", "dd.MM.yyyy hh:mm:ss", "10/24/2017 11:00:00 AM", true)]
        [TestCase("5/10/2014 12:00:00 AM", "d/M/yyyy hh:mm:ss tt", "10/5/2014 12:00:00 AM", true)]
        [TestCase("7/1/2023 2:00:00 AM", "d/M/yyyy h:mm:ss tt", "1/7/2023 2:00:00 AM", true)]
        [TestCase("3/2/2019 7:31", "M/d/yyyy h:m", "3/2/2019 7:31:00 AM", true)]
        [TestCase("03/02/2019 7:31", "MM/dd/yyyy h:m", "3/2/2019 7:31:00 AM", true)]
        [TestCase("2017-10-24T11:00:00", "yyyy-MM-ddThh:mm:ss", "10/24/2017 11:00:00 AM", true)]
        [TestCase("2017-10-24T13:00:00", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", true)]
        [TestCase("2017-10-24T1:00:00", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", false)]
        [TestCase("5/10/2014", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", false)]
        [TestCase("2014/12/20", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", false)]
        [TestCase("2017-10-24", "yyyy-MM-dd", "10/24/2017 12:00:00 AM", true)]
        [TestCase("10/24/2017", "MM/dd/yyyy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("24/24/2017", "MM/dd/yyyy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("10/24/17", "MM/dd/yy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("24/24/17", "MM/dd/yy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("24/10/2017", "dd/MM/yyyy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("10/24/2017", "dd/MM/yyyy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("24/10/17", "dd/MM/yy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("10/24/17", "dd/MM/yy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("24.10.2017", "dd.MM.yyyy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("10.24.2017", "dd.MM.yyyy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("24.10.17", "dd.MM.yy", "10/24/2017 12:00:00 AM", true)]
        [TestCase("10.24.17", "dd.MM.yy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("23:00:00", "HH:mm:ss", "1/1/0001 11:00:00 PM", true)]
        [TestCase("11:00:00", "HH:mm:ss", "1/1/0001 11:00:00 PM", false)]
        [TestCase("23:00", "HH:mm", "1/1/0001 11:00:00 PM", true)]
        [TestCase("11:00", "HH:mm", "1/1/0001 11:00:00 PM", false)]
        [TestCase("11:00:11", "HH:mm", "1/1/0001 11:00:00 PM", false)]
        [TestCase("11:00:00 PM", "hh:mm:ss tt", "1/1/0001 11:00:00 PM", true)]
        [TestCase("13:00:00 AM", "hh:mm:ss tt", "1/1/0001 11:00:00 PM", false)]
        [TestCase("11:00 PM", "hh:mm tt", "1/1/0001 11:00:00 PM", true)]
        [TestCase("13:00 AM", "hh:mm tt", "1/1/0001 11:00:00 PM", false)]
        [TestCase("2017", "yyyy", "1/1/2017 12:00:00 AM", true)]
        [TestCase("1", "MM", "1/1/0001 12:00:00 AM", true)]
        [TestCase("jan", "MMM", "1/1/2025 12:00:00 AM", true)]
        [TestCase("01", "MM", "1/1/0001 12:00:00 AM", true)]
        [TestCase("january", "MMMM", "1/1/2025 12:00:00 AM", true)]
        [TestCase("Januar", "MMMM", "1/1/2025 12:00:00 AM", true, "de-de")]
        [TestCase("24/10/2017", "MM/dd/yyyy", "10/24/2017 12:00:00 AM", false)]
        [TestCase("2006-2-2", "yyyy-M-d", "2/2/2006 12:00:00 AM", true)]
        [TestCase("2006-02-02", "yyyy-MM-dd", "2/2/2006 12:00:00 AM", true)]
        [TestCase("2006-2-2", "yyyy-d-M", "2/2/2006 12:00:00 AM", true)]
        [TestCase("2006-02-02", "yyyy-dd-MM", "2/2/2006 12:00:00 AM", true)]
        [TestCase("5/10/2014 12:00:00 AM", "M/d/yyyy hh:mm:ss tt", "5/10/2014 12:00:00 AM", true)]
        public void CompareDateTimeValidationAndConvertion(string input, string pattern, string output, bool itMatch, string culture = "")
        {
            IOUtility iOUtility = new IOUtility();
            CultureInfo cultureInfo = null;
            if (!string.IsNullOrEmpty(culture))
            {
                cultureInfo = new CultureInfo(culture, false);
            }

            DateTime result;
            iOUtility.ConvertToDate(input, pattern, out result, cultureInfo);
            DataTypeCheck check = new DataTypeCheck("dtCheck", "DateTime", DecimalCharacter.point, pattern, cultureInfo);
            var checkresult = check.Execute(input, 1);

            if (itMatch)
            {
                result.ToString(new CultureInfo("en-US", false)).Should().NotBeNull("can´t convert : " + input + " with pattern:" + pattern);
                result.ToString(new CultureInfo("en-US", false)).Should().Match(output, "not match :" + result.ToString(new CultureInfo("en-US", false)) + " - " + output);
                //checkresult.Should(typeof(DateTime))
                //Assert.IsInstanceOf<DateTime>(checkresult, "xyz");
                checkresult.Should().BeOfType<DateTime>();
            }
            else
            {
                if (result != null) result.ToString(new CultureInfo("en-US", false)).Should().NotMatch(output, "should not match :" + result + " - " + output);
                else
                {
                    result.ToString(new CultureInfo("en-US", false)).Should().BeNullOrEmpty();
                    checkresult.Should().BeOfType<Error>();
                }
            }
            
        }

        #region Helper


        private List<DateTimeHelperUTObject> generateDateTimeCases()
        {
            List<DateTimeHelperUTObject> cases = new List<DateTimeHelperUTObject>();
            
            

            return cases;
        }

        #endregion Helper
    }

    public class DateTimeHelperUTObject
    {
        public string pattern { get; set; }
        public string input { get; set; }
        public string output { get; set; }
        public bool ItMatch { get; set; }
        public CultureInfo CultureInfo { get; set; }

        public DateTimeHelperUTObject(string input, string pattern, string output, bool itMatch, CultureInfo cultureInfo = null)
        {
            pattern = pattern;
            input = input;
            output = output;
            ItMatch = itMatch;

            if (cultureInfo == null) CultureInfo = CultureInfo.InvariantCulture;
            else CultureInfo = cultureInfo;
        }
    }
}