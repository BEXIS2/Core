# BEXIS2 3.0.0-beta Release Notes

_**Please note that this is a beta version.** It is not recommended to use this version in production._

BEXIS2 version 3.0.0 is a major release. It contains a lot of changes in the background as well as in the UI. The most important changes are listed below.

From this release, semantic versioning is used. The version number is composed of three numbers: major.minor.patch. The major version number is increased if there are breaking changes. The minor version number is increased if new features are added. The patch version number is increased if bugs are fixed.

## Instructions for updating from 2.18.2 to 3.0.0-beta

### Workspace changes:
- all settings.xml are converted to JSON. Transfer your individual settings via the new Settings UI after the update of the code and workspace
- updated EntityReferenceConfig.Xml:
  - removed: IsCompiledBy & Compiles;
  - description revision (if used compile before you should add again)
- Workspace changes: [2.18.2..3.0.0-beta](https://github.com/BEXIS2/Workspace/compare/2.18.2...3.0.0-beta)


### Database Update(s):
- very important: **backup your database before you start the update**
- the underlying database structure has changed and extended. Data will be migrated, but a few SQL statements need to be adjusted
  - please create your **entity templates** and update them according to your datasets and entity template IDs after the update with the provided SQL

- [Update_Script_218to300.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/Update_Script_2182to3.sqls)


## Changes

### General Backend
- Update of the .net framework to 4.8, nhibernate 5.3, and other libraries
- Integration of the Vaiona library into the code
- Improvements and setting up of Jenkins pipeline for development and deployment
- Unit tests were added to most of the refactored backend functions
- Revision of initial seed data for units, dimensions and data types
- JWT support for authentication ([#1276](https://github.com/BEXIS2/Core/issues/1276))

### General Frontend
- Replacing Bootstrap by [TailwindCSS](https://tailwindcss.com/) and the UI framework [Skeleton](https://www.skeleton.dev/) for all refactored UI components
- Creation of BEXIS2 Core UI NPM Packages for centralized maintenance of certain frequently used UI components like forms and tables. (help, page, forms …) ([npm](https://www.npmjs.com/package/@bexis2/bexis2-core-ui), [BEXIS2 Core UI Website](https://bexis2.github.io/bexis2-core-ui/))
- Basic setup for E2E tests with playwright ([see here](https://github.com/BEXIS2/bexis2-core-ui-tests))
- Refactoring and enhancement of UI Elements related to the menu, settings, [entity templates](https://github.com/BEXIS2/Documents/blob/3.0.0-beta/Docu/Entity%20Templates.pdf), [data description](https://github.com/BEXIS2/Documents/blob/3.0.0-beta/Docu/Terms%20Variable%20Concept.pdf), and upload workflow have been refactored to Svelte. The remaining parts of the UI follow step by step.
- Replacing of the header – content dividing block by a breadcrumb
- Settings for modules to adjust, e.g., values or default text have a UI and can be changed without server access. This covers only settings which does not require a server restart.
- Introduction of show/hide-able components (e.g., attachments, links, permissions) via entity templates instead of a global setting. Permissions currently not shown, but will be added again in the future.
- Dashboard: Rename to "My Data" ([#333](https://github.com/BEXIS2/Core/issues/333))

### Bugfixes
- Fixed: Wrong metadata system value for id, if file deleted in a dataset ([#1346](https://github.com/BEXIS2/Core/issues/1346))


### Refactoring dataset creation, data description and upload
- Introduction of Entity Templates (see **Entity Templates**)
- Extended / changes Variable Concept (see **Variable Concept**)
- The different upload workflows have been combined to one. The user can freely decide which step he like to start. Create data structure or upload file. Validation is triggered automatically once the file or the data structure has changed.
- Increased level of detail for validation errors
- Data structures can be detected after uploading a file. The data type is analyzed using a random sample (value changeable in the settings). Descriptions can be read. Units can be read and will be mapped to existing units. Missing values can be added for each column.
- Variable Templates are now treated as templates and no longer actively linked and used once selected for a variable. Instead, all information is copied to a variable.
- Data structures are now optional. The unstructured type (for files) has been removed in favor of forcing information to be added in the metadata / not replicated.
- Excel upload is in the current version not possible, but it will re-introduced in the future.
- Excel Macros within the data structure are not supported anymore. The data structure is now a simple Excel file.
- The upload of files is now possible via drag and drop. The upload of multiple files is possible.
- Created datasets are currently not added to the search index, before the upload is finished or metadata has been edited. This might change in the future back to the old behavior.

### DOI
- DOI registration at DataCite is now possible via the UI. DOI support is only available for instances of having an account at DataCite.
