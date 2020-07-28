# Version Changes 2.13.0 -> 2.14.0

^ in the document not all changes are indicated in the first section, only supposed important changes for other developers are indicated. The second section lists all files that have changed since the last release.

# May Important Changes

## Shell

- Account controller add [ValidateInput(false)] to login and register function
- HomeController add Version action

- dbsettings: changing the batchs size

````xml
<property name="adonet.batch_size">100000</property>

````

- settings 
````xml
 <entry key="version" value="2.14" type="string" />
````

- account & register model - add allow html attr to password
````cs
[System.Web.Mvc.AllowHtml]
        public string Password { get; set; }
````



## Core / Components 

### AAA
- entity store
    * int CountEntities();
- Add Version Entity with mapping file and managers
- Add DisplayName To Subject
- EntityPermissionsMangaer functions changed
- GroupManager changed
- UserManager changed
- BexisEntityAuthorizeAttribute changed
    - no need for entity name like dataset

### Dlm
- datatuples and version 
    * they no longer store the data in xml but in json. also a values column is included to speed up the building of mat.views
- datasetsversions have title and description as column
    * previously the attributes were always taken from the metadata

## DCM
### settings
- add celllimit to the setting
````xml
<!-- This number determines the selection whether an upload of structured data should be executed directly or async. 
      - c < direct
      - c > async 
      -->
  <entry key="celllimit" value="100000" type="Int32" />
````

## DDM
- change bexis indexer
### Settings

- show metadata tabs show or hide, when user has no access
````xml
  <entry key="show_primary_data_tab" value="true" type="String"/>
  <entry key="show_data_structure_tab" value="true" type="String"/>  
  <entry key="show_link_tab" value="true" type="String"/>
  <entry key="show_permission_tab" value="true" type="String"/>
  <entry key="show_publish_tab" value="true" type="String"/>
  <entry key="show_attachments_tab" value="true" type="String"/>

  <entry key="show_tabs_deactivated" value="true" type="String"/>
````

## RPM
### Settings
- Parameter to set otional as default or not : <optionalDefault>true</optionalDefault>

### Units
- Edit View: dimension validation with regex

