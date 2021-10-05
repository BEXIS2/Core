# BEXIS 2.14.5 Release Notes

Attention: Changes were mad in the web.config in line: 72 (see [#803](https://github.com/BEXIS2/Core/issues/803))

### Features


### Enhancements
- Data policy checkbox removed from registration (does not need to be agreed on) ([#784](https://github.com/BEXIS2/Core/issues/784))
- Requests and Decissions: Date column sortable ([#781](https://github.com/BEXIS2/Core/issues/781))
- Replace "Show Data/Dataset" by the current entity name (e.g. Publication) ([#769](https://github.com/BEXIS2/Core/issues/769))
- Sort users descending by id (manage parties)([#806](https://github.com/BEXIS2/Core/issues/806))
- Sort data structures descending by id ([#807](https://github.com/BEXIS2/Core/issues/807))
  
### Bugs
- Fix only a maximum of 100 variables possible inside a data structure (max size of JSON objects too small) ([#803](https://github.com/BEXIS2/Core/issues/803))
- Fix files with longer file names not possible for upload (URI field to short for contentdescriptor) ([#800](https://github.com/BEXIS2/Core/issues/800))
- Fix appliaction name missing in header on Log In page ([#786](https://github.com/BEXIS2/Core/issues/786))
- Fix user names during registration should not allow ending spaces ([#786](https://github.com/BEXIS2/Core/issues/786))
- Fix missing white space on primary data page ([#780](https://github.com/BEXIS2/Core/issues/780))
- Fix missing "Terms and Conditions" link in footer ([#785](https://github.com/BEXIS2/Core/issues/785))
- Fix error during data validation (constraints not checked correctly) ([#792](https://github.com/BEXIS2/Core/issues/792))([#804](https://github.com/BEXIS2/Core/issues/804))
- Fix data structure was not anymore editable after import ([#791](https://github.com/BEXIS2/Core/issues/791))
- Fix missing checkedIn after rollback during data upload ([#805](https://github.com/BEXIS2/Core/issues/805))
- Fix missing values sometimes disappaer after save ([#774](https://github.com/BEXIS2/Core/issues/774))
- Fix download metadata only returned latest version ([#771](https://github.com/BEXIS2/Core/issues/771))
- Fix image view can not be closed (unstructured data) ([#797](https://github.com/BEXIS2/Core/issues/797))
- Fix primary data of unstructured datasets are not visible without rights in public data view ([#799](https://github.com/BEXIS2/Core/issues/799))

## I. Software Information

-	Name: BEXIS
-	Version: 2.14.5
-	Application Type: Web Application
-	Platform: Windows

## II.	License Agreement

> The contents included in this software are licensed to you, the end user. Your use of BEXIS software is subject to the terms of an End User License Agreement (EULA) accompanying the software and located in the \License subdirectory. You must read and accept the terms of the EULA before you access or use BEXIS software. If you do not agree to the terms of the EULA, you are not authorized to use BEXIS software.

## III. System Requirements

- Operating System: Windows Server 2008 or upper. For personal or small installations, the software is able to run on Windows 10, too
- Application Server: IIS 7.0 +. For IIS settings see the installation manual
- DBMS:
	- PostgreSQL: (version 10 - 13) 32 or 64 bits. BEXIS has not been tested on other versions!
- Dependencies
	- Microsoft.NET Framework 4.5.2
	- Microsoft.ASP.NET.MVC 5.2.3
	- Microsoft.ASP.NET.Razor.de 3.2.3
	- Microsoft.ASP.NET.WebHelpers 3.2.3
	- Microsoft.ASP.NET.WebPages.WebData 3.2.3
	- Microsoft.JQuery.Unobtrusive.Validation 3.0.0
	- Lucene.Net 3.0.3
	- Lucene.Net Contrib 3.0.3
	- NHibernate 4.0.4.4000
	- Telerik MVC Extensions 2013.1.611
	- Unity Application Block 3.5.1404
	- Npgsql 2.2.5
	- PostSharp 4.3.19
	- Linq 1.0.4
	- bootstrap 3.4.1
	- bootstrap-slider 5.0.13
	- FluentBootstrap 3.3.5.1
	- FluentBootstrap.Mvc 3.3.5.1
	- font-awesome 4.4.0
	- MediaInfoDotNet.0.7.79.40925
	- Swashbuckle.5.6.0
- Disk Space:
	- The Software: ~300 MB
	- The Workspace: 10 MB and more, based on the number of modules and the amount of data


## Installation

> To install the software, please follow the instructions in the [BEXIS213 Installation Manual](https://github.com/BEXIS2/Documents/blob/2.13/Guides/Installation/installation.md).

## System Functionality

> The software consists of two main types of components; 1) core components that include Data Lifecycle Management (DLM), Web Interfaces, Security and some other internally used ones. 2) A collection of coherent features and use-cases bundled as modules so that each module is designed around one or a small set of steps of the DLM workflow i.e., Data Submission, Quality Control/ Assurance, Data Publication and so on.

### General Application Features

General Application features are available in the entire application and across all modules.

| Existing Features |
|--|
| Online Help for each module available within the application (version 2.7.0)
| Upon registration users need to agree to terms and conditions (placeholder) (version 2.9.0)
| Notification framework by email (version 2.11.0)
| Sync on individual datasets can be triggered manually through Maintain Dataset (version 2.11.0)
| request access to a dataset; notification to owners by email; manage requests and decisions at the Dashboard. Issue #31
| API´s are secured and must be authorized and authenticated. [Issue #200]
| API Documentation in the application [Issue #202]
| The Delete and Purge functions are logged and an email is sent to the system email (2.12.3)
| Hide menu entries without permission ([#143](https://github.com/BEXIS2/Core/issues/143)) (version 2.13)
| API to read and write data ([#250](https://github.com/BEXIS2/Core/issues/250)) and metadata ([#262](https://github.com/BEXIS2/Core/issues/262)) and a new API for Datasets [#260](https://github.com/BEXIS2/Core/issues/260) and Attachments [#261](https://github.com/BEXIS2/Core/issues/261). (version 2.13)
|Admin view to manage requests; Withdraw requests; Changed: Email notification (e.g. send also CC to Sysadmin); ([#532](https://github.com/BEXIS2/Core/issues/532)), ([#582](https://github.com/BEXIS2/Core/issues/582))) (version 2.14)



|Known Issues
|--|
|Some functions of the application do not work properly in Internet Explorer (e.g. download data structure). We recommend using Google Chrome or Mozilla Firefox.

## Data Discovery Module

>The main purpose of this module is to enable users to search for data available within the system. The module executes user defined queries against the metadata and primary data and provides advanced features such as facets, keywords, categories, filters, grouping, and sorting. Depending on individual user permissions the system provides access to full metadata records, the underlying data structure and the primary data.

|Existing Features|
|--|
|Discover data using metadata dimensions (aspects, facets) and attributes (version 2.0.0)
|Configure search module through a user interface (version 2.1.0)
|Primary data search (version 2.4.0)
| mapping Lucene fields to multiple Metadata Nodes in the Search Component Manager (version 2.5.0)
| The homepage contains a list of accessible dataset (version 2.6.0)
< Edit metadata of an existing dataset from the Show Details page (version 2.8.0)
Metadata view was not available for non-authenticated users (public datasets)  (version 2.8.0)
| some metadata entries in complex metadata schemas were not shown correctly (version 2.8.0)
| the search index is now re-created automatically whenever a new dataset is created or deleted (version 2.9.0)
| Bundle a dataset (version 2.10.0)
| Public Search showing only public datasets to anonymous users. The regular Search contains a separate filter now for public datasets. issue #33
| Metadata download directly from the details view as html & xml (2.12.3)
| Allow to switch between dataset versions and show version information ([#283](https://github.com/BEXIS2/Core/issues/283),[#306](https://github.com/BEXIS2/Core/issues/306)) (version 2.13)
| Integration of Multimedia Module ([#281](https://github.com/BEXIS2/Core/issues/281))(version 2.13)
| Dataset view: Allow to hide tabs via settings (hide and hide on missing permission) ([#455](https://github.com/BEXIS2/Core/issues/455))



|Known Issues
|--|

## Data Collection Module
>The purpose of the data collection module is to allow users to submit data alongside with metadata. The module processes the data in accordance with its data/metadata structure rules to find any inconsistencies and communicates with the user to overcome the issues. The module may also notify or trigger other internal services such as the search indexing engine to make the dataset ready for discovery. Based on the settings of the associated research context/ plan, datasets get versioned.

|Existing Features
|--|
|Uploading an Excel file (version 2.1.0)
|Uploading a delimited file (CSV, TSV), (version 2.2.0)
|Adding data to an existing dataset (version 2.2.0)
|Create Structured Dataset (version 2.3.0)
|Create Unstructured Dataset (version 2.4.0)
|Push big files to server (version 2.4.0)
|Importing metadata schemas via XSD files (version 2.6.0)
|The Create Dataset Wizard has been changed to cope with varying metadata structures. (version 2.6.0)
|using a dataset as a template for a new dataset (version 2.9.0)
|import of existing metadata of the same metadata structure from an XML file (version 2.9.0)
|Selecting a data/metadata structure in dataset creation work now also with large quantities of these entities
| Add metadata structure management (version 2.10.0)
| Data Import wizard for MS Excel files, incl. creation of data structure extracted from the file (version 2.11.0)
|Metadata Mapping tool to create mappings between metadata schemas and party package elements (version 2.11.0) |Import date in German format (version 2.11.3)
| Dataset attachments for any additional information (e.g. images, protocols, descriptions)
| Import Data feature updated to improve user guidance on linking variables of the new dataset to existing variables in the system.
| Allow deleting unstructured primary data ([#381](https://github.com/BEXIS2/Core/issues/381)) (version 2.13)
| Allow to link datasets or other entities (e.g. publications) via metadata entry or external link ([#193](https://github.com/BEXIS2/Core/issues/193)). Entity white list ([#374](https://github.com/BEXIS2/Core/issues/374)) to exclude entities + Description for link types ([#372](https://github.com/BEXIS2/Core/issues/372)) (version 2.13)
| Asynchronous data upload  ([#510](https://github.com/BEXIS2/Core/issues/510))

|Known Issues
|--|
| Empty rows within or at the end of Excel or CSV files lead to validation errors and should be removed (by the user) before adding (i.e. uploading) data to a dataset.
| Boolean Values in Excel, should be true or false, 0 1, TRUE FALSE. 
| Excel Files, generated from libre office, currently not supported

## Data Planing Module

> The purpose of the data planning module is to manage (create, edit, delete) data and metadata structures, data attributes (variables), units, data types and their relations. The aim is to foster reuse and data integration while retaining flexibility.

A conceptual model showing the connection and relationship between the different terms and entities is available at: http://fusion.cs.uni-jena.de/bppCM/index.htm


|Existing Features
|--|
| Data structure manager (version 2.1.0)
| Data attribute manager (version 2.1.0)
| Data type manager (version 2.1.0)
| Unit manager (version 2.1.0)
| Unstructured data structures  (version 2.4.0)
| Create copy of a data structure (version 2.5.1)
| Creating units without a data type assigned to it is no longer possible (version 2.6.0)
| In data type creation, more information is provided on available system types (version 2.6.0)
| In-line editing for name and description of a data structure (version 2.6.0)
| Data Attributes and Variables can now have constraints (e.g. list of domain values, data ranges, and patterns as regular expressions)(version 2.7.0)
| Units can now contain a dimension specification (e.g. Length, Mass, Time) )(version 2.7.0)
| Variables can have individual units of the same dimension (e.g. mm, cm, m, km) independent from their corresponding Data Attribute (version 2.7.0)
| The header in downloaded Excel Templates is now locked to avoid accidental editing, which causes errors in the upload process later on (version 2.7.0)
| In the Data Attribute Manager constraints could not be deleted (version 2.8.0)
| After deleting a dataset, its data structure refers to the dataset id=0 (version 2.8.0)
| Patterns of various date/time formats on data types (version 2.9.0)
| Redesign data structure manager (version 2.10.0)
| appending rows to an existing tabular dataset available as an alternative path to the “update” approach.
| Uploading is now possible with XLSX, CSV, TSV files (previously XLSM and TXT only)
| Allow the definition of missing values ([#35](https://github.com/BEXIS2/Core/issues/35)) (version 2.13)

|Changed/ Enhanced Features
|--|
| NA

|Known Issues
|--|
- If macros are disabled in Excel, the downloaded Excel template is not being validated in Excel. This also leads to the issue that only the first row is being imported when submitting a dataset to BEXIS using such a template.

## Data Dissemination Module
> The main purpose of this module is to provide users with access to primary data and metadata managed by the system. In the future this module shall also provide support for publishing, orchestrating and harvesting data of the system by external resources.

|Existing Features
|--|
|Primary data download as Microsoft Excel file (version 2.2.0)
|Primary data download as comma or tab separated file (CSV) (version 2.4.0)
|Download of unstructured data (single file, multiple files in ZIP) (version 2.4.0)
|Export of metadata from an internal representation into XML files (version 2.5.0)
|Prepare Dataset package for publication with GFBio (version 2.11.0)
publication workflow from BEXIS 2 to Pensoft data journals (version 2.11.0)
extended Zip download available incl. separate files for metadata, primary data, data structure, etc.
|In the Primary data view filtering, sorting, and selecting is no available again allowing users to download custom subsets of a dataset (i.e.selected rows/columns).
| Allow to map system data (e.g. version, last modification date) in metadata form (automatically filled; not editable for user) ([#192](https://github.com/BEXIS2/Core/issues/192)) (version 2.13)


|Known Issues
|--|
| Open a downloaded Excel file in Libre Office does not display values correctly

## System Administration Module (former name: Security Module)
> The System Administration Module (SAM) provides functions for dataset maintenance, system configuration and authentication and authorization management. It is typically only accessible to system administrators or data managers.

|Existing Features
|--|
|User self-registration (version 2.1.0)
|Users and roles management for an administrator (version 2.1.0)
|Feature security (version 2.2.0)
|Dataset Security (version 2.4.0)
|Sign-On with existing credentials from an external authentication provider (LDAP Service) (version 2.5.0)
|System features and datasets can be made publicly accessible without authentication (version 2.6.0)
|passwords can be changed by users or administrators (version 2.6.0)
|administrators are able to approve, block, or unlock users (version 2.6.0)
| Sessions are properly timed out and users are logged off after a given time and informed (version 2.6.1)
|Inheritance of feature permissions for groups und users has been changed slightly to be more intuitive. (version 2.7.0)
|Internal refactoring: the security module is now part of the System 
| Administration Module. (version 2.7.0)
| Delete/Purge Datasets (version 2.7.0)
| Dataset permissions are not taken into account when deleting/purging from the dataset maintenance area (version 2.8.0)
| assigning groups to users (and vice versa) did not work across multiple pages (paging) (version 2.8.0)
| when a user logged in through single sign on (LDAP) and tried to edit his local BEXIS profile (My Account) an error occurred
| Users can set permissions on their own datasets in the dataset view.
| upon registration new users will by default get permissions to use the dashboard and the search
| Variable template management
| Unit management
| Data type management
| Modularization framework to un-/install and de-/activate modules in a running instance of BEXIS2 (version 2.11.0)
| switch to ASP.NET Core Identity 3.0 for more interoperability (version 2.11.0)
| Notification about Create/Update/Delete operations on datasets (version 2.11.0)
| extensions of tenants’ properties (version 2.11.0)
| confirmation email upon user registration (version 2.11.0)
| Dataset permissions are part of entity permissions (version 2.11.0)
| Permission settings in the Show data view are separate from the general feature permission settings (version 2.11.0)
| Support Anonymous SMTP (version 2.11.3)
| Documents management through the UI (e.g. guidelines, policies)
|  Allow email address in addition to account name for login ([#402](https://github.com/BEXIS2/Core/issues/402)) (version 2.13)


|Known Issues
|--|

## Business Administration Module (Party Package) 
 
> This module is to manage (Create, Update, and Delete) Information about organizations, projects, people, etc. and their relationships in central place and make it available for re-use throughout the system. 
 
|Existing Features 
|--|
| Import an XML file at instantiation time containing party types and relation types (version 2.11.0)
| Create/Edit/delete instances of parties and relations through the UI (version 2.11.0)
Dataset owner concept implemented . Issue #29, issue #40

Known Issues
|--|

## Visualization Module 
 
> This module provides visualization for the BEXIS2. Histograms represent data such as system activity or creating a dataset over time to make data more comprehensible for a user/administrator.
 
|Existing Features 
|--|
| Category selector, time selectors and time slider (version 2.12.0)
| Histograms to visualize system activities and created/deleted datasets over time (version 2.12.0)

Known Issues
|--|


# Contact

## Help desk:

Website:	http://bexis2.uni-jena.de
Email address: bexis-support@uni-jena.de
-	Telephone:	+49-(0)3641-948968
-	Fax:	+49-(0)3641-946302
