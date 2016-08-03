using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IBM.Data.DB2;
using IBM.Data.DB2Types;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Authorization;

namespace BExISMigration
{
    public class UserProperties
    {
        // bexis1 DB user data
        public string username { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string organization { get; set; }
        public string projectname { get; set; }
        public string projectleader { get; set; }
        public string url { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string fax { get; set; }
        public string original { get; set; }
        public string street { get; set; }
        public string zipcode { get; set; }
        public string city { get; set; }
        // bexis2 required security data
        public string password { get; set; }
        public long securityQuestionId { get; set; }
        public string securityAnswer { get; set; }
        public long authenticatorId { get; set; }
    }


    public class UserMigration
    {
        // create or get and return the group named "bexisUser"
        public long bexisUserGroup()
        {
            SubjectManager subjectManager = new SubjectManager();

            string groupName = "bexisUser";
            string groupDescription = "user of old BExIS";

            Group group = subjectManager.GroupsRepo.Get(g => groupName.Equals(g.Name)).FirstOrDefault();

            if (group == null)
            {
                group = subjectManager.CreateGroup(groupName, groupDescription);
            }

            return group.Id;
        }

        // set feature permission of the group "bexisUser"
        public void SetFeaturePermissions(long groupId, string[] featureNames)
        {
            FeatureManager featureManager = new FeatureManager();
            PermissionManager permissionManager = new PermissionManager();

            // never set administration features (this parent Id)
            long parentId = featureManager.FeaturesRepo.Get().Where(f => f.Name.Equals("Administration")).FirstOrDefault().Id;

            for (int i = 0; i < featureNames.Length; i++)
            {
                long featureId = featureManager.FeaturesRepo.Get(f => featureNames[i].Equals(f.Name) && !f.Parent.Id.Equals(parentId)).FirstOrDefault().Id;
                permissionManager.CreateFeaturePermission(groupId, featureId);
            }
        }

        // query bexis1 user from provider.users and generate a random password
        public List<UserProperties> GetFromBExIS(string DataBase)
        {
            List<UserProperties> transferUsers = new List<UserProperties>();

            // DB query
            string mySelectQuery = "select username, email, firstname, lastname, " +
                                   "organization, projectname, projectleader, " +
                                   "url, phone, mobile, fax, original, street, zipcode, city";
            mySelectQuery += " from provider.users;";
            DB2Connection connect = new DB2Connection(DataBase);
            DB2Command myCommand = new DB2Command(mySelectQuery, connect);
            connect.Open();
            DB2DataReader myReader = myCommand.ExecuteReader();
            // random password
            Random gen = new Random();
            while (myReader.Read())
            {
                UserProperties transferUser = new UserProperties();

                // bexis1 DB user data
                transferUser.username = myReader.GetValue(0).ToString();
                transferUser.email = myReader.GetValue(1).ToString();
                transferUser.firstname = myReader.GetValue(2).ToString();
                transferUser.lastname = myReader.GetValue(3).ToString();
                transferUser.organization = myReader.GetValue(4).ToString();
                transferUser.projectname = myReader.GetValue(5).ToString();
                transferUser.projectleader = myReader.GetValue(6).ToString();
                transferUser.url = myReader.GetValue(7).ToString();
                transferUser.phone = myReader.GetValue(8).ToString();
                transferUser.mobile = myReader.GetValue(9).ToString();
                transferUser.fax = myReader.GetValue(10).ToString();
                transferUser.original = myReader.GetValue(11).ToString();
                transferUser.street = myReader.GetValue(12).ToString();
                transferUser.zipcode = myReader.GetValue(13).ToString();
                transferUser.city = myReader.GetValue(14).ToString();
                // bexis2 required security data
                transferUser.password = randomPassword(ref gen); // random password
                transferUser.securityQuestionId = 1;
                transferUser.securityAnswer = "1";
                transferUser.authenticatorId = 1;

                // add to list; username required
                if (transferUser.username != "")
                    transferUsers.Add(transferUser);
            }
            myReader.Close();
            connect.Close();

            return transferUsers;
        }

        // transfer users to bpp; not all user data are provided in bpp
        // save usernames and passwords in file "passwords.txt"
        public void CreateOnBPP(List<UserProperties> transferUsers, string filePath, long groupId)
        {
            SubjectManager subjectManager = new SubjectManager();
            StreamWriter file = new StreamWriter(filePath + @"\passwords.txt");

            foreach (UserProperties transferUser in transferUsers)
            {
                // transfer user if not exist
                if (!subjectManager.ExistsUserName(transferUser.username))
                {
                    // create user
                    User user = subjectManager.CreateUser(
                        transferUser.username,
                        transferUser.password,
                        transferUser.firstname + " " + transferUser.lastname,
                        transferUser.email,
                        transferUser.securityQuestionId,
                        transferUser.securityAnswer,
                        transferUser.authenticatorId
                        );

                    // add user to group; the group "bexisUser" must be created manually
                    subjectManager.AddUserToGroup(user.Id, groupId);
                    // write username and generated password to file "passwords.txt"
                    file.WriteLine(transferUser.username + ",\t" + transferUser.password);
                }
            }

            file.Close();
        }


        /////////////////////////////////////////////////////////////////////////
        // helper methods
        #region helper methods

        private string randomPassword(ref Random gen)
        {
            int passwordLength = 8;
            string newPassword = "";
            string allowedChars = "";

            allowedChars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            allowedChars += "abcdefghijklmnopqrstuvwxyz";
            allowedChars += "0123456789";
            allowedChars += "-_.:,;!?=";

            //Random gen = new Random();
            for (int i = 1; i <= passwordLength; i++)
            {
                newPassword += allowedChars[gen.Next(0, allowedChars.Length)];
            }

            return newPassword;
        }

        #endregion
    }
}
