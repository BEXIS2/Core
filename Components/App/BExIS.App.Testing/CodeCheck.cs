using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BExIS.App.Testing
{
    [TestFixture()]
    public class CodeCheck
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
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

        [Test()]
        public void ManagerDisposed()
        {
            var current = System.AppDomain.CurrentDomain.BaseDirectory;
            var solutionpath = VisualStudioProvider.TryGetSolutionDirectoryInfo(current);
            string[] files = Directory.GetFiles(solutionpath.FullName, "*.cs", SearchOption.AllDirectories);
            const Int32 BufferSize = 128;
            List<string> lines = new List<string>();
            int countAll = 0;

            foreach (string file in files)
            {
                string filename = Path.GetFileNameWithoutExtension(file);
                if (filename.EndsWith("Tests")) continue;
                if (filename.EndsWith("Test")) continue;
                if (filename.StartsWith("Test")) continue;
                if (filename.Contains("UiTestController")) continue;

                using (var fileStream = File.OpenRead(file))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    string regexPatternManager = @"(new.*Manager\(\))";
                    string regexPatternDispose = @".Dispose\(\)";

                    String line;
                    int count = 0;

                    //bool found = false;
                    lines = new List<string>();
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        count++;

                        if ((Regex.IsMatch(line, regexPatternManager)
                            && !line.Trim().StartsWith("using")
                            && !line.Trim().StartsWith("*")
                            && !line.Trim().StartsWith("/")
                            && !line.Trim().Contains("AbstractTaskManager")
                            && !line.Trim().Contains("XmlSchemaManager")
                            && !line.Trim().Contains("OutputDataManager")
                            && !line.Trim().Contains("SubmissionManager")
                            && !line.Trim().Contains("ImportMetadataStructureTaskManager")
                            && !line.Trim().Contains("EasyUploadTaskManager")
                            && !line.Trim().Contains("TaskManager")
                            && !line.Trim().Contains("base(")) ||
                            (Regex.IsMatch(line, regexPatternDispose) && !line.Contains("guow")))
                        {
                            bool isInit = false;
                            //check if its a start or end pattern
                            if ((Regex.IsMatch(line, regexPatternManager) && !line.Trim().StartsWith("using"))) isInit = true;

                            if (isInit)
                            {
                                lines.Add(count + " :\t" + line);
                            }
                            else
                            {
                                //example
                                /**
                                 * var dm = new DatasetManager();
                                 *
                                 * dm.dispose();
                                 */

                                //e.g line = dm.dispose();
                                var name = line.Split('.')[0];
                                name = name.Trim();
                                if (name.Contains('?')) name = name.Replace("?", string.Empty);

                                for (int i = 0; i < lines.Count; i++)
                                {
                                    var l = lines[i];
                                    if (l.Contains(name)) lines.RemoveAt(i);
                                }
                            }
                        }
                    }

                    //write lines from file to output
                    if (lines.Any())
                    {
                        countAll += lines.Count;

                        Debug.WriteLine(file);
                        Debug.WriteLine("-------------------------------");
                        foreach (var l in lines)
                        {
                            Debug.WriteLine(l);
                        }

                        Debug.WriteLine("++++++++++++++++++++++++++++++++");
                    }
                }
            }

            Debug.WriteLine("********************");
            Debug.WriteLine("open: " + countAll);
            Debug.WriteLine("********************");
        }

        private class VisualStudioProvider
        {
            public static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
            {
                var directory = new DirectoryInfo(
                    currentPath ?? Directory.GetCurrentDirectory());
                while (directory != null && !directory.GetFiles("*.sln").Any())
                {
                    directory = directory.Parent;
                }
                return directory;
            }
        }
    }
}