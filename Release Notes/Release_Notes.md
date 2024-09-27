> This version contains some new features and a lot of smaller enhancements. Once the new features are stable, we plan to release version 4.0.0.

<b>Important:</b> web.config.samples and the workspace contain important changes.

### Workspace changes:
- Workspace changes: [3.3.3..3.4.0](https://github.com/BEXIS2/Workspace/compare/3.3.3..3.4.0)

### Database Update(s):
- Update script from version 3.3.3 to 3.4.0: [Update_Script_3.3.3_3.4.0.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/3.3.3-3.4.0.sql)

### New Settings
- General
  - <b>landingPageForUsers</b> update link to new search
- Data Discovery
  - <b>use_tags</b> : Enable users to create tags as a combination of dataset versions.
  - <b>use_minor</b> : Activate minor tags to show the changes more granularly (x.x).


### Features
- Dataset versioning: Creation, View, and Editing. Call via tag (url & API) [#1647](https://github.com/BEXIS2/Core/issues/#1647)
- Bioschema.org integration: Add as a concept for mapping and to the metadata [#511](https://github.com/BEXIS2/Core/issues/511)
- Refactored Search UI to Svelte (as it was before) [#1801](https://github.com/BEXIS2/Core/issues/1801)
- DOI integration into BEXIS2 Core System [#1567](https://github.com/BEXIS2/Core/issues/1567)


### Enhancements
- Hide empty field groups in Metadata View [#1807](https://github.com/BEXIS2/Core/issues/1807)
- Change and edit missing values during editing and creation of a data structure [#1779](https://github.com/BEXIS2/Core/issues/1779)
- Add centralized BEXIS2 theme CSS generation [#1790](https://github.com/BEXIS2/Core/issues/1790)
- Variable edit in data structure: Change group names from value to category [#1787](https://github.com/BEXIS2/Core/issues/1787)
- Bigger refactoring of the variable edit in the data structure [#1792](ttps://github.com/BEXIS2/Core/issues/1792)
- UI improvements File Reader Information & Spell checking [#1855](https://github.com/BEXIS2/Core/issues/1855)
- Change the order of the data structure and file upload in the metadata edit [#1859](https://github.com/BEXIS2/Core/issues/1859)
- Add variable count to data structure error message [#1777](https://github.com/BEXIS2/Core/issues/1777)
- Remove "in use" info from user view [#1782](https://github.com/BEXIS2/Core/issues/1782)

### Maintenance & Security
- Add API error messages as notifications to page component for development [#91](https://github.com/BEXIS2/bexis2-core-ui/issues/91)
- Fix problems with build solution after new installation of BEXIS 2 [#1791](https://github.com/BEXIS2/Core/issues/1791)
- Reduction of used NuGet packages [#1794](https://github.com/BEXIS2/Core/issues/1794)
- Robots.txt: change default settings [#735](https://github.com/BEXIS2/Core/issues/)
- Prevent Web Application Potentially Vulnerable to Clickjacking [#1853](https://github.com/BEXIS2/Core/issues/1853)
- Prevent Web Server Allows Password Auto-Completion [#1861](https://github.com/BEXIS2/Core/issues/1861)


### Bugfixes
- Fix data structure (date pattern) changes are not detected [#1858](https://github.com/BEXIS2/Core/issues/1858)
- Fix name of a person (party type) switches after edit [#1484](https://github.com/BEXIS2/Core/issues/1484)
- Fix Log Off does not work in Svelte Layout [#1518](https://github.com/BEXIS2/Core/issues/1518)
- Fix file reader information: Selection without description does not work [#1856](https://github.com/BEXIS2/Core/issues/1856)
- Fix disable Submit after submitting does not work for small files [#1857](https://github.com/BEXIS2/Core/issues/1857)
- Fix data structure (date pattern & missing values) changes are not detected [#1858](https://github.com/BEXIS2/Core/issues/1858)
- Fix data type Decimal does not allow negative values [#1866](https://github.com/BEXIS2/Core/issues/1866)
- Fix wrong title for delete Variable & Dimension [#1868](https://github.com/BEXIS2/Core/issues/1868)