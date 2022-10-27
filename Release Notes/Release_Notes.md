# BEXIS 2.16 Release Notes
>Help URLs are now generated automatically based on the used relaese version. It is not needed from now on to always update the links in the workspace. Empty values in the settings file (default -> generated links) can be still overwritten with your own customized help links.

>For developer: API documentation is now generated automatically. Files need to be copied during build (debug & relaese) into the App_Data. Example changes can be found e.g. in the BExIS.Modules.Dim.UI.csproj file. At line 45 and changes in line 379 ([#973](https://github.com/BEXIS2/Core/issues/973))

### Features
- Metadata Statistic API: Draft version added supporting query by xpath and filtering by metadata structue and dataset ids ([#979](https://github.com/BEXIS2/Core/issues/979))([#986](https://github.com/BEXIS2/Core/issues/986))([#991](https://github.com/BEXIS2/Core/issues/991))
  
### Enhancements
- Metadata Structure API: Add entity type ([#987](https://github.com/BEXIS2/Core/issues/971))
- Primaray Data API: Improve error messages ([#858](https://github.com/BEXIS2/Core/issues/858))
- Data Structure API: Extend by missing values and date pattern ([#974](https://github.com/BEXIS2/Core/issues/974))
- Data Statistic API: Improve documentation ([#975](https://github.com/BEXIS2/Core/issues/975))
- Former Member: Allow for dynamic email content via settings ([#976](https://github.com/BEXIS2/Core/issues/976))
- Improve API documentation and automation of manual steps ([#973](https://github.com/BEXIS2/Core/issues/973))
- Improve automation of build process ([#964](https://github.com/BEXIS2/Core/issues/964)
- Add further publishing profiles ([#972](https://github.com/BEXIS2/Core/issues/972)


### Bugs
- Fix spaces in names during mapping cause JavaScript error ([#2651](https://github.com/BEXIS2/Core/issues/265))
- Fix no dataset download, if no primary data was uploaded ([#562](https://github.com/BEXIS2/Core/issues/562))
- Fix metadata import via API works only with metadata structure id 1 ([#977](https://github.com/BEXIS2/Core/issues/977))
- Fix metadata status not allways copied correct in a new version (e.g., during data import via API) ([#977](https://github.com/BEXIS2/Core/issues/977))
