using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;

namespace BExIS.Web.Shell.Areas.Auth.Helpers
{
    public class AuthSeedDataGenerator
    {
       

        public static void GenerateSeedData()
        {
            createSecuritySeedData();
        }

        private static void createSecuritySeedData()
        {

            #region Security

            // Authenticators
            AuthenticatorManager authenticatorManager = new AuthenticatorManager();

            Authenticator a1 = authenticatorManager.CreateAuthenticator("local", "BExIS.Security.Services.Authentication.BuiltInAuthenticationManager", "BExIS.Security.Services", "", AuthenticatorType.Internal);
            Authenticator a2 = authenticatorManager.CreateAuthenticator("ldap test server", "BExIS.Security.Services.Authentication.LdapAuthenticationManager", "BExIS.Security.Services", "ldapHost:ldap.forumsys.com;ldapPort:389;ldapBaseDn:dc=example,dc=com;ldapSecure:false;ldapAuthUid:uid;ldapProtocolVersion:3", AuthenticatorType.External);

            // Security Questions
            SecurityQuestionManager securityQuestionManager = new SecurityQuestionManager();

            SecurityQuestion sq1 = securityQuestionManager.CreateSecurityQuestion("What is the first name of the person you first kissed?");
            SecurityQuestion sq2 = securityQuestionManager.CreateSecurityQuestion("What was your favorite place to visit as a child?");
            SecurityQuestion sq3 = securityQuestionManager.CreateSecurityQuestion("What is the name of the place your wedding reception was held?");
            SecurityQuestion sq4 = securityQuestionManager.CreateSecurityQuestion("In what city or town did you meet your spouse/partner?");
            SecurityQuestion sq5 = securityQuestionManager.CreateSecurityQuestion("What was the make and model of your first car?");
            SecurityQuestion sq6 = securityQuestionManager.CreateSecurityQuestion("What was the name of your elementary / primary school?");

            // Entities
            EntityManager entityManager = new EntityManager();

            entityManager.CreateEntity("Dataset", "BExIS.Dlm.Entities.Data.Dataset", "BExIS.Dlm.Entities");

            // Subjects
            SubjectManager subjectManager = new SubjectManager();

            Group g1 = subjectManager.CreateGroup("Admin", "Admin");
            User u1 = subjectManager.CreateUser("Administrator", "gWg2xG", "Admin", "admin@bexis.de", sq1.Id, "Nothing", a1.Id);

            subjectManager.AddUserToGroup(u1.Id, g1.Id);

            // Tasks
            TaskManager taskManager = new TaskManager();

            taskManager.CreateTask("Auth", "Account", "*", true);
            taskManager.CreateTask("Site", "Nav", "*", true);
            taskManager.CreateTask("Shell", "Home", "*", true);
            taskManager.CreateTask("System", "Utils", "*", true);

            // Features
            FeatureManager featureManager = new FeatureManager();
            Feature f1 = featureManager.CreateFeature("BExIS", "BExIS");

            Feature f2 = featureManager.CreateFeature("Administration", "Administration", f1.Id);
            Feature f3 = featureManager.CreateFeature("Users Management", "Users Management", f2.Id);
            Feature f4 = featureManager.CreateFeature("Groups Management", "Groups Management", f2.Id);
            Feature f5 = featureManager.CreateFeature("Data Management", "Data Management", f2.Id);
            Feature f6 = featureManager.CreateFeature("Feature Management", "Feature Management", f2.Id);
            Feature f7 = featureManager.CreateFeature("Search", "Search", f2.Id);
            Feature f8 = featureManager.CreateFeature("Seed Data Creation", "Seed Data Creation", f2.Id);

            Feature f9 = featureManager.CreateFeature("Search", "Search", f1.Id);

            Feature f10 = featureManager.CreateFeature("Data Collection", "Data Collection", f1.Id);
            Feature f11 = featureManager.CreateFeature("Dataset Creation", "Dataset Creation", f10.Id);
            Feature f12 = featureManager.CreateFeature("Dataset Submission", "Dataset Submission", f10.Id);

            Feature f13 = featureManager.CreateFeature("Research Plan", "Research Plan", f1.Id);

            Feature f14 = featureManager.CreateFeature("Data Dissemination", "Data Dissemination", f1.Id);
            
            Task t1 = taskManager.CreateTask("Auth", "Users", "*");
            t1.Feature = f3;
            taskManager.UpdateTask(t1);
            Task t2 = taskManager.CreateTask("Auth", "Groups", "*");
            t2.Feature = f4;
            taskManager.UpdateTask(t2);
            Task t3 = taskManager.CreateTask("Auth", "DataPermissions", "*");
            t3.Feature = f5;
            taskManager.UpdateTask(t3);
            Task t4 = taskManager.CreateTask("Auth", "FeaturePermissions", "*");
            t4.Feature = f6;
            taskManager.UpdateTask(t4);
            Task t5 = taskManager.CreateTask("DDM", "Admin", "*");
            t5.Feature = f7;
            taskManager.UpdateTask(t5);
            Task t6 = taskManager.CreateTask("SAM", "Seed", "*");
            t6.Feature = f8;
            taskManager.UpdateTask(t6);


            Task t7 = taskManager.CreateTask("DDM", "Data", "*");
            t7.Feature = f9;
            taskManager.UpdateTask(t7);
            Task t8 = taskManager.CreateTask("DDM", "Home", "*");
            t8.Feature = f9;
            taskManager.UpdateTask(t8);

            Task t9 = taskManager.CreateTask("DCM", "Create", "*");
            t9.Feature = f11;
            taskManager.UpdateTask(t9);
            Task t10 = taskManager.CreateTask("DCM", "CreateSelectDatasetSetup", "*");
            t10.Feature = f11;
            taskManager.UpdateTask(t10);
            Task t11 = taskManager.CreateTask("DCM", "CreateSetMetadataPackage", "*");
            t11.Feature = f11;
            taskManager.UpdateTask(t11);
            Task t12 = taskManager.CreateTask("DCM", "CreateSummary", "*");
            t12.Feature = f11;
            taskManager.UpdateTask(t12);

            Task t15 = taskManager.CreateTask("DCM", "Push", "*");
            t15.Feature = f12;
            taskManager.UpdateTask(t15);
            Task t16 = taskManager.CreateTask("DCM", "Submit", "*");
            t16.Feature = f12;
            taskManager.UpdateTask(t16);
            Task t17 = taskManager.CreateTask("DCM", "SubmitDefinePrimaryKey", "*");
            t17.Feature = f12;
            taskManager.UpdateTask(t17);
            Task t18 = taskManager.CreateTask("DCM", "SubmitGetFileInformation", "*");
            t18.Feature = f12;
            taskManager.UpdateTask(t18);
            Task t19 = taskManager.CreateTask("DCM", "SubmitSelectAFile", "*");
            t19.Feature = f12;
            taskManager.UpdateTask(t19);
            Task t20 = taskManager.CreateTask("DCM", "SubmitSpecifyDataset", "*");
            t20.Feature = f12;
            taskManager.UpdateTask(t20);
            Task t21 = taskManager.CreateTask("DCM", "SubmitSummary", "*");
            t21.Feature = f12;
            taskManager.UpdateTask(t21);
            Task t22 = taskManager.CreateTask("DCM", "SubmitValidation", "*");
            t22.Feature = f12;
            taskManager.UpdateTask(t22);

            Task t23 = taskManager.CreateTask("RPM", "Home", "*");
            t23.Feature = f13;
            taskManager.UpdateTask(t23);

            Task t24 = taskManager.CreateTask("DIM", "Admin", "*");
            t24.Feature = f14;
            taskManager.UpdateTask(t24);

            // Feature Permissions
            PermissionManager permissionManager = new PermissionManager();
            permissionManager.CreateFeaturePermission(g1.Id, f1.Id);

            #endregion
        }

    }
}