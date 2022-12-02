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

        List<DateTimeHelperUTObject> DateTimeCases = new List<DateTimeHelperUTObject>();


        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            DateTimeCases = generateDateTimeCases();
        }

        [SetUp]
        /// performs the initial setup for the tests. This runs once per test, NOT per class!
        protected void SetUp()
        {

        }

        [TearDown]
        /// performs the cleanup after each test
        public void TearDown()
        {

        }

        [OneTimeTearDown]
        /// It is called once after executing all the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the children
        /// Executes only if: counterpart OneTimeSetUp exists and executed successfully.
        public void OneTimeTearDown()
        {

        }

        [Test()]
        public void ConvertStringToDateTimeWithPatternTest()
        {
            IOUtility iOUtility = new IOUtility();

            foreach (var dt in DateTimeCases)
            {
                DateTime result;
                iOUtility.ConvertToDate(dt.InputDateTimeString, dt.Pattern, out result, dt.CultureInfo);

                if (dt.ItMatch)
                {
                    result.ToString(new CultureInfo("en-US", false)).Should().NotBeNull("can´t convert : " + dt.InputDateTimeString + " with pattern:" + dt.Pattern);
                    result.ToString(new CultureInfo("en-US", false)).Should().Match(dt.OutputDateTimeString, "not match :" + result + " - " + dt.OutputDateTimeString);
                }
                else
                {
                    if (result != null) result.ToString(new CultureInfo("en-US", false)).Should().NotMatch(dt.OutputDateTimeString, "should not match :" + result + " - " + dt.OutputDateTimeString);
                    else result.ToString(new CultureInfo("en-US", false)).Should().BeNullOrEmpty();
                }
            }
        }

        [Test()]
        public void CompareDateTimeValidationAndConvertion()
        {
            IOUtility iOUtility = new IOUtility();

            foreach (var dt in DateTimeCases)
            {
                DateTime result;
                iOUtility.ConvertToDate(dt.InputDateTimeString, dt.Pattern, out result, dt.CultureInfo);
                DataTypeCheck check = new DataTypeCheck("dtCheck", "DateTime", DecimalCharacter.point, dt.Pattern, dt.CultureInfo);
                var checkresult = check.Execute(dt.InputDateTimeString, 1);

                if (dt.ItMatch)
                {
                    result.ToString(new CultureInfo("en-US", false)).Should().NotBeNull("can´t convert : " + dt.InputDateTimeString + " with pattern:" + dt.Pattern);
                    result.ToString(new CultureInfo("en-US", false)).Should().Match(dt.OutputDateTimeString, "not match :" + result.ToString(new CultureInfo("en-US", false)) + " - " + dt.OutputDateTimeString);
                    //checkresult.Should(typeof(DateTime))
                    //Assert.IsInstanceOf<DateTime>(checkresult, "xyz");
                    checkresult.Should().BeOfType<DateTime>();
                }
                else
                {
                    if (result != null) result.ToString(new CultureInfo("en-US", false)).Should().NotMatch(dt.OutputDateTimeString, "should not match :" + result + " - " + dt.OutputDateTimeString);
                    else
                    {
                        result.ToString(new CultureInfo("en-US", false)).Should().BeNullOrEmpty();
                        checkresult.Should().BeOfType<Error>();
                    }
                }
            }
        }

        #region Helper

        /*
          private static List<DataTypeDisplayPattern> displayPattern = new List<DataTypeDisplayPattern>()
        {
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateTimeIso",   StringPattern = "yyyy-MM-ddThh:mm:ss",      RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateIso",       StringPattern = "yyyy-MM-dd",               RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateUs",        StringPattern = "MM/dd/yyyy",               RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateUk",        StringPattern = "dd/MM/yyyy",               RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateEu",        StringPattern = "dd.MM.yyyy",               RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Time",          StringPattern = "HH:mm:ss",                 RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Time 12h",      StringPattern = "hh:mm:ss tt",              RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Year",          StringPattern = "yyyy",                     RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Month",         StringPattern = "MMMM",                   RegexPattern = null}26.11.1921
        };
        */

        private List<DateTimeHelperUTObject> generateDateTimeCases()
        {
            List<DateTimeHelperUTObject> cases = new List<DateTimeHelperUTObject>();

            cases.Add(new DateTimeHelperUTObject("3/2/2019 7:31", "M/d/yyyy h:m", "3/2/2019 7:31:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("03/02/2019 7:31", "MM/dd/yyyy h:m", "3/2/2019 7:31:00 AM", true));

            cases.Add(new DateTimeHelperUTObject("2017-10-24T11:00:00", "yyyy-MM-ddThh:mm:ss", "10/24/2017 11:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("2017-10-24T13:00:00", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", true));
            cases.Add(new DateTimeHelperUTObject("2017-10-24T1:00:00", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", false));
            cases.Add(new DateTimeHelperUTObject("5/10/2014", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", false));
            cases.Add(new DateTimeHelperUTObject("2014/12/20", "yyyy-MM-ddTHH:mm:ss", "10/24/2017 1:00:00 PM", false));

            cases.Add(new DateTimeHelperUTObject("2017-10-24", "yyyy-MM-dd", "10/24/2017 12:00:00 AM", true));

            cases.Add(new DateTimeHelperUTObject("10/24/2017", "MM/dd/yyyy", "10/24/2017 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("24/24/2017", "MM/dd/yyyy", "10/24/2017 12:00:00 AM", false));
            cases.Add(new DateTimeHelperUTObject("10/24/17", "MM/dd/yy", "10/24/2017 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("24/24/17", "MM/dd/yy", "10/24/2017 12:00:00 AM", false));

            cases.Add(new DateTimeHelperUTObject("24/10/2017", "dd/MM/yyyy", "10/24/2017 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("10/24/2017", "dd/MM/yyyy", "10/24/2017 12:00:00 AM", false));
            cases.Add(new DateTimeHelperUTObject("24/10/17", "dd/MM/yy", "10/24/2017 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("10/24/17", "dd/MM/yy", "10/24/2017 12:00:00 AM", false));

            cases.Add(new DateTimeHelperUTObject("24.10.2017", "dd.MM.yyyy", "10/24/2017 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("10.24.2017", "dd.MM.yyyy", "10/24/2017 12:00:00 AM", false));
            cases.Add(new DateTimeHelperUTObject("24.10.17", "dd.MM.yy", "10/24/2017 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("10.24.17", "dd.MM.yy", "10/24/2017 12:00:00 AM", false));

            cases.Add(new DateTimeHelperUTObject("23:00:00", "HH:mm:ss", "1/1/0001 11:00:00 PM", true));
            cases.Add(new DateTimeHelperUTObject("11:00:00", "HH:mm:ss", "1/1/0001 11:00:00 PM", false));

            cases.Add(new DateTimeHelperUTObject("23:00", "HH:mm", "1/1/0001 11:00:00 PM", true));
            cases.Add(new DateTimeHelperUTObject("11:00", "HH:mm", "1/1/0001 11:00:00 PM", false));
            cases.Add(new DateTimeHelperUTObject("11:00:11", "HH:mm", "1/1/0001 11:00:00 PM", false));

            cases.Add(new DateTimeHelperUTObject("11:00:00 PM", "hh:mm:ss tt", "1/1/0001 11:00:00 PM", true));
            cases.Add(new DateTimeHelperUTObject("13:00:00 AM", "hh:mm:ss tt", "1/1/0001 11:00:00 PM", false));

            cases.Add(new DateTimeHelperUTObject("11:00 PM", "hh:mm tt", "1/1/0001 11:00:00 PM", true));
            cases.Add(new DateTimeHelperUTObject("13:00 AM", "hh:mm tt", "1/1/0001 11:00:00 PM", false));

            cases.Add(new DateTimeHelperUTObject("2017", "yyyy", "1/1/2017 12:00:00 AM", true));
            //cases.Add(new DateTimeHelperUTObject("1", "MM", "1/1/"+DateTime.Now.Year+" 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("jan", "MMM", "1/1/" + DateTime.Now.Year + " 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("01", "MM", "1/1/" + DateTime.Now.Year + " 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("january", "MMMM", "1/1/" + DateTime.Now.Year + " 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("Januar", "MMMM", "1/1/" + DateTime.Now.Year + " 12:00:00 AM", true, new CultureInfo("de-de")));
            //cases.Add(new DateTimeHelperUTObject("24/10/2017", "MM/dd/yyyy", "10/24/2017 12:00:00 AM", true));

            cases.Add(new DateTimeHelperUTObject("2006-2-2", "yyyy-M-d", "2/2/2006 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("2006-02-02", "yyyy-MM-dd", "2/2/2006 12:00:00 AM", true));

            cases.Add(new DateTimeHelperUTObject("2006-2-2", "yyyy-d-M", "2/2/2006 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("2006-02-02", "yyyy-dd-MM", "2/2/2006 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("5/10/2014 12:00:00 AM", "d/M/yyyy hh:mm:ss tt", "10/5/2014 12:00:00 AM", true));
            cases.Add(new DateTimeHelperUTObject("5/10/2014 12:00:00 AM", "M/d/yyyy hh:mm:ss tt", "5/10/2014 12:00:00 AM", true));

            return cases;
        }

        #endregion
    }

    public class DateTimeHelperUTObject
    {
        public string Pattern { get; set; }
        public string InputDateTimeString { get; set; }
        public string OutputDateTimeString { get; set; }
        public bool ItMatch { get; set; }
        public CultureInfo CultureInfo { get; set; }

        public DateTimeHelperUTObject(string input, string pattern, string output, bool itMatch, CultureInfo cultureInfo = null)
        {
            Pattern = pattern;
            InputDateTimeString = input;
            OutputDateTimeString = output;
            ItMatch = itMatch;

            if (cultureInfo == null) CultureInfo = CultureInfo.InvariantCulture;
            else CultureInfo = cultureInfo;

        }
    }
}
