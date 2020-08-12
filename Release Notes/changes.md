# Version Changes 2.14.0 -> 2.14.1

^ in the document not all changes are indicated in the first section, only supposed important changes for other developers are indicated. The second section lists all files that have changed since the last release.

# May Important Changes

## Shell


- dbsettings: changing the batchs size

````xml
<property name="adonet.batch_size">65000</property>

````

- settings 
````xml
 <entry key="version" value="2.14.1" type="string" />
````

## DB
[go to the db update script v2.14-2.14.1](https://github.com/BEXIS2/Core/blob/releases/2.14.1/database%20update%20scripts/Update_Script_214to2141.sql)

## WORKSPACE

````
 .gitignore
+2 −1  General/Db/Settings/PostgreSQL82Dialect.hibernate.cfg.xml
+1 −1  General/General.Settings.xml
+1 −1  Modules/BAM/Bam.Settings.xml
+1 −1  Modules/DCM/Dcm.Settings.xml
+1 −1  Modules/DDM/Ddm.Settings.xml
BIN  Modules/DDM/Lucene/SearchIndex/BexisAutoComplete/segments.gen
BIN  Modules/DDM/Lucene/SearchIndex/BexisAutoComplete/segments_1
BIN  Modules/DDM/Lucene/SearchIndex/BexisSearchIndex/segments.gen
BIN  Modules/DDM/Lucene/SearchIndex/BexisSearchIndex/segments_1
+1 −1  Modules/DIM/Dim.Settings.xml
+1 −1  Modules/RPM/Rpm.Settings.xml
+1 −1  Modules/SAM/Sam.Settings.xml
+1 −1  Modules/VIM/Vim.Settings.xml

````

## UI

## web.config

- change web config changes listed here
````xml

<add key="ApplicationVersion" value="2.14.1" />

<!-- <system.webServer> -->
<staticContent>
      <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="1.00:00:00"/>
    </staticContent>
</system.webServer>

````


# All CHANGED Files 

$ git diff --stat origin/releases/2.13..origin/releases/2.14 --no-merges

 | File| Changes |
 |---|---|
 | .../Objects/DatasetStore.cs                        |   5 +
 | .../Objects/IEntityStore.cs                        |   2 +
 Components/XML/BExIS.Xml.Helpers/DatasetStore.cs   |  23 +++
 Console/BExIS.Web.Shell/Areas/BAM/Bam.Settings.xml |   2 +-
 | .../BAM/Controllers/PartyServiceController.cs      |  60 +++----
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 | .../PartyService/_userRegisterationPartial.cshtml  |   7 +
 Console/BExIS.Web.Shell/Areas/DCM/Dcm.Settings.xml |   2 +-
 | .../Areas/DCM/Helpers/DataASyncUploadHelper.cs     |  27 +++-
 | .../Areas/DCM/Helpers/EntityReferenceHelper.cs     |  73 ++++++---
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 | .../Areas/DDM/BExIS.Modules.Ddm.UI.csproj          |   1 +
 | .../Areas/DDM/Controllers/DataController.cs        |   4 +-
 | .../Areas/DDM/Controllers/HomeController.cs        |   2 +-
 | .../DDM/Controllers/PublicSearchController.cs      |   2 +-
 Console/BExIS.Web.Shell/Areas/DDM/Ddm.Settings.xml |   2 +-
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 | .../Areas/DDM/Views/Data/ShowData.cshtml           |   2 +-
 | .../DDM/Views/Data/_unstructuredDataView.cshtml    |   2 +-
 | .../Areas/DDM/Views/Shared/Error.cshtml            |  11 ++
 | .../Areas/DIM/Controllers/ExportController.cs      |   2 +-
 Console/BExIS.Web.Shell/Areas/DIM/Dim.Settings.xml |   2 +-
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 | .../Objects/DatasetStore.cs                        |   5 +
 | .../Objects/IEntityStore.cs                        |   2 +
 Components/XML/BExIS.Xml.Helpers/DatasetStore.cs   |  23 +++
 Console/BExIS.Web.Shell/Areas/BAM/Bam.Settings.xml |   2 +-
 | .../BAM/Controllers/PartyServiceController.cs      |  60 +++----
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 | .../PartyService/_userRegisterationPartial.cshtml  |   7 +
 Console/BExIS.Web.Shell/Areas/DCM/Dcm.Settings.xml |   2 +-
 | .../Areas/DCM/Helpers/DataASyncUploadHelper.cs     |  27 +++-
 | .../Areas/DCM/Helpers/EntityReferenceHelper.cs     |  73 ++++++---
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 | .../Areas/DDM/BExIS.Modules.Ddm.UI.csproj          |   1 +
 | .../Areas/DDM/Controllers/DataController.cs        |   4 +-
 | .../Areas/DDM/Controllers/HomeController.cs        |   2 +-
 | .../DDM/Controllers/PublicSearchController.cs      |   2 +-
 Console/BExIS.Web.Shell/Areas/DDM/Ddm.Settings.xml |   2 +-
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 | .../Areas/DDM/Views/Data/ShowData.cshtml           |   2 +-
 | .../DDM/Views/Data/_unstructuredDataView.cshtml    |   2 +-
 | .../Areas/DDM/Views/Shared/Error.cshtml            |  11 ++
 | .../Areas/DIM/Controllers/ExportController.cs      |   2 +-
 Console/BExIS.Web.Shell/Areas/DIM/Dim.Settings.xml |   2 +-
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 | .../Controllers/ShowMultimediaDataController.cs    |  14 +-
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 | .../Areas/RPM/Controllers/Legacy/UnitController.cs |   1 +
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 Console/BExIS.Web.Shell/Areas/RPM/Rpm.Settings.xml |   2 +-
 | .../SAM/Controllers/RequestsAdminController.cs     |   9 +-
 | .../Areas/SAM/Controllers/RequestsController.cs    |  59 ++++---
 | .../Areas/SAM/Models/DecisionModels.cs             |   4 +
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-
 Console/BExIS.Web.Shell/Areas/SAM/Sam.Settings.xml |   2 +-
 | .../SAM/Views/RequestsAdmin/_DecisionsAdmin.cshtml |   7 +-
 | .../PublishProfiles/FolderProfile.pubxml           |   2 +-

