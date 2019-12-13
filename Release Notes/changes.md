# Version Changes 2.12.3 -> 2.13.0

## Shell

- Manifest File changed
  
```xml
<!--FAQ-->
<Export tag="lunchbar" id="helpDdm" order="1"
        title="FAQ" description="Link to the faq page" icon=""
        controller="help" action="faq"
        extends="./lunchbarRoot/help" />
```

- general settings - add landingPageForUsers

```xml

<settings>
  <!--
    The default landing page for the search in the form of <moduleId>, <controller>, <action>. The application's home page will redirect here.
    The module must be avilable and active, otherwise another redirect happens to the home page, which causes a loop.
    The designated action must be set to public in the feature permission mgmt, otherwise the request will be redirected to the login page.
    The 'type' attribute must be compatible with the System.TypeCode case-sensitive.
  -->
  <entry key="landingPage" value="ddm, PublicSearch, index" type="String" />
  <!-- user is not logging in -> app goes to-->
  <entry key="landingPageForUsers" value="ddm, Home, index" type="String" />
  <!-- landing page for users , when they logged in successfuly -->
  <entry key="faq" value="https://github.com/BEXIS2/Core/wiki/FAQ" type="string" />
</settings>

```

- change home Controller for landingpage routing

```cs

@@ -15,9 +17,34 @@ public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Home", this.Session.GetTenant());

            // here there are 2 cases to consider.
            // 1.no user->landingpage
            // 2.user logged into landingpage for users
            Tuple<string, string, string> landingPage = null;
            //check if user exist
            if (!string.IsNullOrEmpty(HttpContext.User?.Identity?.Name)) //user
            {
                // User exist : load ladingpage for users
                GeneralSettings generalSettings = IoCFactory.Container.Resolve<GeneralSettings>();
                var landingPageForUsers = generalSettings.GetEntryValue("landingPageForUsers").ToString();

                if (landingPageForUsers.Split(',').Length == 3)//check wheter 3 values exist for teh action
                {
                    landingPage = new Tuple<string, string, string>(
                        landingPageForUsers.Split(',')[0].Trim(), //module id
                        landingPageForUsers.Split(',')[1].Trim(), //controller
                        landingPageForUsers.Split(',')[2].Trim());//action
                }
            }
            else
            {
                landingPage = this.Session.GetTenant().LandingPageTuple;
            }

            //if the landingPage not null and the action is accessable
            if (landingPage == null || !this.IsAccessible(landingPage.Item1, landingPage.Item2, landingPage.Item3))
                return View();

            var result = this.Render(landingPage.Item1, landingPage.Item2, landingPage.Item3);
            return Content(result.ToHtmlString(), "text/html");
        }

```


## Core / Components 

- EntityStore  
  -  GetVersionById
  -  -+++ a lot of new functions
- EntityStoreItem 
  - public int Version { get; set; }

````c#
public interface IEntityStore
{
    List<EntityStoreItem> GetEntities();

    string GetTitleById(long id);

    bool HasVersions();

    int| CountVersions(long id);

    List<EntityStoreItem> GetVersionsById(long id);
}

public class EntityStoreItem
{
    public long Id { get; set; }
    public int Version { get; set; }
    public string Title { get; set; }
    public string| CommitComment { get; set; }
}
````
### AAA

- EntityPermissionMananger

  - add 2 functions

```cs

        public string[] GetRights(short rights)
        {
            return Enum.GetNames(typeof(RightType)).Select(n => n)
                .Where(n => (rights & (int) Enum.Parse(typeof(RightType), n)) > 0).ToArray();
        }

        public short GetRights(string[] rights)
        {
            throw new NotImplementedException();
        }
```

- FeaturesPermissionManager

  - HasAccess

```cs 

    var SubjectRepository = uow.GetReadOnlyRepository<Subject>();

    var operation = operationRepository.Query(x => x.Module.ToUpperInvariant() == module.ToUpperInvariant() && x.Controller.ToUpperInvariant() == controller.ToUpperInvariant() && x.Action.ToUpperInvariant() == action.ToUpperInvariant()).FirstOrDefault();
    if (operation == null) return false;

    var feature = operation?.Feature;
    var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
 
    //both exits
    if (feature != null)
        return HasAccess(subject?.Id, feature.Id);

    // operation exist but feature not exist -  operatioen is public
    if (feature == null && subject != null)
        return true;

    // operation exist but the features is null -> operation is public
    // subject = null if no user is logged in
    if (feature == null && subject == null)
        return true;

    return false;

```

- GroupManager
- SubjectManager
- UserManager
- EMailService

### App

- Application.cs
- BExISApiAuthorizeAttribute.cs
- BExISEntityAuthorizeAttribute.cs 

### DLM

- MetadataAttributeUsage.hbm.xml
- MetadataNestedAttributeUsage.hbm.xml 
- MetadataPackage.hbm.xml
- MetadataPackageUsage.hbm.xml
- PartyCustomAttribute.hbm.xml
- PartyCustomAttributeValue.hbm.xml 

#### Managers

- MissingValueManager
- PartyManager
- PartyTypeManager

### IO
- AsciiReader, DataReader, EasyUploadExcelReader, ExcelReader
- DataWriter, AscciWriter & ExcelWriter -> add units option
- OutputMetadataManager.cs
- MissingValueCheck.cs

### UI

- JsonTableGenerator.cs
- TelerikGridHelper.cs
  
### Utils

- ShellSeedDataGenerator.cs
- RegExHelper.cs 
- SearchAttribute.cs
- HtmlNavigationExtensions.cs

### Xml

- XsdSchemaReaderTests.cs 
- DatasetStore.cs
- XmlSchemaManager.cs
- XmlDatasetHelper.cs 
- XmlMetadataWriter.cs
- XmlUtility.cs 
- XsdSchemaReader.cs


## ADDS
- Datatables

## DB
Add new Table EntityReferences
Add missing values Table
changes in MetadataAttributeUsage
changes in MetadataNestedAttributeUsage
changes in MetadataPackageUsage
changes in MetadataPackage


## Seedata
changes features rights for AttachmentsController - must be public (dcm)
changes features rights for EntityReferenceController - must be public (dcm)
changes features rights for SubmissionController - must be public (dim)

## WORKSPACE

- apiConfig.xml (oai-pmh)
- entityReferenceConfig.xml
- MMM Module
- geänderte mapping files 
- BAM settings go to bam.settings.xml

  
````xml

  <config>
    <referenceTypes>
        <referenceType>based on </referenceType>
        <referenceType>collection</referenceType>
        <referenceType>parent of</referenceType>
        <referenceType>child of</referenceType>
        <referenceType>link</referenceType>
    </referenceTypes>
</config>

````

## UI

- CSS -> erstellen von variables für Colors



- Site.css remove links urls by print view
- ```css
/* Don't print link hrefs */
@media print {
    a [href]:after {
        content: none
    }
}

```



## web.config

```xml
<remove name="WebDAVModule"/>

<add key="SessionWarningDelaySecond" value="30" />

<add key="useMultimediaModule" value="true" />

```


## DCM
- FormController -> version
- Add new entity called EntityReference + EntityReferenceManager
- add EntityReferenceController
- add EntityReferenceController to seed data

## DDM
- change manifest file Search to secured search
- 
```xml
 <Export tag="menubar" id="menuSearch" order="2"
            title="Search" description="Search datasets" icon=""
            controller="Home" action="index"
            extends="./menubarRoot" />
```




# CHANGED Files 

