# BEXIS 2.18 Release Notes
>**Database update**: [Update_Script_217to218.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/Update_Script_217to218.sql)

>**API changes**: Attention the api calls for **metadata**, **data** and **dataset** have been extended and changed with regard to versions. 
>in general, the calls work according to the following pattern <br>
>*api/{name}/{id}/{version}* <br>
>*api/{name}/{id}/version_number/{version_number}*<br>
>*api/{name}/{id}/version_name/{version_name}*<br>

**Workspace changes:** [2.17.1...2.18](https://github.com/BEXIS2/Workspace/compare/2.17.1...2.18)

### Features
- Deleted Datasets: Adjust info text, add reason and allow metadata view (https://github.com/BEXIS2/Core/issues/1099)
- Version API: Add (https://github.com/BEXIS2/Core/issues/1028)
- Export a single dataset to a **gbif** valid darwin core archive (v1)  (https://github.com/BEXIS2/Core/issues/917) - [how to setup?](https://github.com/BEXIS2/Documents/blob/master/Manuals/DIM/Manual.md#3-gbif)

### Enhancements
- Metadata API: Unify API calls and add version number and name (https://github.com/BEXIS2/Core/issues/1101)
- Metadata API: Update to get values in the structure of the concept (https://github.com/BEXIS2/Core/issues/1093)
- Search: Order items column-wise instead of row-wise when clicking on "more" in the facets (https://github.com/BEXIS2/Core/issues/1089)
- Darwin Core: Create Concept for DWC Metadata needed Attributes (https://github.com/BEXIS2/Core/issues/1092)
 
### Bugs
- Fix Requests are sent to old owner after change (https://github.com/BEXIS2/Core/issues/1108)
- Fix Metadata Edit: xsd element type "xs:boolean" always disabled (https://github.com/BEXIS2/Core/issues/1106)
- Fix Download data: Fails with filter if special characters in dataset title (https://github.com/BEXIS2/Core/issues/1105)
- Fix Tabular primary data display problem "Displaying items 0 - 0 of 0" (https://github.com/BEXIS2/Core/issues/1116)
- Fix Leading empty line inside each metadata input field (https://github.com/BEXIS2/Core/issues/1117)
- Darwin Core archive: Add documentation to dim manual enhancement (https://github.com/BEXIS2/Core/issues/1104)
- Code cleanup: Remove duplicate folder & consolidation of hamdi1992/core:master and bexis2/core:rc (https://github.com/BEXIS2/Core/issues/1107) (https://github.com/BEXIS2/Core/issues/1066)







