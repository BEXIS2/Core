using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExISMigration
{
    public class MigrationHelpers
    {
        // the dataStructure, dataSet or primaryData of these datasets will be transfered
        public List<string> getDatasets(string filePath)
        {
            List<string> datasets = new List<string>();

            filePath += @"\datasets.txt";
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            string readLine;
            while ((readLine = file.ReadLine()) != null)
                datasets.Add(readLine);
            file.Close();

            return datasets;
        }
    }
}