$ git diff --stat origin/dev..origin/releases/2.13 --no-merges

 | File| Changes |
 |---|---|
 | .gitignore                                         |    1 + |
 | .gitmodules                                        |    0 |
 | BExIS++.sln                                        |  359 +- |
 | .../BExIS.Security.Entities.csproj                 |    9 + |
 | .../BExIS.Security.Entities/Requests/Decision.cs   |    1 + |
 | .../BExIS.Security.Orm.NH.csproj                   |    9 + |
 | .../Authorization/EntityPermissionManagerTests.cs  |   57 + |
 | .../BExIS.Security.Services.Tests.csproj           |  202 + |
 | .../Properties/AssemblyInfo.cs                     |   20 + |
 | .../Subjects/GroupManagerTests.cs                  |   42 + |
 | .../AAA/BExIS.Security.Services.Tests/app.config   |  131 + |
 | .../BExIS.Security.Services.Tests/packages.config  |   10 + |
 | .../Authorization/EntityPermissionManager.cs       |   11 + |
 | .../Authorization/FeaturePermissionManager.cs      |   14 +- |
 | .../BExIS.Security.Services.csproj                 |    9 + |
 | .../Subjects/GroupManager.cs                       |   25 +- |
 | .../Subjects/SubjectManager.cs                     |    2 + |
 | .../Subjects/UserManager.cs                        |   30 +- |
 | .../Utilities/EmailService.cs                      |    4 +- |
 | .../BExIS.Security.Tests.csproj                    |   85 - |
 | .../Properties/AssemblyInfo.cs                     |   36 - |
 | .../AAA/BExIS.Security.Tests/Temp/UnitTest1.cs     |   14 - |
 | Components/App/BExIS.App.Bootstrap/Application.cs  |    4 +- |
 | .../Attributes/BExISApiAuthorizeAttribute.cs       |    3 + |
 | .../Attributes/BExISEntityAuthorizeAttribute.cs    |   54 + |
 | .../BExIS.App.Bootstrap/BExIS.App.Bootstrap.csproj |   10 + |
 | .../App/BExIS.App.Testing/BExIS.App.Testing.csproj |    9 + |
 | .../BExIS.Dlm.Entities/BExIS.Dlm.Entities.csproj   |    9 + |
 | .../DLM/BExIS.Dlm.Orm.NH/BExIS.Dlm.Orm.NH.csproj   |    9 + |
 | .../MetadataAttributeUsage.hbm.xml                 |   18 +- |
 | .../MetadataNestedAttributeUsage.hbm.xml           |   18 +- |
 | .../MetadataStructure/MetadataPackage.hbm.xml      |   25 +- |
 | .../MetadataStructure/MetadataPackageUsage.hbm.xml |   18 +- |
 | .../Default/Party/PartyCustomAttribute.hbm.xml     |   55 +- |
 | .../Party/PartyCustomAttributeValue.hbm.xml        |   13 +- |
 | .../BExIS.Dlm.Services/BExIS.Dlm.Services.csproj   |    9 + |
 | .../DataStructure/MissingValueManager.cs           |   26 + |
 | .../DLM/BExIS.Dlm.Services/Party/PartyManager.cs   |  169 +- |
 | .../BExIS.Dlm.Services/Party/PartyTypeManager.cs   |   40 +- |
 | .../DLM/BExIS.Dlm.Tests/BExIS.Dlm.Tests.csproj     |    9 + |
 | .../BExIS.Ext.Entities/BExIS.Ext.Entities.csproj   |    9 + |
 | .../EXT/BExIS.Ext.Model/BExIS.Ext.Model.csproj     |    9 + |
 | .../EXT/BExIS.Ext.Orm.NH/BExIS.Ext.Orm.NH.csproj   |    9 + |
 | .../BExIS.Ext.Services/BExIS.Ext.Services.csproj   |    9 + |
 | .../BExIS.IO.DataType.DisplayPattern.csproj        |    9 + |
| Components/IO/BExIS.IO.Tests/BExIS.IO.Tests.csproj |   10 + |
 | .../Transform/Input/AsciiReaderTest.cs             |  129 + |
 | .../Transform/Input/DataReaderTests.cs             |   44 +- |
 | .../IO/BExIS.IO.Transform.Input/AsciiReader.cs     |   52 +- |
 | .../BExIS.IO.Transform.Input.csproj                |    9 + |
 | .../IO/BExIS.IO.Transform.Input/DataReader.cs      |    2 +- |
 | .../EasyUploadExcelReader.cs                       |    6 +- |
 | .../IO/BExIS.IO.Transform.Input/ExcelReader.cs     |    2 +- |
 | .../BExIS.Io.Transform.Output.csproj               |    9 + |
 | .../OutputMetadataManager.cs                       |    7 + |
 | .../BExIS.Io.Transform.Validation.csproj           |    9 + |
 | .../ValueCheck/MissingValueCheck.cs                |   17 +- |
 | Components/IO/BExIS.Io/BExIS.Io.csproj             |    9 + |
 | Components/UI/BExIS.UI/BExIS.UI.csproj             |    9 + |
 | .../UI/BExIS.UI/Helpers/JsonTableGenerator.cs      |   17 +- |
 | .../UI/BExIS.UI/Helpers/TelerikGridHelper.cs       |   25 +- |
 | .../Utils/BExIS.Utils.Data/BExIS.Utils.Data.csproj |    9 + |
 | .../Helpers/ShellSeedDataGenerator.cs              |    1 + |
 | .../Utils/BExIS.Utils.NH/BExIS.Utils.NH.csproj     |    9 + |
 | Components/Utils/BExIS.Utils/BExIS.Utils.csproj    |    9 + |
 | .../Utils/BExIS.Utils/Helpers/RegExHelper.cs       |    5 +- |
 | .../Utils/BExIS.Utils/Models/SearchAttribute.cs    |   12 +- |
 | .../WebHelpers/HtmlNavigationExtensions.cs         |   16 +- |
 | .../BExIS.Xml.Helpers.UnitTests.csproj             |   96 + |
 | .../Properties/AssemblyInfo.cs                     |   20 + |
 | .../XmlUtility_XDocumentTests.cs                   |  413 + |
 | .../XmlUtility_XmlDocumentTests.cs                 |  466 + |
 | .../XsdSchemaReaderTests.cs                        |   41 + |
 | .../BExIS.Xml.Helpers.UnitTests/packages.config    |   11 + |
 | .../XML/BExIS.Xml.Helpers/BExIS.Xml.Helpers.csproj |    9 + |
 | Components/XML/BExIS.Xml.Helpers/DatasetStore.cs   |    4 +- |
 | .../BExIS.Xml.Helpers/Mapping/XmlSchemaManager.cs  |  223 +- |
 | .../XML/BExIS.Xml.Helpers/XmlDatasetHelper.cs      |    2 +- |
 | .../XML/BExIS.Xml.Helpers/XmlMetadataWriter.cs     |    2 +- |
 | Components/XML/BExIS.Xml.Helpers/XmlUtility.cs     |  140 +- |
 | .../XML/BExIS.Xml.Helpers/XsdSchemaReader.cs       |   69 +- |
 | .../XML/BExIS.Xml.Models/BExIS.Xml.Models.csproj   |    9 + |
 | .../BExIS.Web.Shell.Tests.csproj                   |    9 + |
| Console/BExIS.Web.Shell.Tests/app.config           |   11 +- |
 | .../BExIS.Web.Shell/App_Data/api_documentation.xml |   32 + |
 | .../Areas/BAM/BExIS.Modules.Bam.UI.csproj          |   28 +- |
| Console/BExIS.Web.Shell/Areas/BAM/Bam.Settings.xml |    7 +- |
 | .../BAM/Content/Images/help/create_party_1.jpg     |  Bin 74958 -> 0 bytes |
 | .../BAM/Content/Images/help/create_party_2.jpg     |  Bin 98082 -> 0 bytes |
 | .../BAM/Content/Images/help/manage_parties.jpg     |  Bin 95217 -> 0 bytes |
 | .../BAM/Content/Images/help/new_party_relation.jpg |  Bin 18712 -> 0 bytes |
 | .../Content/Images/help/new_party_relation2.jpg    |  Bin 57375 -> 0 bytes |
 | .../Areas/BAM/Content/Images/help/registration.jpg |  Bin 75482 -> 0 bytes |
 | .../Areas/BAM/Content/Images/help/schema.jpg       |  Bin 61378 -> 0 bytes |
 | .../Areas/BAM/Controllers/HelpController.cs        |   14 +- |
 | .../Areas/BAM/Helper/BAMSeedDataGenerator.cs       |  332 - |
