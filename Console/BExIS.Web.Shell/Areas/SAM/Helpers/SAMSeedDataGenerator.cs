using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;

namespace BExIS.Modules.Sam.UI.Helpers
{
    public class SamSeedDataGenerator
    {
        public static void GenerateSeedData()
        {
            createSecuritySeedData();
        }

        private static void createSecuritySeedData()
        {
            // Javad:
            // 1) all the create operations should check for existence of the record
            // 2) failure on creating any record should rollback the whole seed data generation. It is one transaction.
            // 3) failues should throw an exception with enough information to pin point the root cause
            // 4) only seed data related to the functions of this modules should be genereated here.
            // BUG: seed data creation is not working because of the changes that were done in the entities and services.
            // TODO: reimplement the seed data creation method.

            //#region Security

            //// Entities
            var entityManager = new EntityManager();
            entityManager.Create(new Entity() { EntityType = typeof(Dataset), EntityStoreType = typeof(DatasetStore), Securable = true, UseMetadata = true });

            //// Groups
            var groupManager = new GroupManager();
            groupManager.Create(new Group() { Name = "everyone", Description = "everyone group", IsValid = true, GroupType = GroupType.Public });
            groupManager.Create(new Group() { Name = "Admin", Description = "Admin" });

            //// Features
            


            //// Users - DO NOT CREATE A USER, BECAUSE OF MISSING INFORMATION, E.G. EMAIL

            //// Tasks
            //TaskManager taskManager = new TaskManager();

            //taskManager.CreateTask("SAM", "Account", "*");
            //taskManager.CreateTask("Shell", "Nav", "*");
            //taskManager.CreateTask("Shell", "Home", "*");
            //taskManager.CreateTask("System", "Utils", "*");
            //taskManager.CreateTask("DCM", "Help", "*");
            //taskManager.CreateTask("DDM", "Help", "*");
            //taskManager.CreateTask("DIM", "Help", "*");
            //taskManager.CreateTask("RPM", "Help", "*");
            //taskManager.CreateTask("SAM", "Help", "*");
            //taskManager.CreateTask("Site", "Footer", "*");

            ////generic form for metadata
            //taskManager.CreateTask("DCM", "Form", "*");

            //// Features
            var featureManager = new FeatureManager();

            var f1 = new Feature() { Name = "BExIS", Description = "BExIS" };
            featureManager.Create(f1);

            var f2 = new Feature() { Name = "Search", Description = "Search", Parent = f1 };
            featureManager.Create(f2);

            //Feature f1 = featureManager.CreateFeature("BExIS", "BExIS");
            //Feature f9 = featureManager.CreateFeature("Search", "Search", f1.Id);

            //#region Data Planning

            //Feature f18 = featureManager.CreateFeature("Data Planning Management", "Data Planning Management", f1.Id);
            //Feature f13 = featureManager.CreateFeature("Datastructure Management", "Datastructure Management", f18.Id);

            //#endregion

            //#region Data Dissemination

            //Feature f16 = featureManager.CreateFeature("Data Dissemination", "Data Dissemination", f1.Id);

            //#endregion

            //#region Data Collection

            //Feature f10 = featureManager.CreateFeature("Data Collection", "Data Collection", f1.Id);
            //Feature f11 = featureManager.CreateFeature("Dataset Creation", "Dataset Creation", f10.Id);
            //Feature f12 = featureManager.CreateFeature("Dataset Upload", "Dataset Upload", f10.Id);
            //Feature f17 = featureManager.CreateFeature("Metadata Management", "Metadata Management", f10.Id);
            //#endregion

            //#region admin

            //Feature f2 = featureManager.CreateFeature("Administration", "Administration", f1.Id);
            //Feature f3 = featureManager.CreateFeature("Users Management", "Users Management", f2.Id);
            //Feature f4 = featureManager.CreateFeature("Groups Management", "Groups Management", f2.Id);
            //Feature f6 = featureManager.CreateFeature("Feature Permissions", "Feature Permissions", f2.Id);
            //Feature f5 = featureManager.CreateFeature("Data Permissions", "Data Permissions", f2.Id);
            //Feature f7 = featureManager.CreateFeature("Search Management", "Search Management", f2.Id);
            //Feature f8 = featureManager.CreateFeature("Dataset Maintenance", "Dataset Maintenance", f2.Id);

            //#endregion

            //Task t1 = taskManager.CreateTask("SAM", "Users", "*");
            //t1.Feature = f3;
            //taskManager.UpdateTask(t1);
            //Task t2 = taskManager.CreateTask("SAM", "Groups", "*");
            //t2.Feature = f4;
            //taskManager.UpdateTask(t2);
            //Task t3 = taskManager.CreateTask("SAM", "DataPermissions", "*");
            //t3.Feature = f5;
            //taskManager.UpdateTask(t3);
            //Task t4 = taskManager.CreateTask("SAM", "FeaturePermissions", "*");
            //t4.Feature = f6;
            //taskManager.UpdateTask(t4);
            //Task t5 = taskManager.CreateTask("DDM", "Admin", "*");
            //t5.Feature = f7;
            //taskManager.UpdateTask(t5);

            //Task t7 = taskManager.CreateTask("DDM", "Data", "*");
            //t7.Feature = f9;
            //taskManager.UpdateTask(t7);
            //Task t8 = taskManager.CreateTask("DDM", "Home", "*");
            //t8.Feature = f9;
            //taskManager.UpdateTask(t8);
            //Task t33 = taskManager.CreateTask("DDM", "CreateDataset", "*");
            //t33.Feature = f9;
            //taskManager.UpdateTask(t33);

            //Task t9 = taskManager.CreateTask("DCM", "CreateDataset", "*");
            //t9.Feature = f11;
            //taskManager.UpdateTask(t9);
            //Task t10 = taskManager.CreateTask("DCM", "CreateSelectDatasetSetup", "*");
            //t10.Feature = f11;
            //taskManager.UpdateTask(t10);
            //Task t11 = taskManager.CreateTask("DCM", "CreateSetMetadataPackage", "*");
            //t11.Feature = f11;
            //taskManager.UpdateTask(t11);
            //Task t12 = taskManager.CreateTask("DCM", "CreateSummary", "*");
            //t12.Feature = f11;
            //taskManager.UpdateTask(t12);
            
            //Task t15 = taskManager.CreateTask("DCM", "Push", "*");
            //t15.Feature = f12;
            //taskManager.UpdateTask(t15);
            //Task t16 = taskManager.CreateTask("DCM", "Submit", "*");
            //t16.Feature = f12;
            //taskManager.UpdateTask(t16);
            //Task t17 = taskManager.CreateTask("DCM", "SubmitDefinePrimaryKey", "*");
            //t17.Feature = f12;
            //taskManager.UpdateTask(t17);
            //Task t18 = taskManager.CreateTask("DCM", "SubmitGetFileInformation", "*");
            //t18.Feature = f12;
            //taskManager.UpdateTask(t18);
            //Task t19 = taskManager.CreateTask("DCM", "SubmitSelectAFile", "*");
            //t19.Feature = f12;
            //taskManager.UpdateTask(t19);
            //Task t20 = taskManager.CreateTask("DCM", "SubmitSpecifyDataset", "*");
            //t20.Feature = f12;
            //taskManager.UpdateTask(t20);
            //Task t21 = taskManager.CreateTask("DCM", "SubmitSummary", "*");
            //t21.Feature = f12;
            //taskManager.UpdateTask(t21);
            //Task t22 = taskManager.CreateTask("DCM", "SubmitValidation", "*");
            //t22.Feature = f12;
            //taskManager.UpdateTask(t22);

            //Task t23 = taskManager.CreateTask("RPM", "Home", "*");
            //t23.Feature = f18;
            //taskManager.UpdateTask(t23);
            //Task t24 = taskManager.CreateTask("RPM", "DataAttribute", "*");
            //t24.Feature = f18;
            //taskManager.UpdateTask(t24);
            //Task t25 = taskManager.CreateTask("RPM", "Unit", "*");
            //t25.Feature = f18;
            //taskManager.UpdateTask(t25);

            //Task t26 = taskManager.CreateTask("DIM", "Admin", "*");
            //t26.Feature = f16;
            //taskManager.UpdateTask(t26);

            //Task t27 = taskManager.CreateTask("DCM", "ImportMetadataStructure", "*");
            //t27.Feature = f17;
            //taskManager.UpdateTask(t27);

            //Task t28 = taskManager.CreateTask("DCM", "ImportMetadataStructureReadSource", "*");
            //t28.Feature = f17;
            //taskManager.UpdateTask(t28);

            //Task t29 = taskManager.CreateTask("DCM", "ImportMetadataStructureSelectAFile", "*");
            //t29.Feature = f17;
            //taskManager.UpdateTask(t29);

            //Task t30 = taskManager.CreateTask("DCM", "ImportMetadataStructureSetParameters", "*");
            //t30.Feature = f17;
            //taskManager.UpdateTask(t30);

            //Task t31 = taskManager.CreateTask("DCM", "ImportMetadataStructureSummary", "*");
            //t31.Feature = f17;
            //taskManager.UpdateTask(t31);

            //Task t32 = taskManager.CreateTask("SAM", "Dataset", "*");
            //t32.Feature = f8;
            //taskManager.UpdateTask(t32);

            //Task t35 = taskManager.CreateTask("RPM", "DataStructureSearch", "*");
            //t35.Feature = f13;
            //taskManager.UpdateTask(t35);

            //Task t36 = taskManager.CreateTask("RPM", "DataStructureEdit", "*");
            //t36.Feature = f13;
            //taskManager.UpdateTask(t36);

            //Task t37 = taskManager.CreateTask("RPM", "DataStructureIO", "*");
            //t37.Feature = f13;
            //taskManager.UpdateTask(t37);

            //Task t38 = taskManager.CreateTask("DCM", "ManageMetadataStructure", "*");
            //t38.Feature = f17;
            //taskManager.UpdateTask(t38);

            //// Feature Permissions
            //PermissionManager permissionManager = new PermissionManager();
            //permissionManager.CreateFeaturePermission(g1.Id, f1.Id);
            ////permissionManager.CreateFeaturePermission(everyone.Id, f9.Id);

            //#endregion Security
        }
    }
}