## DB
[go to the db update script v2.13-2.14](https://github.com/BEXIS2/Core/blob/releases/2.14/database%20update%20scripts/Update_Script_213to214.sql)

## WORKSPACE

````
+8 −0  Components/Dlm/Db/Mappings/Default/Data/DatasetVersion.hbm.xml
+1 −1  Components/Dlm/Db/Mappings/Default/DataStructure/Constraint.PatternConstraint.hbm.xml
+1 −1  Components/Dlm/Db/Mappings/Default/Party/Party.hbm.xml
+3 −1  Components/Security/Db/Mappings/Default/Subjects/Group.hbm.xml
+1 −0  Components/Security/Db/Mappings/Default/Subjects/Subject.hbm.xml
+20 −0  Components/Security/Db/Mappings/Default/Versions/Version.hbm.xml
+1 −1  General/Db/Settings/PostgreSQL82Dialect.hibernate.cfg.xml
+3 −0  General/General.Settings.xml
+1 −1  Modules/BAM/Bam.Settings.xml
+6 −2  Modules/DCM/Dcm.Settings.xml
+11 −3  Modules/DDM/Ddm.Settings.xml
+3 −3  Modules/DIM/Db/Mappings/Default/Mapping/Mapping.hbm.xml
+1 −1  Modules/DIM/Dim.Settings.xml
+2 −1  Modules/RPM/Rpm.Settings.xml
+26 −26  Modules/RPM/dimensions.csv
+17 −20  Modules/RPM/units.csv
+1 −1  Modules/SAM/Sam.Settings.xml
+1 −1  Modules/VIM/Vim.Settings.xml

````

## UI

## web.config

- change web config changes listed here
````xml

<add key="ApplicationVersion" value="2.14" />

...



 <!-- Allow to use the user email within the party person; change party email on changed user email -->
    <add key="usePersonEmailAttributeName" value ="false"/>
    <add key="PersonEmailAttributeName" value ="Email"/>

<!-- Allow to use the user email within the party person; change party email on changed user email -->
    <add key="usePersonEmailAttributeName" value ="false"/>
    <add key="PersonEmailAttributeName" value ="Email"/>

...


 <remove name="WebDAV" />

````


# All CHANGED Files 

$ git diff --stat origin/releases/2.13..origin/releases/2.14 --no-merges

 | File| Changes |
 |---|---|
 | BExIS++.sln                                        |    4 +- 
 | .../BExIS.Security.Entities.csproj                 |    1 +
 | .../BExIS.Security.Entities/Requests/Decision.cs   |    3 +-
 | .../BExIS.Security.Entities/Requests/Request.cs    |    3 +-
 | .../BExIS.Security.Entities/Subjects/Subject.cs    |    1 +
 | .../AAA/BExIS.Security.Entities/Subjects/User.cs   |    5 +
 | .../BExIS.Security.Entities/Versions/Version.cs    |   18 +
 | .../BExIS.Security.Orm.NH.csproj                   |    1 +
 | .../Mappings/Default/Subjects/Group.hbm.xml        |    4 +-
 | .../Mappings/Default/Subjects/Subject.hbm.xml      |    1 +
 | .../Mappings/Default/Versions/Version.hbm.xml      |   20 +
 | .../BExIS.Security.Services.Tests.csproj           |    6 +-
 | .../BExIS.Security.Services.Tests/packages.config  |    2 +
 | .../Authorization/EntityPermissionManager.cs       |   66 +-
 | .../BExIS.Security.Services.csproj                 |    1 +
 | .../Objects/DatasetStore.cs                        |   10 +
 | .../Objects/IEntityStore.cs                        |    2 +
 | .../Requests/DecisionManager.cs                    |   32 +
 | .../Requests/RequestManager.cs                     |    3 +-
 | .../Subjects/GroupManager.cs                       |    4 +-
 | .../Subjects/UserManager.cs                        |    4 +-
 | .../Utilities/EmailService.cs                      |   17 +
 | .../Utilities/MessageHelper.cs                     |  116 +-
 | .../Versions/VersionManager.cs                     |  124 +
 | .../Attributes/BExISEntityAuthorizeAttribute.cs    |    8 +-
 | .../BExIS.App.Bootstrap/BExIS.App.Bootstrap.csproj |    9 +-
 Components/App/BExIS.App.Bootstrap/app.config      |   12 +
 Components/App/BExIS.App.Bootstrap/packages.config |    3 +
 | .../App/BExIS.App.Testing/BExIS.App.Testing.csproj |    9 +-
 Components/App/BExIS.App.Testing/app.config        |   16 +
 Components/App/BExIS.App.Testing/packages.config   |    3 +
 | .../BExIS.Dlm.Entities/BExIS.Dlm.Entities.csproj   |    7 +
 | .../DLM/BExIS.Dlm.Entities/Data/DataTuple.cs       |  151 +-
 | .../BExIS.Dlm.Entities/Data/DataTupleVersion.cs    |   10 +
 | .../DLM/BExIS.Dlm.Entities/Data/DataValue.cs       |   13 +-
 | .../DLM/BExIS.Dlm.Entities/Data/DatasetVersion.cs  |    8 +-
 | .../DLM/BExIS.Dlm.Entities/Data/VariableValue.cs   |   22 +-
 Components/DLM/BExIS.Dlm.Entities/packages.config  |    1 +
 | .../DLM/BExIS.Dlm.Orm.NH/BExIS.Dlm.Orm.NH.csproj   |    1 +
 BExIS++.sln                                        |    4 +-
 | .../BExIS.Security.Entities.csproj                 |    1 +
 | .../BExIS.Security.Entities/Requests/Decision.cs   |    3 +-
 | .../BExIS.Security.Entities/Requests/Request.cs    |    3 +-
 | .../BExIS.Security.Entities/Subjects/Subject.cs    |    1 +
 | .../AAA/BExIS.Security.Entities/Subjects/User.cs   |    5 +
 | .../BExIS.Security.Entities/Versions/Version.cs    |   18 +
 | .../BExIS.Security.Orm.NH.csproj                   |    1 +
 | .../Mappings/Default/Subjects/Group.hbm.xml        |    4 +-
 | .../Mappings/Default/Subjects/Subject.hbm.xml      |    1 +
 | .../Mappings/Default/Versions/Version.hbm.xml      |   20 +
 | .../BExIS.Security.Services.Tests.csproj           |    6 +-
 | .../BExIS.Security.Services.Tests/packages.config  |    2 +
 | .../Authorization/EntityPermissionManager.cs       |   66 +-
 | .../BExIS.Security.Services.csproj                 |    1 +
 | .../Objects/DatasetStore.cs                        |   10 +
 | .../Objects/IEntityStore.cs                        |    2 +
 | .../Requests/DecisionManager.cs                    |   32 +
 | .../Requests/RequestManager.cs                     |    3 +-
 | .../Subjects/GroupManager.cs                       |    4 +-
 | .../Subjects/UserManager.cs                        |    4 +-
 | .../Utilities/EmailService.cs                      |   17 +
 | .../Utilities/MessageHelper.cs                     |  116 +-
 | .../Versions/VersionManager.cs                     |  124 +
 | .../Attributes/BExISEntityAuthorizeAttribute.cs    |    8 +-
 | .../BExIS.App.Bootstrap/BExIS.App.Bootstrap.csproj |    9 +-
 Components/App/BExIS.App.Bootstrap/app.config      |   12 +
 Components/App/BExIS.App.Bootstrap/packages.config |    3 +
 | .../App/BExIS.App.Testing/BExIS.App.Testing.csproj |    9 +-
 Components/App/BExIS.App.Testing/app.config        |   16 +
 Components/App/BExIS.App.Testing/packages.config   |    3 +
 | .../BExIS.Dlm.Entities/BExIS.Dlm.Entities.csproj   |    7 +
 | .../DLM/BExIS.Dlm.Entities/Data/DataTuple.cs       |  151 +-
 | .../BExIS.Dlm.Entities/Data/DataTupleVersion.cs    |   10 +
 | .../DLM/BExIS.Dlm.Entities/Data/DataValue.cs       |   13 +-
 | .../DLM/BExIS.Dlm.Entities/Data/DatasetVersion.cs  |    8 +-
 | .../DLM/BExIS.Dlm.Entities/Data/VariableValue.cs   |   22 +-
 Components/DLM/BExIS.Dlm.Entities/packages.config  |    1 +
 | .../DLM/BExIS.Dlm.Orm.NH/BExIS.Dlm.Orm.NH.csproj   |    1 +
 | .../Mappings/Default/Data/DataTuple.hbm.xml        |   25 +-
 | .../Mappings/Default/Data/DataTupleVersion.hbm.xml |    9 +-
 | .../Mappings/Default/Data/DatasetVersion.hbm.xml   |    8 +
 | .../Constraint.PatternConstraint.hbm.xml           |    2 +-
 | .../DataStructure.StructuredDataStructure.hbm.xml  |    4 +-
 | .../Default/DataStructure/Variable.hbm.xml         |    4 +-
 | .../Mappings/Default/Party/Party.hbm.xml           |    2 +-
 | .../Utils/MaterializedViewHelper.cs                |   71 +-
 Components/DLM/BExIS.Dlm.Orm.NH/app.config         |   11 +
 | .../DLM/BExIS.Dlm.Services/Data/DatasetManager.cs  |  370 +-
 | .../DataStructure/MissingValueManager.cs           |   22 +-
 | .../DLM/BExIS.Dlm.Services/Party/PartyManager.cs   |   21 +-
 | .../BExIS.Dlm.Services/Party/PartyTypeManager.cs   |    6 +-
 | .../DLM/BExIS.Dlm.Tests/BExIS.Dlm.Tests.csproj     |   11 +-
 | .../DLM/BExIS.Dlm.Tests/Helpers/DatasetHelper.cs   |  118 +-
 | .../Services/Data/DataTupleTests.cs                |  163 +
 | .../Services/Data/DatasetManagerTests.cs           |   10 +-
 | .../Data/DatasetManager_EditDatasetVersionTests.cs |  189 +
 | .../Data/DatasetManager_GetDataTuplesTest.cs       |  246 +
 ...ger_GetDatasetVersionEffectiveDataTuplesTest.cs |  252 +
 Components/DLM/BExIS.Dlm.Tests/packages.config     |    2 +
 | .../IO/BExIS.IO.DataType.DisplayPattern/app.config |    8 +
 Components/IO/BExIS.IO.Tests/BExIS.IO.Tests.csproj |   16 +-
 Components/IO/BExIS.IO.Tests/IOUtilityTests.cs     |   42 +-
 | .../Transform/Input/AsciiReaderEncodingTest.cs     |  184 +
 | .../Transform/Input/DataReaderTests.cs             |    6 +
 Components/IO/BExIS.IO.Tests/packages.config       |    4 +-
 | .../IO/BExIS.IO.Transform.Input/AsciiReader.cs     |   52 +-
 | .../IO/BExIS.IO.Transform.Input/DataReader.cs      |   22 +-
 | .../IO/BExIS.IO.Transform.Input/ExcelReader.cs     |   45 +-
 Components/IO/BExIS.IO.Transform.Input/app.config  |    8 +
 BExIS++.sln                                        |    4 +-
 | .../BExIS.Security.Entities.csproj                 |    1 +
 | .../BExIS.Security.Entities/Requests/Decision.cs   |    3 +-
 | .../BExIS.Security.Entities/Requests/Request.cs    |    3 +-
 | .../BExIS.Security.Entities/Subjects/Subject.cs    |    1 +
 | .../AAA/BExIS.Security.Entities/Subjects/User.cs   |    5 +
 | .../BExIS.Security.Entities/Versions/Version.cs    |   18 +
 | .../BExIS.Security.Orm.NH.csproj                   |    1 +
 | .../Mappings/Default/Subjects/Group.hbm.xml        |    4 +-
 | .../Mappings/Default/Subjects/Subject.hbm.xml      |    1 +
 | .../Mappings/Default/Versions/Version.hbm.xml      |   20 +
 | .../BExIS.Security.Services.Tests.csproj           |    6 +-
 | .../BExIS.Security.Services.Tests/packages.config  |    2 +
 | .../Authorization/EntityPermissionManager.cs       |   66 +-
 | .../BExIS.Security.Services.csproj                 |    1 +
 | .../Objects/DatasetStore.cs                        |   10 +
 | .../Objects/IEntityStore.cs                        |    2 +
 | .../Requests/DecisionManager.cs                    |   32 +
 | .../Requests/RequestManager.cs                     |    3 +-
 | .../Subjects/GroupManager.cs                       |    4 +-
 | .../Subjects/UserManager.cs                        |    4 +-
 | .../Utilities/EmailService.cs                      |   17 +
 | .../Utilities/MessageHelper.cs                     |  116 +-
 | .../Versions/VersionManager.cs                     |  124 +
 | .../Attributes/BExISEntityAuthorizeAttribute.cs    |    8 +-
 | .../BExIS.App.Bootstrap/BExIS.App.Bootstrap.csproj |    9 +-
 Components/App/BExIS.App.Bootstrap/app.config      |   12 +
 Components/App/BExIS.App.Bootstrap/packages.config |    3 +
 | .../App/BExIS.App.Testing/BExIS.App.Testing.csproj |    9 +-
 Components/App/BExIS.App.Testing/app.config        |   16 +
 Components/App/BExIS.App.Testing/packages.config   |    3 +
 | .../BExIS.Dlm.Entities/BExIS.Dlm.Entities.csproj   |    7 +
 | .../DLM/BExIS.Dlm.Entities/Data/DataTuple.cs       |  151 +-
 | .../BExIS.Dlm.Entities/Data/DataTupleVersion.cs    |   10 +
 | .../DLM/BExIS.Dlm.Entities/Data/DataValue.cs       |   13 +-
 | .../DLM/BExIS.Dlm.Entities/Data/DatasetVersion.cs  |    8 +-
 | .../DLM/BExIS.Dlm.Entities/Data/VariableValue.cs   |   22 +-
 Components/DLM/BExIS.Dlm.Entities/packages.config  |    1 +
 | .../DLM/BExIS.Dlm.Orm.NH/BExIS.Dlm.Orm.NH.csproj   |    1 +
 | .../Mappings/Default/Data/DataTuple.hbm.xml        |   25 +-
 | .../Mappings/Default/Data/DataTupleVersion.hbm.xml |    9 +-
 | .../Mappings/Default/Data/DatasetVersion.hbm.xml   |    8 +
 | .../Constraint.PatternConstraint.hbm.xml           |    2 +-
 | .../DataStructure.StructuredDataStructure.hbm.xml  |    4 +-
 | .../Default/DataStructure/Variable.hbm.xml         |    4 +-
 | .../Mappings/Default/Party/Party.hbm.xml           |    2 +-
 | .../Utils/MaterializedViewHelper.cs                |   71 +-
 Components/DLM/BExIS.Dlm.Orm.NH/app.config         |   11 +
 | .../DLM/BExIS.Dlm.Services/Data/DatasetManager.cs  |  370 +-
 | .../DataStructure/MissingValueManager.cs           |   22 +-
 | .../DLM/BExIS.Dlm.Services/Party/PartyManager.cs   |   21 +-
 | .../BExIS.Dlm.Services/Party/PartyTypeManager.cs   |    6 +-
 | .../DLM/BExIS.Dlm.Tests/BExIS.Dlm.Tests.csproj     |   11 +-
 | .../DLM/BExIS.Dlm.Tests/Helpers/DatasetHelper.cs   |  118 +-
 | .../Services/Data/DataTupleTests.cs                |  163 +
 | .../Services/Data/DatasetManagerTests.cs           |   10 +-
 | .../Data/DatasetManager_EditDatasetVersionTests.cs |  189 +
 | .../Data/DatasetManager_GetDataTuplesTest.cs       |  246 +
 ...ger_GetDatasetVersionEffectiveDataTuplesTest.cs |  252 +
 Components/DLM/BExIS.Dlm.Tests/packages.config     |    2 +
 | .../IO/BExIS.IO.DataType.DisplayPattern/app.config |    8 +
 Components/IO/BExIS.IO.Tests/BExIS.IO.Tests.csproj |   16 +-
 Components/IO/BExIS.IO.Tests/IOUtilityTests.cs     |   42 +-
 | .../Transform/Input/AsciiReaderEncodingTest.cs     |  184 +
 | .../Transform/Input/DataReaderTests.cs             |    6 +
 Components/IO/BExIS.IO.Tests/packages.config       |    4 +-
 | .../IO/BExIS.IO.Transform.Input/AsciiReader.cs     |   52 +-
 | .../IO/BExIS.IO.Transform.Input/DataReader.cs      |   22 +-
 | .../IO/BExIS.IO.Transform.Input/ExcelReader.cs     |   45 +-
 Components/IO/BExIS.IO.Transform.Input/app.config  |    8 +
 | .../IO/BExIS.Io.Transform.Output/AsciiHelper.cs    |   16 +
 | .../IO/BExIS.Io.Transform.Output/AsciiWriter.cs    |   43 +-
 | .../IO/BExIS.Io.Transform.Output/DataWriter.cs     |    1 +
 | .../BExIS.Io.Transform.Output/OutputDataManager.cs |  184 +-
 | .../OutputDatasetManager.cs                        |   10 +-
 | .../OutputMetadataManager.cs                       |    4 +-
 Components/IO/BExIS.Io.Transform.Output/app.config |   12 +
 | .../BExIS.Io.Transform.Validation.csproj           |    1 +
 | .../DSValidation/DatastructureMatchCheck.cs        |   11 +-
 | .../DSValidation/DatastructureOrderCheck.cs        |   95 +
 | .../Exceptions/Error.cs                            |    8 +-
 | .../ValueCheck/DataTypeCheck.cs                    |   32 +-
 | .../ValueValidationManager.cs                      |    5 +-
 Components/IO/BExIS.Io/IOUtility.cs                |   47 +
 Components/UI/BExIS.UI/app.config                  |    8 +
 | .../Helpers/ShellSeedDataGenerator.cs              |    7 +
 | .../Utils/BExIS.Utils.Data/Upload/UploadHelper.cs  |   75 +-
 Components/Utils/BExIS.Utils.Data/app.config       |   16 +
 | .../Utils/BExIS.Utils.NH/BExIS.Utils.NH.csproj     |   15 +
 Components/Utils/BExIS.Utils.NH/Querying/Enums.cs  |   16 +-
 | .../Utils/BExIS.Utils.NH/Querying/Expression.cs    |    4 +-
 | .../BExIS.Utils.NH/Querying/QueryExtention.cs      |   34 +
 Components/Utils/BExIS.Utils.NH/app.config         |   11 +
 Components/Utils/BExIS.Utils.NH/packages.config    |    6 +
 Components/Utils/BExIS.Utils/BExIS.Utils.csproj    |    6 +-
 | .../Utils/BExIS.Utils/Helpers/RegExHelper.cs       |    2 +
 | .../WebHelpers/HtmlNavigationExtensions.cs         |   35 +-
 Components/Utils/BExIS.Utils/packages.config       |    2 +
 | .../BExIS.Xml.Helpers.UnitTests.csproj             |   14 +-
 | .../DatasetStoreTests.cs                           |   98 +
 | .../Helpers/DatasetHelper.cs                       |  301 +
 | .../XmlDataTuplesHelperTests.cs                    |   98 +
 | .../XML/BExIS.Xml.Helpers.UnitTests/app.config     |   51 +
 | .../BExIS.Xml.Helpers.UnitTests/packages.config    |    4 +-
 | .../XML/BExIS.Xml.Helpers/BExIS.Xml.Helpers.csproj |    1 +
 Components/XML/BExIS.Xml.Helpers/DatasetStore.cs   |  119 +-
 | .../BExIS.Xml.Helpers/Mapping/XmlSchemaManager.cs  |   64 +-
 | .../XML/BExIS.Xml.Helpers/XmlDataTupleHelper.cs    |   64 +
 | .../XML/BExIS.Xml.Helpers/XmlDatasetHelper.cs      |  157 +-
 | .../XML/BExIS.Xml.Helpers/XmlSchemaUtility.cs      |   20 +-
 Components/XML/BExIS.Xml.Helpers/app.config        |    8 +
 | .../BExIS.Web.Shell.Tests.csproj                   |    7 +-
 Console/BExIS.Web.Shell.Tests/packages.config      |    2 +
 Console/BExIS.Web.Shell/App_Start/BundleConfig.cs  |   10 +
 | .../Areas/BAM/BExIS.Modules.Bam.UI.csproj          |   12 +
 Console/BExIS.Web.Shell/Areas/BAM/Bam.Settings.xml |    2 +-
 | .../Areas/BAM/Controllers/PartyController.cs       |   11 +-
 | .../BAM/Controllers/PartyServiceController.cs      |   80 +
 | .../BExIS.Web.Shell/Areas/BAM/Helpers/Helper.cs    |    2 +-
 | .../PublishProfiles/TestServer_dev.pubxml}         |    9 +-
 | .../PublishProfiles/TestServer_release.pubxml}     |    9 +-
 | .../Areas/BAM/Views/Party/_partiesPartial.cshtml   |    2 +-
 | .../PartyService/_customAttributesPartial.cshtml   |   40 +-
 | .../PartyService/_partyRelationshipsPartial.cshtml |    2 +-
 | .../PartyService/_userRegisterationPartial.cshtml  |    4 +-
 Console/BExIS.Web.Shell/Areas/BAM/web.config       |   23 +-
 | .../Areas/DCM/BExIS.Modules.Dcm.UI.csproj          |   35 +-
 | .../Areas/DCM/Content/bootstrap-theme.css          |  200 +-
 | .../Areas/DCM/Content/bootstrap-theme.css.map      |    2 +-
 | .../Areas/DCM/Content/bootstrap-theme.min.css      |    7 +-
 | .../Areas/DCM/Content/bootstrap-theme.min.css.map  |    1 +
 | .../Areas/DCM/Content/bootstrap.css                | 1084 +--
 | .../Areas/DCM/Content/bootstrap.css.map            |    2 +-
 | .../Areas/DCM/Content/bootstrap.min.css            |    7 +-
 | .../Areas/DCM/Content/bootstrap.min.css.map        |    1 +
 | .../DCM/Controllers/API/AttachmentInController.cs  |    2 +-
 | .../Areas/DCM/Controllers/API/DataInController.cs  |   14 +-
 | .../DCM/Controllers/API/DatasetInController.cs     |    3 +
 | .../DCM/Controllers/API/MetadataInController.cs    |    6 +-
 | .../Areas/DCM/Controllers/AttachmentsController.cs |   29 +-
 | .../DCM/Controllers/CreateDatasetController.cs     |  136 +-
 | .../DCM/Controllers/EasyUploadSummaryController.cs |    3 +-
 | .../DCM/Controllers/EntityReferenceController.cs   |    8 +-
 | .../Areas/DCM/Controllers/FormController.cs        |   43 +-
 | .../Areas/DCM/Controllers/HelpController.cs        |   11 +-
 | .../ManageMetadataStructureController.cs           |   31 +
 | .../Areas/DCM/Controllers/SubmitController.cs      |   96 +-
 | .../DCM/Controllers/SubmitSelectAFileController.cs |    5 +
 | .../Controllers/SubmitSpecifyDatasetController.cs  |   11 +-
 | .../DCM/Controllers/SubmitSummaryController.cs     |  724 +-
 | .../DCM/Controllers/SubmitValidationController.cs  |   56 +-
 | .../Areas/DCM/Controllers/TestController.cs        |   73 +-
 Console/BExIS.Web.Shell/Areas/DCM/Dcm.Settings.xml |    8 +-
 | .../Areas/DCM/Helpers/API/DataApiHelper.cs         |    3 +-
 | .../Areas/DCM/Helpers/DataASyncUploadHelper.cs     |  612 ++
 | .../Areas/DCM/Helpers/FormHelper.cs                |   46 +
 | .../Areas/DCM/Helpers/SettingsHelper.cs            |   40 +
 | .../Areas/DCM/Images/ui-anim_basic_16x16.gif       |  Bin 0 -> 1553 bytes
 | .../Areas/DCM/Models/CreateDataset/SetupModel.cs   |   31 +-
 | .../DCM/Models/Metadata/MetadataAttributeModel.cs  |    6 +
 | .../Areas/DCM/Models/SummaryModel.cs               |   51 +-
 | .../PublishProfiles/TestServer_dev.pubxml}         |    2 +-
 | .../PublishProfiles/TestServer_release.pubxml}     |    8 +-
 Console/BExIS.Web.Shell/Areas/DCM/Scripts/Form.js  |  268 +-
 | .../BExIS.Web.Shell/Areas/DCM/Scripts/bootstrap.js | 2580 ++++++
 | .../Areas/DCM/Scripts/bootstrap.min.js             |    6 +
 | .../Areas/DCM/Scripts/entity.reference.js          |    5 +
 | .../Areas/DCM/Views/CreateDataset/Index.cshtml     |  275 +-
 | .../Areas/DCM/Views/EntityReference/Show.cshtml    |   10 +-
 | .../Areas/DCM/Views/EntityReference/_create.cshtml |    9 +-
 | .../Areas/DCM/Views/Form/MetadataEditor.cshtml     |   23 +-
 | .../DCM/Views/Form/_metadataAttributeView.cshtml   |  654 +-
 | .../Form/_metadataAttributeViewOffline.cshtml      |  117 +-
 ...etadataCompoundAttributeUsageViewOffline.cshtml |    5 +-
 | .../_editMetadataStructureView.cshtml              |   22 +-
 | .../Areas/DCM/Views/Submit/_uploadWizardNav.cshtml |   87 +-
 | .../ChooseUpdateMethod.cshtml                      |    2 +-
 | .../DCM/Views/SubmitSelectAFile/SelectAFile.cshtml |  119 +-
 | .../SubmitSpecifyDataset/SpecifyDataset.cshtml     |   80 +-
 | .../Areas/DCM/Views/SubmitSummary/Summary.cshtml   |  158 +-
 | .../DCM/Views/SubmitValidation/Validation.cshtml   |    2 +-
 Console/BExIS.Web.Shell/Areas/DCM/Web.config       |    9 +-
 Console/BExIS.Web.Shell/Areas/DCM/packages.config  |    4 +-
 | .../Areas/DDM/BExIS.Modules.Ddm.UI.csproj          |   13 +-
 | .../Areas/DDM/Controllers/DashboardController.cs   |  134 +-
 | .../Areas/DDM/Controllers/DataController.cs        |  338 +-
 Console/BExIS.Web.Shell/Areas/DDM/Ddm.Settings.xml |   14 +-
 | .../Areas/DDM/Helpers/SearchUIHelper.cs            |   12 +-
 | .../Areas/DDM/Models/DashboardModel.cs             |   25 +
 | .../Areas/DDM/Models/ShowPrimaryDataModel.cs       |    9 +-
 | .../PublishProfiles/TestServer_dev.pubxml          |   18 +
 | .../PublishProfiles/TestServer_release.pubxml      |   18 +
 | .../Properties/PublishProfiles/WebDeploy.pubxml    |   21 -
 | .../PublishProfiles/WebDeploy.pubxml.user          |   12 -
 | .../Areas/DDM/Views/Dashboard/Index.cshtml         |    9 +-
 | .../DDM/Views/Dashboard/_myDatasetsView.cshtml     |  186 +
 | .../Views/Dashboard/_myDatasetsViewHeader.cshtml   |  149 +
 | .../Areas/DDM/Views/Data/ShowData.cshtml           |  206 +-
 | .../Areas/DDM/Views/Data/ShowPrimaryData.cshtml    |   57 +-
 | .../DDM/Views/Data/_previewDatastructure.cshtml    |   20 +-
 | .../DDM/Views/Data/_structuredDataView.cshtml      |  265 +-
 | .../Views/Shared/_metaDataResultGridView.cshtml    |    2 +-
 Console/BExIS.Web.Shell/Areas/DDM/Web.config       |   26 +-
 | .../Areas/DIM/BExIS.Modules.Dim.UI.csproj          |   13 +-
 | .../DIM/Controllers/API/AttachmentOutController.cs |    4 +-
 | .../Areas/DIM/Controllers/API/DataOutController.cs |    6 +-
 | .../DIM/Controllers/API/DatasetOutController.cs    |   16 +-
 | .../Areas/DIM/Controllers/AdminController.cs       |    2 +-
 | .../Areas/DIM/Controllers/ExportController.cs      |   22 +-
 | .../Areas/DIM/Controllers/MappingController.cs     |    6 +
 | .../Areas/DIM/Controllers/SubmissionController.cs  |   23 +-
 Console/BExIS.Web.Shell/Areas/DIM/Dim.Settings.xml |    2 +-
 | .../Areas/DIM/Helper/DimSeedDataGenerator.cs       |    5 +-
 | .../Areas/DIM/Helper/MappingHelper.cs              |   30 +-
 | .../PublishProfiles/TestServer_dev.pubxml          |   18 +
 | .../PublishProfiles/TestServer_release.pubxml      |   18 +
 | .../PublishProfiles/WebDeploy.pubxml.user          |   12 -
 | .../Views/Submission/_showPublishDataView.cshtml   |    4 +-
 Console/BExIS.Web.Shell/Areas/DIM/Web.config       |   17 +-
 Console/BExIS.Web.Shell/Areas/DIM/packages.config  |    6 +-
 | .../Areas/MMM/Content/bootstrap-theme.css.map      |    1 +
 | .../Areas/MMM/Content/bootstrap-theme.min.css.map  |    1 +
 | .../Areas/MMM/Content/bootstrap.css                | 8704 ++++++++++----------
 | .../Areas/MMM/Content/bootstrap.css.map            |    1 +
 | .../Areas/MMM/Content/bootstrap.min.css            |   24 +-
 | .../Areas/MMM/Content/bootstrap.min.css.map        |    1 +
 | .../Controllers/ShowMultimediaDataController.cs    |  219 +-
 | .../Areas/MMM/IDIV.Modules.Mmm.UI.csproj           |   35 +-
 | .../PublishProfiles/TestServer_dev.pubxml          |   18 +
 | .../PublishProfiles/TestServer_release.pubxml      |   18 +
 | .../PublishProfiles/WebDeploy.pubxml.user          |   12 -
 | .../Areas/MMM/Scripts/modernizr-2.8.3.js           | 1406 ++++
 | .../MMM/Scripts/respond.matchmedia.addListener.js  |  273 +
 | .../Scripts/respond.matchmedia.addListener.min.js  |    5 +
 | .../Areas/MMM/Stylesheets/MultimediaData.css       |    7 +-
 | .../MMM/Views/ShowMultimediaData/_imageView.cshtml |    2 +-
 | .../ShowMultimediaData/_multimediaData.cshtml      |  545 +-
 | .../MMM/Views/ShowMultimediaData/index.cshtml      |    6 +-
 | .../MMM/fonts/glyphicons-halflings-regular.eot     |  Bin 14079 -> 20127 bytes
 | .../MMM/fonts/glyphicons-halflings-regular.svg     |  480 +-
 | .../MMM/fonts/glyphicons-halflings-regular.ttf     |  Bin 29512 -> 45404 bytes
 | .../MMM/fonts/glyphicons-halflings-regular.woff    |  Bin 16448 -> 23424 bytes
 | .../MMM/fonts/glyphicons-halflings-regular.woff2   |  Bin 0 -> 18028 bytes
 Console/BExIS.Web.Shell/Areas/MMM/packages.config  |   10 +-
 Console/BExIS.Web.Shell/Areas/MMM/web.config       |   15 +-
 | .../Areas/RPM/BExIS.Modules.Rpm.UI.csproj          |    8 +-
 | .../RPM/Controllers/DataStructureEditController.cs |   14 +-
 | .../Controllers/DataStructureSearchController.cs   |    6 +-
 | .../Areas/RPM/Controllers/Legacy/UnitController.cs |   62 +-
 | .../Areas/RPM/Models/DataStructureEditModel.cs     |  167 +-
 | .../Areas/RPM/Models/DataStructureSearchModel.cs   |  109 +-
 | .../Models/Legacy/DataStructureDesignerModel.cs    |    2 +-
 | .../Areas/RPM/Models/Legacy/UnitManagerModel.cs    |   47 +-
 | .../PublishProfiles/TestServer_dev.pubxml          |   18 +
 | .../PublishProfiles/TestServer_release.pubxml      |   18 +
 | .../PublishProfiles/WebDeploy.pubxml.user          |   12 -
 Console/BExIS.Web.Shell/Areas/RPM/Rpm.Manifest.xml |    5 +
 Console/BExIS.Web.Shell/Areas/RPM/Rpm.Settings.xml |    3 +-
 | .../Areas/RPM/Stylesheets/DataStructure.css        |   24 +-
 | .../Areas/RPM/Stylesheets/DataStructureEdit.css    |   79 +-
 | .../Views/DataAttribute/AttributeManager.cshtml    |    2 +-
 | .../DataStructureEdit/_attributeElement.cshtml     |   19 +-
 | .../DataStructureEdit/_attributeFilter.cshtml      |    3 +
 | .../_attributeSearchResult.cshtml                  |    9 +-
 | .../Views/DataStructureEdit/_dataStructure.cshtml  |   40 +-
 | .../DataStructureEdit/_variableElement.cshtml      |   48 +-
 | .../RPM/Views/DataStructureSearch/Index.cshtml     |   22 +-
 | .../Areas/RPM/Views/Unit/UnitManager.cshtml        |   29 +-
 | .../Areas/RPM/Views/Unit/_editUnit.cshtml          |   63 +-
 Console/BExIS.Web.Shell/Areas/RPM/web.config       |    8 +
 | .../Areas/SAM/BExIS.Modules.Sam.UI.csproj          |   18 +-
 | .../Areas/SAM/Controllers/DatasetsController.cs    |    4 +-
 | .../SAM/Controllers/EntityPermissionsController.cs |   14 +-
 | .../Controllers/FeaturePermissionsController.cs    |    8 -
 | .../Areas/SAM/Controllers/GroupsController.cs      |   11 +-
 | .../SAM/Controllers/RequestsAdminController.cs     |  237 +
 | .../Areas/SAM/Controllers/RequestsController.cs    |  205 +-
 | .../SAM/Controllers/UserPermissionsController.cs   |    8 +-
 | .../Areas/SAM/Controllers/UsersController.cs       |   25 +-
 | .../Areas/SAM/Helpers/SAMSeedDataGenerator.cs      |    3 +
 | .../Areas/SAM/Models/DatasetModels.cs              |    2 +-
 | .../Areas/SAM/Models/DecisionModels.cs             |    4 +
 | .../Areas/SAM/Models/EntityModels.cs               |    2 +-
 | .../Areas/SAM/Models/EntityPermissionModels.cs     |    4 +-
 | .../Areas/SAM/Models/FeaturePermissionModels.cs    |    5 +-
 | .../Areas/SAM/Models/RequestModels.cs              |    2 +
 | .../PublishProfiles/TestServer_dev.pubxml          |   18 +
 | .../PublishProfiles/TestServer_release.pubxml      |   18 +
 | .../Properties/PublishProfiles/WebDeploy.pubxml    |   21 -
 | .../PublishProfiles/WebDeploy.pubxml.user          | 1535 ----
 Console/BExIS.Web.Shell/Areas/SAM/Sam.Manifest.xml |   11 +-
 Console/BExIS.Web.Shell/Areas/SAM/Sam.Settings.xml |    2 +-
 | .../SAM/Views/EntityPermissions/_Instances.cshtml  |    2 +
 | .../SAM/Views/EntityPermissions/_Subjects.cshtml   |    4 +-
 | .../SAM/Views/FeaturePermissions/_Subjects.cshtml  |    4 +-
 | .../Areas/SAM/Views/Requests/Index.cshtml          |   13 +
 | .../Areas/SAM/Views/Requests/_Decisions.cshtml     |  171 +-
 | .../Areas/SAM/Views/Requests/_Requests.cshtml      |  152 +-
 | .../Areas/SAM/Views/RequestsAdmin/Index.cshtml     |   40 +
 | .../SAM/Views/RequestsAdmin/_DecisionsAdmin.cshtml |  188 +
 | .../Areas/SAM/Views/RequestsAdmin/_Entities.cshtml |   70 +
 | .../SAM/Views/UserPermissions/_Subjects.cshtml     |    4 +-
 Console/BExIS.Web.Shell/Areas/SAM/Web.config       |   12 +-
 | .../Areas/VIM/BExIS.Modules.Vim.UI.csproj          |   13 +-
 | .../PublishProfiles/TestServer_dev.pubxml          |   18 +
 | .../PublishProfiles/TestServer_release.pubxml      |   18 +
 | .../PublishProfiles/WebDeploy.pubxml.user          | 1817 ----
 Console/BExIS.Web.Shell/Areas/VIM/Vim.Settings.xml |    2 +-
 Console/BExIS.Web.Shell/Areas/VIM/Web.config       |   17 +-
 Console/BExIS.Web.Shell/BExIS.Web.Shell.csproj     |    9 +-
 | .../Content/Images/ui-bg_glass_55_fbf9ee_1x400.png |  Bin 0 -> 120 bytes
 | .../Content/Images/ui-bg_glass_75_dadada_1x400.png |  Bin 0 -> 111 bytes
 | .../Content/Images/ui-bg_glass_75_e6e6e6_1x400.png |  Bin 0 -> 110 bytes
 | .../Content/Images/ui-icons_222222_256x240.png     |  Bin 0 -> 4369 bytes
 | .../Controllers/AccountController.cs               |    8 +-
 | .../BExIS.Web.Shell/Controllers/HomeController.cs  |   38 +-
 | .../Settings/PostgreSQL82Dialect.hibernate.cfg.xml |    2 +-
 Console/BExIS.Web.Shell/General.Settings.xml       |    3 +
 Console/BExIS.Web.Shell/Models/AccountModels.cs    |    2 +
 Console/BExIS.Web.Shell/Models/VersionModels.cs    |   14 +
 | .../PublishProfiles/TestServer_dev.pubxml          |   40 +
 | .../PublishProfiles/TestServer_release.pubxml      |   40 +
 | .../Scripts/2013.2.611/telerik.grid.min.js         |    4 +-
 | .../Themes/Default/Layouts/_Layout.cshtml          |   37 +-
 | .../Themes/Default/Styles/wizard.css               |   10 +-
 Console/BExIS.Web.Shell/Views/Home/Version.cshtml  |   12 +
 Console/BExIS.Web.Shell/web.config                 |   22 +-
 Console/Workspace                                  |    2 +-
 Modules/DCM/BExIS.Dcm.UploadWizard/TaskManager.cs  |    2 +-
 Modules/DCM/BExIS.Dcm.UploadWizard/app.config      |    8 +
 | .../BExIS.Ddm.Providers.LuceneProvider.csproj      |    1 +
 | .../Indexer/BexisIndexer.cs                        |   80 +-
 | .../Searcher/BexisIndexSearcher.cs                 |   18 +
 | .../BExIS.Ddm.Providers.LuceneProvider/app.config  |    8 +
 Modules/DIM/BExIS.Dim.Entities/app.config          |    8 +
 | .../Export/GenericDataRepoConverter.cs             |   13 +-
 | .../Export/PangaeaDataRepoConverter.cs             |    1 -
 | .../DIM/BExIS.Dim.Helper/Mapping/MappingUtils.cs   |  387 +-
 Modules/DIM/BExIS.Dim.Helper/app.config            |    8 +
 | .../Mappings/Default/Mapping/Mapping.hbm.xml       |    6 +-
 Modules/DIM/BExIS.Dim.Services/MappingManager.cs   |   24 +-
 Modules/DIM/BExIS.Dim.Services/app.config          |    8 +
 README.md                                          |    2 +-
 Release Notes/Release_Notes.md                     |  181 +-
 | .../Update_Script_2123to213.txt                    |    4 +-
 | .../Update_Script_213to2131.txt                    |    9 +
 database update scripts/Update_Script_213to214.sql |  396 +