| Console/BExIS.Web.Shell/Areas/BAM/Helper/Helper.cs |  325 - |
 | .../BExIS.Web.Shell/Areas/BAM/Helper/Settings.cs   |  130 - |
 | .../Areas/BAM/Helpers/BAMSeedDataGenerator.cs      |  386 +- |
 | .../BExIS.Web.Shell/Areas/BAM/Helpers/Helper.cs    |  260 +- |
 | .../BExIS.Web.Shell/Areas/BAM/Helpers/Settings.cs  |   21 +- |
 | .../PublishProfiles/FolderProfile.pubxml           |   18 + |
 | .../Areas/BAM/Views/Party/Index.cshtml             |    2 +- |
 | .../Views/Party/_partiesDynamicGridPartial.cshtml  |    2 +- |
 | .../Areas/BAM/Views/PartyService/Edit.cshtml       |   18 + |
 | .../PartyService/_customAttributesPartial.cshtml   |   10 +- |
 | .../PartyService/_userRegisterationPartial.cshtml  |   37 +- |
 | .../Areas/DCM/BExIS.Modules.Dcm.UI.csproj          |   44 +- |
 | .../DCM/Content/Images/help/ChooseUpdateMethod.png |  Bin 17649 -> 0 bytes |
 | .../DCM/Content/Images/help/Create_Dataset.jpg     |  Bin 72360 -> 0 bytes |
 | .../DCM/Content/Images/help/Create_Dataset.png     |  Bin 23923 -> 0 bytes |
 | .../Content/Images/help/GetFileInformations.png    |  Bin 17346 -> 0 bytes |
 | .../Content/Images/help/Help_easy_upload_end.png   |  Bin 38406 -> 0 bytes |
 | .../Images/help/Help_easy_upload_metadata.png      |  Bin 34124 -> 0 bytes |
 | .../Images/help/Help_easy_upload_select_areas.png  |  Bin 46630 -> 0 bytes |
 | .../Images/help/Help_easy_upload_select_file.png   |  Bin 45503 -> 0 bytes |
 | .../help/Help_easy_upload_sheet_structure.png      |  Bin 33219 -> 0 bytes |
 | .../Images/help/Help_easy_upload_summary.png       |  Bin 35271 -> 0 bytes |
 | .../Images/help/Help_easy_upload_verification.png  |  Bin 31092 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/collapse.png     |  Bin 337 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/columnwise.jpg   |  Bin 66851 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/expand.png       |  Bin 348 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/messages.png     |  Bin 248101 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/metadata1.png    |  Bin 4828 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/metadata2.png    |  Bin 22990 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/metadata3.png    |  Bin 4181 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/metadata4.png    |  Bin 17282 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/mines.png        |  Bin 402 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/plus.png         |  Bin 550 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/primary_key.png  |  Bin 42191 -> 0 bytes |
 | .../DCM/Content/Images/help/push_big_file.png      |  Bin 34295 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/read_xsd.png     |  Bin 38175 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/red_star.png     |  Bin 334 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/rowwise.jpg      |  Bin 59315 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/select_file.png  |  Bin 42446 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/select_xsd.png   |  Bin 34532 -> 0 bytes |
 | .../DCM/Content/Images/help/set_xsd_parameters.png |  Bin 35116 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/summary_xsd.png  |  Bin 27699 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/up_down.png      |  Bin 619 -> 0 bytes |
 | .../Areas/DCM/Content/Images/help/upload_data.png  |  Bin 25014 -> 0 bytes |
 | .../DCM/Content/Images/help/upload_tabular.jpg     |  Bin 73236 -> 0 bytes |
 | .../Areas/DCM/Content/bexis-metadata.css           |   18 +- |
 | .../DCM/Controllers/API/AttachmentInController.cs  |   16 +- |
 | .../Areas/DCM/Controllers/API/DataInController.cs  |   22 +- |
 | .../DCM/Controllers/API/DatasetInController.cs     |   15 +- |
 | .../DCM/Controllers/API/MetadataInController.cs    |    2 +- |
 | .../Areas/DCM/Controllers/AttachmentsController.cs |   36 +- |
 | .../DCM/Controllers/CreateDatasetController.cs     |   52 +- |
 | .../Controllers/EasyUploadSelectAreasController.cs |   20 +- |
 | .../DCM/Controllers/EasyUploadSummaryController.cs |   54 +- |
 | .../DCM/Controllers/EntityReferenceController.cs   |  175 +- |
 | .../Areas/DCM/Controllers/FormController.cs        |   88 +- |
 | .../Areas/DCM/Controllers/HelpController.cs        |   19 +- |
 | .../SubmitGetFileInformationController.cs          |   52 +- |
 | .../DCM/Controllers/SubmitSummaryController.cs     |   40 +- |
| Console/BExIS.Web.Shell/Areas/DCM/Dcm.Settings.xml |    2 +- |
 | .../Areas/DCM/Helpers/API/DataApiHelper.cs         |   11 +- |
 | .../Areas/DCM/Helpers/DCMSeedDataGenerator.cs      |    8 +- |
 | .../Areas/DCM/Helpers/EntityReferenceHelper.cs     |   60 +- |
 | .../Areas/DCM/Helpers/UploadUIHelper.cs            |   15 +- |
 | .../Models/CreateDataset/MetadataEditorModel.cs    |    4 +- |
 | .../EntityReference/CreateSimpleReferenceModel.cs  |   35 - |
 | .../DCM/Models/EntityReference/ReferenceModels.cs  |    5 +- |
 | .../BExIS.Web.Shell/Areas/DCM/Models/TestModel.cs  |    2 +- |
 | .../PublishProfiles/FolderProfile.pubxml           |   18 + |
 | .../DCM/Scripts/2013.2.611/jquery-1.7.1.min.js     |    4 - |
 | .../DCM/Scripts/2013.2.611/jquery.validate.min.js  |   50 - |
 | .../DCM/Scripts/2013.2.611/telerik.all.min.js      |    1 - |
 | .../Scripts/2013.2.611/telerik.autocomplete.min.js |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.calendar.min.js |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.chart.min.js    |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.combobox.min.js |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.common.min.js   |    1 - |
 | .../Scripts/2013.2.611/telerik.datepicker.min.js   |    1 - |
 | .../2013.2.611/telerik.datetimepicker.min.js       |    1 - |
 | .../Scripts/2013.2.611/telerik.draganddrop.min.js  |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.editor.min.js   |    1 - |
 | .../Scripts/2013.2.611/telerik.grid.editing.min.js |    1 - |
 | .../2013.2.611/telerik.grid.filtering.min.js       |    1 - |
 | .../2013.2.611/telerik.grid.grouping.min.js        |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.grid.min.js     |    1 - |
 | .../2013.2.611/telerik.grid.reordering.min.js      |    1 - |
 | .../2013.2.611/telerik.grid.resizing.min.js        |    1 - |
 | .../Scripts/2013.2.611/telerik.imagebrowser.min.js |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.list.min.js     |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.menu.min.js     |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.panelbar.min.js |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.slider.min.js   |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.splitter.min.js |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.tabstrip.min.js |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.textbox.min.js  |    1 - |
 | .../Scripts/2013.2.611/telerik.timepicker.min.js   |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.treeview.min.js |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.upload.min.js   |    1 - |
 | .../DCM/Scripts/2013.2.611/telerik.window.min.js   |    1 - |
| Console/BExIS.Web.Shell/Areas/DCM/Scripts/Form.js  |  161 +- |
 | .../BExIS.Web.Shell/Areas/DCM/Scripts/bootstrap.js | 1999 ---- |
 | .../Areas/DCM/Scripts/bootstrap.min.js             |    6 - |
| Console/BExIS.Web.Shell/Areas/DCM/Scripts/ddm.js   |    7 - |
 | .../Areas/DCM/Scripts/entity.reference.js          |   30 +- |
 | .../Areas/DCM/Scripts/jquery-2.1.4.intellisense.js | 2670 ------ |
 | .../Areas/DCM/Scripts/jquery-2.1.4.js              | 9210 ------------------ |
 | .../Areas/DCM/Scripts/jquery-2.1.4.min.js          |    5 - |
 | .../Areas/DCM/Scripts/jquery-2.1.4.min.map         |    1 - |
 | .../Areas/DCM/Scripts/jquery-migrate-1.2.1.js      |  521 -- |
 | .../Areas/DCM/Scripts/jquery-migrate-1.2.1.min.js  |    2 - |
 | .../Areas/DCM/Scripts/jquery.validate-vsdoc.js     | 1288 --- |
 | .../Areas/DCM/Scripts/jquery.validate.js           | 1231 --- |
 | .../Areas/DCM/Scripts/jquery.validate.min.js       |    2 - |
 | .../DCM/Scripts/jquery.validate.unobtrusive.js     |  429 - |
 | .../DCM/Scripts/jquery.validate.unobtrusive.min.js |   19 - |
 | .../Areas/DCM/Scripts/modernizr-2.6.2.js           | 1393 --- |
 | .../BExIS.Web.Shell/Areas/DCM/Scripts/respond.js   |  326 - |
 | .../Areas/DCM/Scripts/respond.min.js               |    6 - |
 | .../Views/Attachments/_datasetAttachements.cshtml  |    6 +- |
 | .../Views/EasyUploadSelectAFile/SelectAFile.cshtml |    2 +- |
 | .../Areas/DCM/Views/EntityReference/Show.cshtml    |  219 +- |
 | .../Areas/DCM/Views/EntityReference/_create.cshtml |   18 +- |
 | .../Areas/DCM/Views/Form/MetadataEditor.cshtml     |   82 +- |
 | .../DCM/Views/Form/_metadataAttributeView.cshtml   |  125 +- |
 | .../_metadataCompoundAttributeUsageView.cshtml     |   12 +- |
 | .../DCM/Views/ManageMetadataStructure/Index.cshtml |    4 +- |
 | .../DCM/Views/SubmitSelectAFile/SelectAFile.cshtml |    2 +- |
| Console/BExIS.Web.Shell/Areas/DCM/Views/Web.config |    6 +- |
 | .../Areas/DDM/BExIS.Modules.Ddm.UI.csproj          |   34 +- |
 | .../Areas/DDM/Content/Images/Help/enableMacro.png  |  Bin 132875 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image001.png     |  Bin 78923 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image002.jpg     |  Bin 24530 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image003.jpg     |  Bin 24831 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image004.png     |  Bin 48625 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image005.jpg     |  Bin 28581 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image006.jpg     |  Bin 20269 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image007.jpg     |  Bin 18204 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image008.jpg     |  Bin 24363 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image009.jpg     |  Bin 24163 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image010.png     |  Bin 469 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image011.jpg     |  Bin 769 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image012.jpg     |  Bin 27557 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image013.jpg     |  Bin 78732 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image014.jpg     |  Bin 10610 -> 0 bytes |
 | .../Areas/DDM/Content/Images/Help/image015.jpg     |  Bin 10922 -> 0 bytes |
 | .../Areas/DDM/Controllers/DashboardController.cs   |   37 +- |
 | .../Areas/DDM/Controllers/DataController.cs        |  183 +- |
 | .../Areas/DDM/Controllers/HelpController.cs        |   26 +- |
 | .../Areas/DDM/Controllers/HomeController.cs        |   62 +- |
 | .../DDM/Controllers/PublicSearchController.cs      |   18 +- |
