# BEXIS2 x.y Release Notes

>**Database Update(s)**: [Update_Script_218to300.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/Update_Script_218to300.sql](url)

>**API changes**:

## Tenant(s)

In general, the system does not provide different tenants anymore. Instead, the workspace contains only one tenants called 'bexis2'.

Additionally, to be more consistent, the configuration file (called 'manifest.xml') contains the new entry '<Brand>...</Brand>' which is used to enter the name of the image used within the menu bar. This is going to replace the wrong use of '<Logo>...</Logo>'. Please be aware of this update and react accordingly.

For most instances, the favicon was not loaded correctly, because the referenced name of the file within 'manifest.xml' was not correct. Please be aware of that information as well and check/change the name within 'manifest.xml' of the active tenant.
<!-- # BEXIS 2.18 Release Notes
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
- Export a single dataset to darwin core archive format (v1)  (https://github.com/BEXIS2/Core/issues/917) - [how to setup?](https://github.com/BEXIS2/Documents/blob/master/Manuals/DIM/Manual.md#3-gbif)

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
- Code cleanup: Remove duplicate folder & consolidation of hamdi1992/core:master and bexis2/core:rc (https://github.com/BEXIS2/Core/issues/1107) (https://github.com/BEXIS2/Core/issues/1066) -->







