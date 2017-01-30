using IBM.Data.DB2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExISMigration
{
    public class SecurityMigration
    {
        public class Right
        {
            public int DataSetId { get; set; }
            public string RoleName { get; set; }
            public bool CanEdit { get; set; }
        }
        internal List<string> GetBexisRoles(string DataBase)
        {
            List<string> bexisRoles = new List<string>();
            // DB query
            string mySelectQuery = "SELECT ROLENAME, APPLICATIONNAME FROM \"PROVIDER\".\"ROLES\"";
            DB2Connection connect = new DB2Connection(DataBase);
            DB2Command myCommand = new DB2Command(mySelectQuery, connect);
            connect.Open();
            DB2DataReader myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                bexisRoles.Add(myReader.GetValue(0).ToString());
            }
            myReader.Close();
            connect.Close();
            return bexisRoles;
        }

        internal List<string> GetBexisUsersInRole(string dataBase, string roleName)
        {
            List<string> bexisUsersInRole = new List<string>();
            // DB query
            string mySelectQuery = "SELECT USERNAME FROM \"PROVIDER\".\"USERSINROLES\" where ROLENAME='" + roleName + "'";
            DB2Connection connect = new DB2Connection(dataBase);
            DB2Command myCommand = new DB2Command(mySelectQuery, connect);
            connect.Open();
            DB2DataReader myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                bexisUsersInRole.Add(myReader.GetValue(0).ToString());
            }
            myReader.Close();
            connect.Close();
            return bexisUsersInRole;
        }

        internal List<Right> GetBexisRights(string dataBase,  Dictionary<int, int> dataSetsMapping)
        {
            List<Right> bexisRights = new List<Right>();
            string datasetQuery = "";
            foreach (var dataSetMapping in dataSetsMapping)
            {
                datasetQuery += "DATASETID = "+ dataSetMapping.Key;
                if (dataSetsMapping.Last().Key != dataSetMapping.Key)
                    datasetQuery += " or ";
            }
            if (dataSetsMapping.Any())
            {
                datasetQuery = "where " + datasetQuery + "";
            }
            // DB query
            string mySelectQuery = "SELECT ROLENAME, DATASETID, FOREDIT, APPLICATIONNAME FROM \"PROVIDER\".\"RIGHTS\"   "+ datasetQuery;
            DB2Connection connect = new DB2Connection(dataBase);
            DB2Command myCommand = new DB2Command(mySelectQuery, connect);
            connect.Open();
            DB2DataReader myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                bexisRights.Add(new Right()
                {
                    RoleName = myReader.GetString(0),
                    DataSetId = (int)(myReader.GetValue(1)),
                    CanEdit = myReader.GetString(2)=="N"?false:true
                });
            }
            myReader.Close();
            connect.Close();
            return bexisRights;
        }
    }
}