| Console/BExIS.Web.Shell/Areas/DDM/Ddm.Manifest.xml |   13 +- |
| Console/BExIS.Web.Shell/Areas/DDM/Ddm.Settings.xml |    2 +- |
 | .../Areas/DDM/Models/SearchAttributeViewModel.cs   |    3 +- |
 | .../PublishProfiles/FolderProfile.pubxml           |   18 + |
 | .../DDM/Views/Admin/_editSearchAttribute.cshtml    |    4 +- |
 | .../Areas/DDM/Views/Admin/_metadataNode.cshtml     |    4 +- |
 | .../Areas/DDM/Views/Dashboard/Index.cshtml         |    3 + |
 | .../DDM/Views/Dashboard/_myDatasetGridView.cshtml  |   15 +- |
 | .../Areas/DDM/Views/Data/ShowData.cshtml           |   58 +- |
 | .../Areas/DDM/Views/Help/Index.cshtml              |  169 +- |
 | .../DDM/Views/Shared/_checkboxButtonsView.cshtml   |   78 + |
 | .../Areas/DDM/Views/Shared/_dropBoxView.cshtml     |   38 +- |
 | .../Views/Shared/_metaDataResultGridView.cshtml    |    4 +- |
 | .../DDM/Views/Shared/_radioButtonsView.cshtml      |   41 +- |
 | .../Areas/DDM/Views/Shared/_searchHeader.cshtml    |    3 +- |
 | .../DDM/Views/Shared/_searchProperties.cshtml      |    4 +- |
 | .../Areas/DIM/BExIS.Modules.Dim.UI.csproj          |   29 +- |
 | .../Areas/DIM/Content/Images/help/collections.png  |  Bin 44795 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/help_img1.png    |  Bin 22652 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/help_img2.png    |  Bin 28286 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/help_img3.png    |  Bin 16848 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/help_img4.png    |  Bin 40860 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/key_overview.PNG |  Bin 107171 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/many_to_one.png  |  Bin 54242 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/mapping.png      |  Bin 123242 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/mapping_tool.png |  Bin 32274 -> 0 bytes |
 | .../DIM/Content/Images/help/mapping_tool2.png      |  Bin 40918 -> 0 bytes |
 | .../DIM/Content/Images/help/mapping_tool3.png      |  Bin 36900 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/one_to_many.png  |  Bin 54834 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/one_to_one.png   |  Bin 27772 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/pangaea1.png     |  Bin 104710 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/pangaea2.png     |  Bin 63428 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/publish.png      |  Bin 41472 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/source.png       |  Bin 9949 -> 0 bytes |
 | .../Areas/DIM/Content/Images/help/target.png       |  Bin 11421 -> 0 bytes |
 | .../Areas/DIM/Controllers/API/DataOutController.cs |    2 + |
 | .../DIM/Controllers/API/MetadataOutController.cs   |   14 + |
 | .../Areas/DIM/Controllers/ExportController.cs      |   26 + |
 | .../Areas/DIM/Controllers/HelpController.cs        |   14 +- |
 | .../Areas/DIM/Controllers/SubmissionController.cs  |   31 +- |
| Console/BExIS.Web.Shell/Areas/DIM/Dim.Settings.xml |    2 +- |
 | .../Areas/DIM/Helper/DimSeedDataGenerator.cs       |    8 +- |
 | .../PublishProfiles/FolderProfile.pubxml           |   18 + |
 | .../Areas/DIM/Scripts/DIM/bexis.dim.mapping.js     |  324 +- |
 | .../Controllers/ShowMultimediaDataController.cs    |  179 +- |
 | .../Areas/MMM/IDIV.Modules.Mmm.UI.csproj           |   23 +- |
 | .../Areas/MMM/Models/DatasetInfoModel.cs           |   36 + |
 | .../PublishProfiles/FolderProfile.pubxml           |   18 + |
 | .../Properties/PublishProfiles/TestServer.pubxml   |    2 +- |
 | .../Areas/MMM/Stylesheets/MultimediaData.css       |   12 +- |
 | .../Views/ShowMultimediaData/_audioPreview.cshtml  |    7 +- |
 | .../ShowMultimediaData/_bundleImageView.cshtml     |    4 +- |
 | .../MMM/Views/ShowMultimediaData/_imageView.cshtml |    4 +- |
 | .../ShowMultimediaData/_multimediaData.cshtml      |  120 +- |
 | .../Views/ShowMultimediaData/_videoPreview.cshtml  |    5 +- |
 | .../BExIS.Web.Shell/Areas/MMM/x64/MediaInfo.dll    |  Bin 0 -> 5461840 bytes |
 | .../BExIS.Web.Shell/Areas/MMM/x86/MediaInfo.dll    |  Bin 0 -> 4586832 bytes |
 | .../Areas/RPM/BExIS.Modules.Rpm.UI.csproj          |   28 +- |
 | .../Areas/RPM/Content/Images/Help/Help_img6.png    |  Bin 113709 -> 0 bytes |
 | .../RPM/Content/Images/Help/add_variables.png      |  Bin 61926 -> 0 bytes |
 | .../Content/Images/Help/copy_data_structure.png    |  Bin 64220 -> 0 bytes |
 | .../Content/Images/Help/create_data_structure.png  |  Bin 24771 -> 0 bytes |
 | .../RPM/Content/Images/Help/create_data_type.png   |  Bin 41763 -> 0 bytes |
 | .../Areas/RPM/Content/Images/Help/create_unit.png  |  Bin 58537 -> 0 bytes |
 | .../RPM/Content/Images/Help/create_variable.png    |  Bin 43140 -> 0 bytes |
 | .../RPM/Content/Images/Help/delete_button.png      |  Bin 604 -> 0 bytes |
 | .../Areas/RPM/Content/Images/Help/edit_button.png  |  Bin 407 -> 0 bytes |
 | .../Areas/RPM/Content/Images/Help/main_menu.png    |  Bin 38083 -> 0 bytes |
 | .../RPM/Content/Images/Help/manage_data_type.png   |  Bin 38538 -> 0 bytes |
 | .../Areas/RPM/Controllers/HelpController.cs        |   14 +- |
 | .../Controllers/Legacy/DataAttributeController.cs  |   19 +- |
 | .../Areas/RPM/Models/DataStructureEditModel.cs     |   22 +- |
 | .../Areas/RPM/Models/DataStructureSearchModel.cs   |   49 +- |
 | .../Areas/RPM/Models/MessageModel.cs               |   22 +- |
 | .../PublishProfiles/FolderProfile.pubxml           |   18 + |
 | .../Properties/PublishProfiles/TestServer.pubxml   |    2 +- |
| Console/BExIS.Web.Shell/Areas/RPM/Rpm.Settings.xml |    2 +- |
 | .../Views/DataStructureEdit/_dataStructure.cshtml  |   21 +- |
 | .../Areas/SAM/BExIS.Modules.Sam.UI.csproj          |   26 +- |
 | .../Areas/SAM/Content/Images/Help/Help_img10.png   |  Bin 31091 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/Help_img11.png   |  Bin 86844 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/create_group.png |  Bin 37409 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/create_user.png  |  Bin 48463 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/edit_group.png   |  Bin 42428 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/edit_user.png    |  Bin 51417 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/features.png     |  Bin 67120 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/groups.png       |  Bin 34797 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/login.png        |  Bin 41388 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/register.png     |  Bin 43722 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/token.png        |  Bin 44058 -> 0 bytes |
 | .../Areas/SAM/Content/Images/Help/users.png        |  Bin 51869 -> 0 bytes |
 | .../SAM/Controllers/EntityPermissionsController.cs |   13 +- |
 | .../Controllers/FeaturePermissionsController.cs    |   13 +- |
 | .../Areas/SAM/Controllers/GroupsController.cs      |    8 +- |
 | .../Areas/SAM/Controllers/HelpController.cs        |   20 +- |
 | .../Areas/SAM/Controllers/ModulesController.cs     |   56 +- |
 | .../Areas/SAM/Controllers/RequestsController.cs    |   13 +- |
 | .../SAM/Controllers/UserPermissionsController.cs   |   15 +- |
 | .../Areas/SAM/Controllers/UsersController.cs       |    4 +- |
 | .../Areas/SAM/Models/DecisionModels.cs             |    8 +- |
 | .../Areas/SAM/Models/RequestModels.cs              |    7 +- |
 | .../PublishProfiles/FolderProfile.pubxml           |   18 + |
 | .../Properties/PublishProfiles/TestServer.pubxml   |    2 +- |
| Console/BExIS.Web.Shell/Areas/SAM/Sam.Settings.xml |    2 +- |
 | .../SAM/Views/EntityPermissions/_Instances.cshtml  |   10 + |
 | .../Areas/SAM/Views/Groups/Index.cshtml            |    2 +- |
 | .../Areas/SAM/Views/Help/Informations.xml          |    2 +- |
 | .../Areas/SAM/Views/Requests/_Decisions.cshtml     |    6 +- |
 | .../SAM/Views/UserPermissions/_Subjects.cshtml     |    4 +- |
 | .../Areas/VIM/BExIS.Modules.Vim.UI.csproj          |   15 +- |
 | .../Areas/VIM/Content/Images/help/ui.png           |  Bin 30069 -> 0 bytes |
 | .../Areas/VIM/Content/Images/help/ui_category.png  |  Bin 32319 -> 0 bytes |
 | .../Areas/VIM/Content/Images/help/ui_slider.png    |  Bin 30675 -> 0 bytes |
 | .../Areas/VIM/Content/Images/help/ui_year.png      |  Bin 32365 -> 0 bytes |
 | .../Areas/VIM/Controllers/HelpController.cs        |   16 +- |
 | .../PublishProfiles/FolderProfile.pubxml           |   18 + |
 | .../Properties/PublishProfiles/TestServer.pubxml   |    2 +- |
