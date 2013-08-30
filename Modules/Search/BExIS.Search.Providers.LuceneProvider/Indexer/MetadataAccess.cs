using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;

using IBM.Data.DB2;
using IBM.Data.DB2Types;
using BExIS.Dlm.Services.Data;

namespace BExIS.Search.Providers.LuceneProvider.Indexer
{
    class metadataAccess
    {

        private DB2Command cmd = null;
        private DB2Connection con = null;
        private DB2DataReader rdr = null;
        private DB2Xml xmlColValue;
        private String xmlString;
        private Hashtable hashtable = new Hashtable();



        public metadataAccess()
        {
            /*
                        data = new Hashtable();
                        XmlDocument metadataXml1 = new XmlDocument();
                        metadataXml1.Load("C:\\Users\\standard\\Downloads\\bexis++\\trunk\\docs\\Sprints\\Sprint11\\SearchFeature1\\BExIS.Search.UI\\App_Data\\xml\\metadataXml1.xml");
                        data["metadataXml1"] = metadataXml1;

                        XmlDocument metadataXml2 = new XmlDocument();
                        metadataXml2.Load("C:\\Users\\standard\\Downloads\\bexis++\\trunk\\docs\\Sprints\\Sprint11\\SearchFeature1\\BExIS.Search.UI\\App_Data\\xml\\metadataXml2.xml");
                        data["metadataXml2"] = metadataXml2;

                        XmlDocument metadataXml3 = new XmlDocument();
                        metadataXml3.Load("C:\\Users\\standard\\Downloads\\bexis++\\trunk\\docs\\Sprints\\Sprint11\\SearchFeature1\\BExIS.Search.UI\\App_Data\\xml\\metadataXml3.xml");
                        data["metadataXml3"] = metadataXml3 ;

                        XmlDocument metadataXml4 = new XmlDocument();
                        metadataXml4.Load("C:\\Users\\standard\\Downloads\\bexis++\\trunk\\docs\\Sprints\\Sprint11\\SearchFeature1\\BExIS.Search.UI\\App_Data\\xml\\metadataXml4.xml");
                        data["metadataXml4"] = metadataXml4;

                        XmlDocument metadataXml100 = new XmlDocument();
                        metadataXml100.Load("C:\\Users\\standard\\Downloads\\bexis++\\trunk\\docs\\Sprints\\Sprint11\\SearchFeature1\\BExIS.Search.UI\\App_Data\\xml\\metadataXml100.xml");
                        data["metadataXml100"] = metadataXml100;

                        XmlDocument metadataXml101 = new XmlDocument();
                        metadataXml101.Load("C:\\Users\\standard\\Downloads\\bexis++\\trunk\\docs\\Sprints\\Sprint11\\SearchFeature1\\BExIS.Search.UI\\App_Data\\xml\\metadataXml101.xml");
                        data["metadataXml101"] = metadataXml101;

                        XmlDocument metadataXml102 = new XmlDocument();
                        metadataXml102.Load("C:\\Users\\standard\\Downloads\\bexis++\\trunk\\docs\\Sprints\\Sprint11\\SearchFeature1\\BExIS.Search.UI\\App_Data\\xml\\metadataXml102.xml");
                        data["metadataXml102"] = metadataXml102;

                        XmlDocument metadataXml103 = new XmlDocument();
                        metadataXml103.Load("C:\\Users\\standard\\Downloads\\bexis++\\trunk\\docs\\Sprints\\Sprint11\\SearchFeature1\\BExIS.Search.UI\\App_Data\\xml\\metadataXml103.xml");
                        data["metadataXml103"] = metadataXml103;
                        XmlDocument metadataXml104 = new XmlDocument();
                        metadataXml104.Load("C:\\Users\\standard\\Downloads\\bexis++\\trunk\\docs\\Sprints\\Sprint11\\SearchFeature1\\BExIS.Search.UI\\App_Data\\xml\\metadataXml104.xml");
                        data["metadataXml104"] = metadataXml104;
                        XmlDocument metadataXml105 = new XmlDocument();
                        metadataXml105.Load("C:\\Users\\standard\\Downloads\\bexis++\\trunk\\docs\\Sprints\\Sprint11\\SearchFeature1\\BExIS.Search.UI\\App_Data\\xml\\metadataXml105.xml");
                        data["metadataXml105"] = metadataXml105;
             */

            DateTime start = DateTime.Now;
            try
            {
                DatasetManager dm = new DatasetManager();
                // its better for the GetDatasetLatestMetadataVersions function to return a dictionary and also for the hashtable to be changed to a dictionary in order to reduce data conversions. Javad
                dm.GetDatasetLatestMetadataVersions().ForEach(p => hashtable.Add(p.Key, p.Value));

                

                //con = new DB2Connection("Database=BPP;UID=standard;PWD=ibukun;Server=localhost:50000;");
                //cmd = new DB2Command();
                //cmd.Connection = con;
                //Boolean a = con.IsOpen;
                //cmd.CommandText = "select A.ID, A.METADATA from STANDARD.DATASETS AS A WHERE A.ID > 2";
                //cmd.CommandTimeout = 20;
                //con.Open();
                //Boolean b = con.IsOpen;
                //rdr = cmd.ExecuteReader();
                //start = DateTime.Now;
                //int datasetId = 0;
                //while (rdr.Read() == true)
                //{
                //    datasetId = datasetId + 1;
                //    xmlColValue = rdr.GetDB2Xml(1);
                //    xmlString = xmlColValue.GetString();
                //    hashtable[datasetId] = xmlString;
                //}
                //rdr.Close();
                //con.Close();
            }
            catch (Exception eb)
            {
                // lets move or change this kind of writing errors to some proper ways, like throwing user friendly errors or to write to Debug.xx or to log them...
                Console.WriteLine("\n Source: " + eb.Source + "\n Message: " + eb.Message);
            }



        }


        public Hashtable getMetadata()
        {
            return hashtable;
        }

    }
}

