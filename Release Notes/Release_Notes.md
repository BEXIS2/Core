# BEXIS 2.18 Release Notes
>Database update: [Update_Script_217to2171.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/Update_Script_217to2171.sql)

**Workspace changes:** [2.17.1...2.18](https://github.com/BEXIS2/Workspace/compare/2.17.1...2.18)

### Features
- Deleted Datasets: Adjust info text, add reason and allow metadata view ([#1099])(https://github.com/BEXIS2/Core/issues/1099)
- Version API: Add ([#1028])(https://github.com/BEXIS2/Core/issues/1028)
-
### Enhancements
- Metadata API: Unify API calls and add version number and name ([#1101])(https://github.com/BEXIS2/Core/issues/1101)
- Metadata API: Update to get values in the structure of the concept ([#1093])(https://github.com/BEXIS2/Core/issues/1093)
- Search: Order items column-wise instead of row-wise when clicking on "more" in the facets ([#1089])(https://github.com/BEXIS2/Core/issues/1089)
- Darwin Core: Create Concept for DWC Metadata needed Attributes ([#1092])(https://github.com/BEXIS2/Core/issues/1092)
 
### Bugs
- Fix Requests are sent to old owner after change ([#1108])(https://github.com/BEXIS2/Core/issues/1108)
- Fix Metadata Edit: xsd element type "xs:boolean" always disabled ([#1106])(https://github.com/BEXIS2/Core/issues/1106)
- Fix Download data: Fails with filter if special characters in dataset title ([#1105])(https://github.com/BEXIS2/Core/issues/1105)
- Darwin Core archive: Add documentation to dim manual enhancement ([#114])(https://github.com/BEXIS2/Core/issues/1104)
- Code cleanup: Remove duplicate folder & consolidation of hamdi1992/core:master and bexis2/core:rc ([#1107])(https://github.com/BEXIS2/Core/issues/1107)([#1066])(https://github.com/BEXIS2/Core/issues/1066)