| Console/BExIS.Web.Shell/Areas/VIM/Vim.Settings.xml |    2 +- |
| Console/BExIS.Web.Shell/BExIS.Web.Shell.csproj     |   26 +- |
| Console/BExIS.Web.Shell/Content/Site.css           |   30 +- |
| Console/BExIS.Web.Shell/Content/docs.css           |   59 + |
 | .../Controllers/AccountController.cs               |   43 +- |
 | .../BExIS.Web.Shell/Controllers/HelpController.cs  |   27 + |
 | .../BExIS.Web.Shell/Controllers/HomeController.cs  |   34 +- |
 | .../BExIS.Web.Shell/Controllers/JSController.cs    |   17 + |
 | .../BExIS.Web.Shell/Controllers/LdapController.cs  |    2 +- |
| Console/BExIS.Web.Shell/General.Settings.xml       |    8 +- |
| Console/BExIS.Web.Shell/Global.asax.cs             |   14 +- |
 | .../BExIS.Web.Shell/Instruction/Instruction.txt    |   37 + |
| Console/BExIS.Web.Shell/Models/AccountModels.cs    |    4 +- |
 | .../PublishProfiles/FolderProfile.pubxml           |   18 + |
| Console/BExIS.Web.Shell/Scripts/session.warning.js |  229 + |
| Console/BExIS.Web.Shell/Shell.Manifest.xml         |    6 + |
 | .../Themes/Default/Layouts/_Layout.cshtml          |   14 +- |
 | .../Themes/Default/Partials/_Footer.cshtml         |    2 +- |
 | .../Themes/Default/Styles/bexis-elements.css       |    9 +- |
 | .../Themes/Default/Styles/bexis-jquery-ui.css      |    2 +- |
 | .../Views/Home/SessionTimeout.cshtml               |   16 +- |
| Console/BExIS.Web.Shell/Views/Shared/Error.cshtml  |    3 +- |
| Console/BExIS.Web.Shell/Views/Shared/Info.cshtml   |    4 +- |
 | .../Views/Shared/SessionTimeout.cshtml             |   13 + |
 | .../BExIS.Web.Shell/Web.TestServerRelease.config   |   12 + |
| Console/BExIS.Web.Shell/packages.config            |    1 + |
| Console/BExIS.Web.Shell/web.config                 |   16 +- |
 MODULES.md                                         |    4 +- |
 | .../2.10.0/BEXIS2100_DataCollection_UserGuide.docx |  Bin 2931397 -> 0 bytes |
 | .../BEXIS2100_DataDissemination_UserGuide.docx     |  Bin 131478 -> 0 bytes |
 | .../2.10.0/BEXIS2100_DataPlanning_UserGuide.docx   |  Bin 2252298 -> 0 bytes |
 Manuals/2.10.0/BEXIS2100_Installation_Manual.docx  |  Bin 3570972 -> 0 bytes |
 | .../2.10.0/BEXIS2100_PartyPackage_UserGuide.docx   |  Bin 612988 -> 0 bytes |
 Manuals/2.10.0/BEXIS2100_SearchUI_UserGuide.docx   |  Bin 906294 -> 0 bytes |
 | .../2.10.0/BEXIS2100_SystemAdmin_UserGuide.docx    |  Bin 439467 -> 0 bytes |
 | .../Administration/Manage Feature.jpg              |  Bin 205012 -> 0 bytes |
 | .../Data| Collection/Create-Dataset.png             |  Bin 20102 -> 0 bytes |
 | .../Data| Collection/Dataset_Copy.jpg               |  Bin 113147 -> 0 bytes |
 | .../Data| Collection/Import-Metadata-1.jpg          |  Bin 176379 -> 0 bytes |
 | .../Data| Collection/Import-Metadata-2.jpg          |  Bin 197240 -> 0 bytes |
 | .../Data| Collection/Import-Metadata-3.jpg          |  Bin 181872 -> 0 bytes |
 | .../Data| Collection/Import-Metadata-4.jpg          |  Bin 108326 -> 0 bytes |
 | .../Data| Collection/Push_Big_File.jpg              |  Bin 184578 -> 0 bytes |
 | .../Data| Collection/Upload_Data.jpg                |  Bin 98983 -> 0 bytes |
 | .../Data| Collection/Upload_File.jpg                |  Bin 256762 -> 0 bytes |
 | .../Data| Collection/Upload_Tabular_1.jpg           |  Bin 223766 -> 0 bytes |
 | .../Data| Collection/Upload_Tabular_2.jpg           |  Bin 181729 -> 0 bytes |
 | .../Data| Collection/Upload_Tabular_Validate.jpg    |  Bin 173992 -> 0 bytes |
 | .../Data Planning/Create-Datastructure.jpg         |  Bin 120853 -> 0 bytes |
 | .../Data Planning/Datastructure-Edit.jpg           |  Bin 350359 -> 0 bytes |
 | .../Data Planning/Datastructure-copy.jpg           |  Bin 328220 -> 0 bytes |
 | .../Data Planning/Main_Menu_1.jpg                  |  Bin 127288 -> 0 bytes |
 | .../Data Planning/Main_Menu_2.jpg                  |  Bin 149533 -> 0 bytes |
 | .../Data Planning/Manage_Data_Attributes.jpg       |  Bin 299122 -> 0 bytes |
 | .../Data Planning/Manage_Data_Type.jpg             |  Bin 197230 -> 0 bytes |
 | .../Data Planning/Manage_Units.jpg                 |  Bin 352270 -> 0 bytes |
 | .../BPP2100_Screenshots/Installation/tool.jpg      |  Bin 71027 -> 0 bytes |
 | .../2.11.0/BEXIS2110_DataCollection_UserGuide.docx |  Bin 2902855 -> 0 bytes |
 | .../BEXIS2110_DataDissemination_UserGuide.docx     |  Bin 679942 -> 0 bytes |
 | .../2.11.0/BEXIS2110_DataPlanning_UserGuide.docx   |  Bin 1159357 -> 0 bytes |
 Manuals/2.11.0/BEXIS2110_Installation_Manual.docx  |  Bin 3588315 -> 0 bytes |
 | .../2.11.0/BEXIS2110_PartyPackage_UserGuide.docx   |  Bin 621919 -> 0 bytes |
 Manuals/2.11.0/BEXIS2110_SearchUI_UserGuide.docx   |  Bin 902138 -> 0 bytes |
 | .../2.11.0/BEXIS2110_SystemAdmin_UserGuide.docx    |  Bin 390790 -> 0 bytes |
 | .../Administration/Manage Feature.jpg              |  Bin 205012 -> 0 bytes |
 | .../Data| Collection/Create-Dataset.png             |  Bin 20102 -> 0 bytes |
 | .../Data| Collection/Dataset_Copy.jpg               |  Bin 113147 -> 0 bytes |
 | .../Data| Collection/Import-Metadata-1.jpg          |  Bin 176379 -> 0 bytes |
 | .../Data| Collection/Import-Metadata-2.jpg          |  Bin 197240 -> 0 bytes |
 | .../Data| Collection/Import-Metadata-3.jpg          |  Bin 181872 -> 0 bytes |
 | .../Data| Collection/Import-Metadata-4.jpg          |  Bin 108326 -> 0 bytes |
 | .../Data| Collection/Push_Big_File.jpg              |  Bin 184578 -> 0 bytes |
 | .../Data| Collection/Upload_Data.jpg                |  Bin 98983 -> 0 bytes |
 | .../Data| Collection/Upload_File.jpg                |  Bin 256762 -> 0 bytes |
 | .../Data| Collection/Upload_Tabular_1.jpg           |  Bin 223766 -> 0 bytes |
 | .../Data| Collection/Upload_Tabular_2.jpg           |  Bin 181729 -> 0 bytes |
 | .../Data| Collection/Upload_Tabular_Validate.jpg    |  Bin 173992 -> 0 bytes |
 | .../Data Planning/Create-Datastructure.jpg         |  Bin 120853 -> 0 bytes |
 | .../Data Planning/Datastructure-Edit.jpg           |  Bin 350359 -> 0 bytes |
 | .../Data Planning/Datastructure-copy.jpg           |  Bin 328220 -> 0 bytes |
 | .../Data Planning/Main_Menu_1.jpg                  |  Bin 127288 -> 0 bytes |
 | .../Data Planning/Main_Menu_2.jpg                  |  Bin 149533 -> 0 bytes |
 | .../Data Planning/Manage_Data_Attributes.jpg       |  Bin 299122 -> 0 bytes |
 | .../Data Planning/Manage_Data_Type.jpg             |  Bin 197230 -> 0 bytes |
 | .../Data Planning/Manage_Units.jpg                 |  Bin 352270 -> 0 bytes |
 | .../BPP2100_Screenshots/Installation/tool.jpg      |  Bin 71027 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/1.jpg       |  Bin 61378 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/10.jpg      |  Bin 18712 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/11.jpg      |  Bin 29218 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/12.jpg      |  Bin 57375 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/13.jpg      |  Bin 18521 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/14.jpg      |  Bin 81362 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/15.jpg      |  Bin 39229 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/16.jpg      |  Bin 24986 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/17.jpg      |  Bin 86286 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/18.jpg      |  Bin 75482 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/2.jpg       |  Bin 121160 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/3.jpg       |  Bin 89640 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/3.png       |  Bin 30678 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/4.jpg       |  Bin 95217 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/5.jpg       |  Bin 74958 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/6.jpg       |  Bin 98082 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/7.jpg       |  Bin 82754 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/8.jpg       |  Bin 77094 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/9.jpg       |  Bin 82754 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/BAM/Thumbs.db   |  Bin 106496 -> 0 bytes |
 | .../BPP2110_Screenshots/DCM/Create_Dataset.jpg     |  Bin 129992 -> 0 bytes |
 | .../BPP2110_Screenshots/DCM/create_dataset.png     |  Bin 26362 -> 0 bytes |
 | .../BPP2110_Screenshots/DCM/push_big_file.png      |  Bin 34295 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DCM/read_xsd.png    |  Bin 38175 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DCM/red_star.png    |  Bin 315 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DCM/select_file.png |  Bin 42446 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DCM/select_xsd.png  |  Bin 34532 -> 0 bytes |
 | .../BPP2110_Screenshots/DCM/set_xsd_parameters.png |  Bin 35361 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DCM/summary_xsd.png |  Bin 27699 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DCM/upload_data.png |  Bin 24141 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DIM/collections.png |  Bin 44795 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DIM/many_to_one.png |  Bin 54242 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/DIM/mapping.png |  Bin 123242 -> 0 bytes |
 | .../BPP2110_Screenshots/DIM/mapping_tool.png       |  Bin 32274 -> 0 bytes |
 | .../BPP2110_Screenshots/DIM/mapping_tool2.png      |  Bin 40918 -> 0 bytes |
 | .../BPP2110_Screenshots/DIM/mapping_tool3.png      |  Bin 36900 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DIM/one_to_many.png |  Bin 54834 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DIM/one_to_one.png  |  Bin 27772 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DIM/pangaea1.png    |  Bin 104710 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/DIM/pangaea2.png    |  Bin 63428 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/DIM/publish.png |  Bin 41472 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/DIM/source.png  |  Bin 9949 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/DIM/target.png  |  Bin 11421 -> 0 bytes |
 | .../Other/DB2_Create_Database.png                  |  Bin 14663 -> 0 bytes |
 | .../Other/DB2_Database_Setting.png                 |  Bin 20508 -> 0 bytes |
 | .../Other/DB2_New_Database_.png                    |  Bin 9130 -> 0 bytes |
 | .../BPP2110_Screenshots/Other/DB2_created_db.png   |  Bin 7818 -> 0 bytes |
 | .../BPP2110_Screenshots/RPM/add_variables.png      |  Bin 61926 -> 0 bytes |
 | .../RPM/copy_data_structure.png                    |  Bin 64220 -> 0 bytes |
 | .../RPM/create_data_structure.png                  |  Bin 24771 -> 0 bytes |
 | .../BPP2110_Screenshots/RPM/create_data_type.png   |  Bin 41763 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/RPM/create_unit.png |  Bin 58537 -> 0 bytes |
 | .../BPP2110_Screenshots/RPM/create_variable.png    |  Bin 43140 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/RPM/main_menu.png   |  Bin 38083 -> 0 bytes |
 | .../BPP2110_Screenshots/RPM/manage_data_type.png   |  Bin 38538 -> 0 bytes |
 | .../BPP2110_Screenshots/SAM/create_group.png       |  Bin 29966 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/SAM/create_user.png |  Bin 29939 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/SAM/edit_group.png  |  Bin 31609 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/SAM/edit_user.png   |  Bin 31044 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/SAM/features.png    |  Bin 47202 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/SAM/groups.png  |  Bin 31561 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/SAM/login.png   |  Bin 19983 -> 0 bytes |
 | .../2.11.0/BPP2110_Screenshots/SAM/register.png    |  Bin 20047 -> 0 bytes |
 Manuals/2.11.0/BPP2110_Screenshots/SAM/users.png   |  Bin 28653 -> 0 bytes |
 Manuals/2.11.1/BEXIS2111_Installation_Manual.docx  |  Bin 3512185 -> 0 bytes |
 | .../2.11.2/BEXIS2112_DataCollection_UserGuide.docx |  Bin 2911972 -> 0 bytes |
 | .../BEXIS2112_DataDissemination_UserGuide.docx     |  Bin 684578 -> 0 bytes |
 | .../2.11.2/BEXIS2112_DataPlanning_UserGuide.docx   |  Bin 1166344 -> 0 bytes |
 Manuals/2.11.2/BEXIS2112_Installation_Manual.docx  |  Bin 3512455 -> 0 bytes |
 | .../2.11.2/BEXIS2112_PartyPackage_UserGuide.docx   |  Bin 621724 -> 0 bytes |
 Manuals/2.11.2/BEXIS2112_SearchUI_UserGuide.docx   |  Bin 906469 -> 0 bytes |
 | .../2.11.2/BEXIS2112_SystemAdmin_UserGuide.docx    |  Bin 395288 -> 0 bytes |
 | .../2.11.3/BEXIS2113_DataCollection_UserGuide.docx |  Bin 2903206 -> 0 bytes |
 | .../BEXIS2113_DataDissemination_UserGuide.docx     |  Bin 680353 -> 0 bytes |
 | .../2.11.3/BEXIS2113_DataPlanning_UserGuide.docx   |  Bin 1159731 -> 0 bytes |
 Manuals/2.11.3/BEXIS2113_Installation_Manual.docx  |  Bin 3498946 -> 0 bytes |
 | .../2.11.3/BEXIS2113_PartyPackage_UserGuide.docx   |  Bin 613208 -> 0 bytes |
 Manuals/2.11.3/BEXIS2113_SearchUI_UserGuide.docx   |  Bin 902519 -> 0 bytes |
 | .../2.11.3/BEXIS2113_SystemAdmin_UserGuide.docx    |  Bin 391125 -> 0 bytes |
 | .../2.12.1/BEXIS2121_DataCollection_UserGuide.docx |  Bin 3093591 -> 0 bytes |
 | .../BEXIS2121_DataDissemination_UserGuide.docx     |  Bin 658682 -> 0 bytes |
 | .../2.12.1/BEXIS2121_DataPlanning_UserGuide.docx   |  Bin 1166303 -> 0 bytes |
 Manuals/2.12.1/BEXIS2121_Installation_Manual.docx  |  Bin 1376241 -> 0 bytes |
 | .../2.12.1/BEXIS2121_PartyPackage_UserGuide.docx   |  Bin 620965 -> 0 bytes |
 Manuals/2.12.1/BEXIS2121_SearchUI_UserGuide.docx   |  Bin 1235077 -> 0 bytes |
 | .../2.12.1/BEXIS2121_SystemAdmin_UserGuide.docx    |  Bin 624941 -> 0 bytes |
 Manuals/2.12.1/BEXIS2121_UserGuide.docx            |  Bin 640295 -> 0 bytes |
 | .../2.12.1/BEXIS2121_Visualization_UserGuide.docx  |  Bin 152671 -> 0 bytes |
 | .../2.12.2/BEXIS2122_DataCollection_UserGuide.docx |  Bin 3093597 -> 0 bytes |
 | .../BEXIS2122_DataDissemination_UserGuide.docx     |  Bin 664057 -> 0 bytes |
 | .../2.12.2/BEXIS2122_DataPlanning_UserGuide.docx   |  Bin 1166646 -> 0 bytes |
 Manuals/2.12.2/BEXIS2122_Installation_Manual.docx  |  Bin 1376224 -> 0 bytes |
 | .../2.12.2/BEXIS2122_PartyPackage_UserGuide.docx   |  Bin 621864 -> 0 bytes |
 Manuals/2.12.2/BEXIS2122_SearchUI_UserGuide.docx   |  Bin 1210938 -> 0 bytes |
 | .../2.12.2/BEXIS2122_SystemAdmin_UserGuide.docx    |  Bin 616361 -> 0 bytes |
 Manuals/2.12.2/BEXIS2122_UserGuide.docx            |  Bin 640638 -> 0 bytes |
 | .../2.12.2/BEXIS2122_Visualization_UserGuide.docx  |  Bin 152370 -> 0 bytes |
 | .../2.12/BEXIS212_DataCollection_UserGuide.docx    |  Bin 3093486 -> 0 bytes |
 | .../2.12/BEXIS212_DataDissemination_UserGuide.docx |  Bin 658756 -> 0 bytes |
 Manuals/2.12/BEXIS212_DataPlanning_UserGuide.docx  |  Bin 1159750 -> 0 bytes |
 Manuals/2.12/BEXIS212_Installation_Manual.docx     |  Bin 1363718 -> 0 bytes |
 Manuals/2.12/BEXIS212_PartyPackage_UserGuide.docx  |  Bin 613416 -> 0 bytes |
 Manuals/2.12/BEXIS212_SearchUI_UserGuide.docx      |  Bin 1235621 -> 0 bytes |
 Manuals/2.12/BEXIS2_12_SystemAdmin_UserGuide.docx  |  Bin 577033 -> 0 bytes |
 Manuals/2.12/BEXIS2_12_UserGuide.docx              |  Bin 633214 -> 0 bytes |
 | .../2.12/BEXIS2_12_Visualization_UserGuide.docx    |  Bin 148078 -> 0 bytes |
 | .../2.4.0/BEXIS240_DataCollection_UserGuide.docx   |  Bin 1553535 -> 0 bytes |
 Manuals/2.4.0/BEXIS240_DataPlanning_UserGuide.docx |  Bin 288346 -> 0 bytes |
 Manuals/2.4.0/BEXIS240_SearchManager_Manual.docx   |  Bin 31408 -> 0 bytes |
 Manuals/2.4.0/BEXIS240_SearchUI_Manual.docx        |  Bin 354989 -> 0 bytes |
 | .../2.4.0/BEXIS240_SecuritySystem_UsersGuide.docx  |  Bin 455583 -> 0 bytes |
 Manuals/2.4.0/Install_Manual_BPP250.docx           |  Bin 3607809 -> 0 bytes |
 | .../2.5.0/BEXIS250_DataCollection_UserGuide.docx   |  Bin 1553992 -> 0 bytes |
 | .../2.5.0/BEXIS250_DataDissemination_Manual.docx   |  Bin 78980 -> 0 bytes |
 Manuals/2.5.0/BEXIS250_DataPlanning_UserGuide.docx |  Bin 290782 -> 0 bytes |
 Manuals/2.5.0/BEXIS250_Installation_Manual.docx    |  Bin 3607809 -> 0 bytes |
 Manuals/2.5.0/BEXIS250_SearchManager_Manual.docx   |  Bin 30405 -> 0 bytes |
 Manuals/2.5.0/BEXIS250_SearchUI_Manual.docx        |  Bin 354205 -> 0 bytes |
 | .../2.5.0/BEXIS250_SecuritySystem_UsersGuide.docx  |  Bin 969967 -> 0 bytes |
 | .../2.5.1/BEXIS251_DataCollection_UserGuide.docx   |  Bin 1553992 -> 0 bytes |
 | .../2.5.1/BEXIS251_DataDissemination_Manual.docx   |  Bin 78980 -> 0 bytes |
 Manuals/2.5.1/BEXIS251_DataPlanning_UserGuide.docx |  Bin 290782 -> 0 bytes |
 Manuals/2.5.1/BEXIS251_Installation_Manual.docx    |  Bin 3593993 -> 0 bytes |
 Manuals/2.5.1/BEXIS251_SearchManager_Manual.docx   |  Bin 30405 -> 0 bytes |
 Manuals/2.5.1/BEXIS251_SearchUI_Manual.docx        |  Bin 354205 -> 0 bytes |
 | .../2.5.1/BEXIS251_SecuritySystem_UsersGuide.docx  |  Bin 455943 -> 0 bytes |
 | .../2.6.0/BEXIS260_DataCollection_UserGuide.docx   |  Bin 1236381 -> 0 bytes |
 | .../2.6.0/BEXIS260_DataDissemination_Manual.docx   |  Bin 83389 -> 0 bytes |
 Manuals/2.6.0/BEXIS260_DataPlanning_UserGuide.docx |  Bin 288901 -> 0 bytes |
 Manuals/2.6.0/BEXIS260_Installation_Manual.docx    |  Bin 3593788 -> 0 bytes |
 Manuals/2.6.0/BEXIS260_SearchUI_Manual.docx        |  Bin 474430 -> 0 bytes |
 | .../2.6.0/BEXIS260_SecuritySystem_UsersGuide.docx  |  Bin 301912 -> 0 bytes |
 | .../2.6.1/BEXIS261_DataCollection_UserGuide.docx   |  Bin 1396765 -> 0 bytes |
 | .../2.6.1/BEXIS261_DataDissemination_Manual.docx   |  Bin 72376 -> 0 bytes |
 Manuals/2.6.1/BEXIS261_DataPlanning_UserGuide.docx |  Bin 288645 -> 0 bytes |
 Manuals/2.6.1/BEXIS261_Installation_Manual.docx    |  Bin 3594185 -> 0 bytes |
 Manuals/2.6.1/BEXIS261_SearchUI_Manual.docx        |  Bin 475585 -> 0 bytes |
 | .../2.6.1/BEXIS261_SecuritySystem_UsersGuide.docx  |  Bin 301923 -> 0 bytes |
 | .../2.7.0/BEXIS270_DataCollection_UserGuide.docx   |  Bin 1406960 -> 0 bytes |
 | .../2.7.0/BEXIS270_DataDissemination_Manual.docx   |  Bin 72719 -> 0 bytes |
 | .../BEXIS270_DataDissemination_UsersGuide.docx     |  Bin 72719 -> 0 bytes |
 Manuals/2.7.0/BEXIS270_DataPlanning_UserGuide.docx |  Bin 315937 -> 0 bytes |
 Manuals/2.7.0/BEXIS270_Installation_Manual.docx    |  Bin 4315222 -> 0 bytes |
 Manuals/2.7.0/BEXIS270_SearchUI_UsersGuide.docx    |  Bin 475930 -> 0 bytes |
 Manuals/2.7.0/BEXIS270_SystemAdmin_UsersGuide.docx |  Bin 383869 -> 0 bytes |
 Manuals/2.7.1/Preparing Help files.txt             |   17 - |
 | .../2.8.0/BEXIS280_DataCollection_UserGuide.docx   |  Bin 1378388 -> 0 bytes |
 | .../2.8.0/BEXIS280_DataDissemination_Manual.docx   |  Bin 63471 -> 0 bytes |
 | .../BEXIS280_DataDissemination_UsersGuide.docx     |  Bin 66918 -> 0 bytes |
 Manuals/2.8.0/BEXIS280_DataPlanning_UserGuide.docx |  Bin 546247 -> 0 bytes |
 Manuals/2.8.0/BEXIS280_Installation_Manual.docx    |  Bin 3566187 -> 0 bytes |
 Manuals/2.8.0/BEXIS280_SearchUI_UsersGuide.docx    |  Bin 902267 -> 0 bytes |
 Manuals/2.8.0/BEXIS280_SystemAdmin_UsersGuide.docx |  Bin 524263 -> 0 bytes |
 Manuals/2.8.0/Screenshots.zip                      |  Bin 1177743 -> 0 bytes |
 | .../2.8.1/BEXIS281_DataCollection_UserGuide.docx   |  Bin 1378452 -> 0 bytes |
 | .../2.8.1/BEXIS281_DataDissemination_Manual.docx   |  Bin 63492 -> 0 bytes |
 | .../BEXIS281_DataDissemination_UsersGuide.docx     |  Bin 67130 -> 0 bytes |
 Manuals/2.8.1/BEXIS281_DataPlanning_UserGuide.docx |  Bin 549874 -> 0 bytes |
 Manuals/2.8.1/BEXIS281_Installation_Manual.docx    |  Bin 3567481 -> 0 bytes |
 Manuals/2.8.1/BEXIS281_SearchUI_UsersGuide.docx    |  Bin 905851 -> 0 bytes |
 Manuals/2.8.1/BEXIS281_SystemAdmin_UsersGuide.docx |  Bin 528650 -> 0 bytes |
 Manuals/2.8.1/Screenshots.zip                      |  Bin 1177743 -> 0 bytes |
 | .../2.9.0/BEXIS290_DataCollection_UserGuide.docx   |  Bin 1330710 -> 0 bytes |
 | .../BEXIS290_DataDissemination_UserGuide.docx      |  Bin 57507 -> 0 bytes |
 Manuals/2.9.0/BEXIS290_DataPlanning_UserGuide.docx |  Bin 692493 -> 0 bytes |
 Manuals/2.9.0/BEXIS290_Installation_Manual.docx    |  Bin 3568744 -> 0 bytes |
 Manuals/2.9.0/BEXIS290_SearchUI_UserGuide.docx     |  Bin 885847 -> 0 bytes |
 Manuals/2.9.0/BEXIS290_SystemAdmin_UserGuide.docx  |  Bin 443887 -> 0 bytes |
 Manuals/2.9.0/Preparing Help files.txt             |   17 - |
 | .../Screenshots/Administration/Manage Features.png |  Bin 66009 -> 0 bytes |
 | .../Screenshots/Administration/create group.png    |  Bin 9921 -> 0 bytes |
 | .../Screenshots/Administration/create user.png     |  Bin 57561 -> 0 bytes |
 | .../Screenshots/Administration/data permission.png |  Bin 44992 -> 0 bytes |
 | .../Screenshots/Administration/edit group.png      |  Bin 51542 -> 0 bytes |
 | .../2.9.0/Screenshots/Administration/groups.png    |  Bin 29869 -> 0 bytes |
 | .../Screenshots/Administration/registration.png    |  Bin 48887 -> 0 bytes |
 Manuals/2.9.0/Screenshots/Administration/users.png |  Bin 35424 -> 0 bytes |
 | .../Data| Collection/Collect_upload Structured.png  |  Bin 48691 -> 0 bytes |
 | .../Screenshots/Data| Collection/Copy Dataset.png   |  Bin 122528 -> 0 bytes |
 | .../Screenshots/Data| Collection/Create Dataset.bmp |  Bin 691206 -> 0 bytes |
 | .../Screenshots/Data| Collection/Enable Macro.png   |  Bin 197647 -> 0 bytes |
 | .../Screenshots/Data| Collection/Primary Key.png    |  Bin 41183 -> 0 bytes |
 | .../Screenshots/Data| Collection/Push big file.png  |  Bin 35048 -> 0 bytes |
 | .../Screenshots/Data| Collection/Read Source.png    |  Bin 43295 -> 0 bytes |
 | .../Screenshots/Data| Collection/Select File.png    |  Bin 35472 -> 0 bytes |
 | .../Screenshots/Data| Collection/Set Param.png      |  Bin 37839 -> 0 bytes |
 | .../Screenshots/Data| Collection/Set Parameters.png |  Bin 49114 -> 0 bytes |
 | .../Data| Collection/Specify Dataset.png            |  Bin 42573 -> 0 bytes |
 | .../2.9.0/Screenshots/Data| Collection/Summary.png  |  Bin 33332 -> 0 bytes |
 | .../Data| Collection/Upload Unstructures.png        |  Bin 50318 -> 0 bytes |
 | .../Screenshots/Data| Collection/Upload data.png    |  Bin 20038 -> 0 bytes |
 | .../Screenshots/Data| Collection/Validation.png     |  Bin 40204 -> 0 bytes |
 | .../Data Dissemination/Export Metadata.png         |  Bin 30681 -> 0 bytes |
 | .../Screenshots/Data Planning/enableMacro.png      |  Bin 132875 -> 0 bytes |
 | .../Screenshots/Data Planning/plan_SaveAs.png      |  Bin 74424 -> 0 bytes |
 | .../Data Planning/plan_createDatastructure.png     |  Bin 28709 -> 0 bytes |
 | .../Data Planning/plan_dataAttribute.png           |  Bin 48999 -> 0 bytes |
 | .../Data Planning/plan_dataStructureManager.png    |  Bin 17988 -> 0 bytes |
 | .../Screenshots/Data Planning/plan_datatype.png    |  Bin 29669 -> 0 bytes |
 | .../2.9.0/Screenshots/Data Planning/plan_menu.png  |  Bin 19267 -> 0 bytes |
 | .../2.9.0/Screenshots/Data Planning/plan_unit.png  |  Bin 53667 -> 0 bytes |
 | .../Search UI/create search attribute.png          |  Bin 83803 -> 0 bytes |
 | .../2.9.0/Screenshots/Search UI/data structure.png |  Bin 50006 -> 0 bytes |
 | .../Screenshots/Search UI/dataset permission.png   |  Bin 45553 -> 0 bytes |
 Manuals/2.9.0/Screenshots/Search UI/metadata.png   |  Bin 33645 -> 0 bytes |
 | .../2.9.0/Screenshots/Search UI/primary data.png   |  Bin 46506 -> 0 bytes |
 | .../2.9.0/Screenshots/Search UI/search Manager.png |  Bin 79314 -> 0 bytes |
 Manuals/2.9.1/BEXIS291_Installation_Manual.docx    |  Bin 3583919 -> 0 bytes |
 | .../BExIS.Dcm.CreateDatasetWizard.csproj           |    9 + |
 | .../CreateTaskManager.cs                           |    7 +- |
 | .../BExIS.Dcm.ImportMetadataStructureWizard.csproj |    9 + |
 | .../BExIS.Dcm.UploadWizard.csproj                  |    9 + |
 | .../DCM/BExIS.Dcm.Wizard/BExIS.Dcm.Wizard.csproj   |    9 + |
 Modules/DDM/BExIS.Ddm.Api/BExIS.Ddm.Api.csproj     |    9 + |
 Modules/DDM/BExIS.Ddm.Model/BExIS.Ddm.Model.csproj |    9 + |
 | .../BExIS.Ddm.Providers.LuceneProvider.csproj      |    9 + |
 | .../Config/SearchConfig.cs                         |    7 +- |
 | .../Helpers/EncoderHelper.cs                       |   44 +- |
 | .../Indexer/BexisIndexer.cs                        |    7 +- |
 | .../SearchProvider.cs                              |   59 +- |
 | .../Searcher/BexisIndexSearcher.cs                 |    9 + |
 | .../BExIS.Dim.Entities/BExIS.Dim.Entities.csproj   |    9 + |
 | .../Mapping/MappingResultElemenet.cs               |    2 + |
 | .../DIM/BExIS.Dim.Helper/BExIS.Dim.Helpers.csproj  |    9 + |
 | .../DIM/BExIS.Dim.Helper/Mapping/MappingUtils.cs   |   13 +- |
 | .../DIM/BExIS.Dim.NH.ORM/BExIS.Dim.Orm.NH.csproj   |    9 + |
 | .../BExIS.Dim.Services/BExIS.Dim.Services.csproj   |    9 + |
 | .../BExIS.Sam.Providers.Ldap.csproj                |    9 + |
 README.md                                          |   42 + |
 Release Notes/BEXIS2100_Release_Notes.docx         |  Bin 30444 -> 0 bytes |
 Release Notes/BEXIS2112_Release_Note.docx          |  Bin 35660 -> 0 bytes |
 Release Notes/BEXIS2113_Release_Note.docx          |  Bin 30721 -> 0 bytes |
 Release Notes/BEXIS2121_Release_Note.docx          |  Bin 40898 -> 0 bytes |
 Release Notes/BEXIS2122_Release_Note.docx          |  Bin 42350 -> 0 bytes |
 Release Notes/BEXIS212_Release_Note.docx           |  Bin 43167 -> 0 bytes |
 Release Notes/BEXIS280_Release_Notes.docx          |  Bin 43023 -> 0 bytes |
 Release Notes/BEXIS281_Release_Notes.docx          |  Bin 42257 -> 0 bytes |
 Release Notes/Release_Notes.md                     |  129 + |
 Release Notes/Release_Notes_BPP250.docx            |  Bin 40176 -> 0 bytes |
 Release Notes/Release_Notes_BPP251.docx            |  Bin 40733 -> 0 bytes |
 Release Notes/Release_Notes_BPP260.docx            |  Bin 40549 -> 0 bytes |
 Release Notes/Release_Notes_BPP261.docx            |  Bin 40053 -> 0 bytes |
 Release Notes/Release_Notes_BPP270.docx            |  Bin 42305 -> 0 bytes |
 Release Notes/Release_Notes_BPP280.docx            |  Bin 41933 -> 0 bytes |
 Templates/Module.Template/Content/Site.css         |   24 - |
 Templates/Module.Template/Content/bootstrap.css    | 6816 -------------- |
 | .../Module.Template/Content/bootstrap.min.css      |   20 - |
 Templates/Module.Template/ModuleID.Manifest.xml    |   34 - |
 Templates/Module.Template/ModuleIDModule.cs        |   33 - |
 Templates/Module.Template/ProjectName.UI.csproj    |  175 - |
 | .../Module.Template/Properties/AssemblyInfo.cs     |   35 - |
 Templates/Module.Template/Scripts/_references.js   |  Bin 600 -> 0 bytes |
 | .../Scripts/ai.0.22.19-build00125.js               | 3859 -------- |
 | .../Scripts/ai.0.22.19-build00125.min.js           |    1 - |
 Templates/Module.Template/Scripts/bootstrap.js     | 2014 ---- |
 Templates/Module.Template/Scripts/bootstrap.min.js |   21 - |
 | .../Scripts/jquery-1.10.2.intellisense.js          | 2671 ------ |
 Templates/Module.Template/Scripts/jquery-1.10.2.js | 9803 -------------------- |
 | .../Module.Template/Scripts/jquery-1.10.2.min.js   |   23 - |
 | .../Module.Template/Scripts/jquery-1.10.2.min.map  |    1 - |
 | .../Scripts/jquery.validate-vsdoc.js               | 1288 --- |
 | .../Module.Template/Scripts/jquery.validate.js     | 1231 --- |
 | .../Module.Template/Scripts/jquery.validate.min.js |    2 - |
 | .../Scripts/jquery.validate.unobtrusive.js         |  429 - |
 | .../Scripts/jquery.validate.unobtrusive.min.js     |   19 - |
 | .../Module.Template/Scripts/modernizr-2.6.2.js     | 1416 --- |
 Templates/Module.Template/Scripts/respond.js       |  340 - |
 Templates/Module.Template/Scripts/respond.min.js   |   20 - |
 Templates/Module.Template/Views/Web.config         |   43 - |
 Templates/Module.Template/Views/_ViewStart.cshtml  |    5 - |
 Templates/Module.Template/packages.config          |   23 - |
 Templates/Module.Template/web.config               |    9 - |
 database update scripts/Update_Script_212to213.txt |    4 + |
 722 files changed, 6811 insertions(+), 52688 deletions(-)
