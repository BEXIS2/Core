# BEXIS2 3.3.0 Release Notes

>..Darwin Core Export / DOI Integration .....

### Note:
Webconfig needs to be updated!

### Workspace changes:
- Workspace changes: [3.2.1..3.3.0](https://github.com/BEXIS2/Workspace/compare/3.2.1..3.3.0)

### Database Update(s):
- Update script version 3.2.1 to 3.3.0:
- [Update_Script_3.2.1_3.3.0.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/Update_Script_3.2.1_3.3.0.sql)

### New/Updated Settings
- General/Landing Page - (if empty, load landingpage.htm from tenant)
- Data Colletion/Use external metadata form (Enables the loading of an external metadata form when editing entities)
- Data Colletion/External metadata form destination url (Define the origin from where the external metadata form should be loaded)
- Data Dissemination/GBIF Export - Define a destination of all darwin core archives. 

## Changes

### Enhancements
- DOI Integration into BEXIS2 Core System ([#1567](https://github.com/BEXIS2/Core/issues/1567))
- Improve authorization for internal API calls ([#1730](https://github.com/BEXIS2/Core/issues/1730))
- Handling of JWT: Store JWT in the page component in bexis2-core-ui - sveltekit ([#1694](https://github.com/BEXIS2/Core/issues/1694))
- Security improvements: prevent web applications potentially vulnerable to clickjacking (443/tcp) & Add missing secure cookie attribute (HTTP) (443/tcp) ([#1708](https://github.com/BEXIS2/Core/issues/1708))([#1707](https://github.com/BEXIS2/Core/issues/1707))
- Add the possibility to use an external metadata form instead of the existing one #1715 ([#1715](https://github.com/BEXIS2/Core/issues/1715))
- Instead of Application Name, The breadcrumb from the old layout shows Home at first element #1720 ([#1720](https://github.com/BEXIS2/Core/issues/1720))
- Add Page Content to edit content pages in frontend #1693 ([#1693](https://github.com/BEXIS2/Core/issues/1693))
- RPM: handle missing value in domain constraints - import from dataset([#1692](https://github.com/BEXIS2/Core/issues/1692))
- Use the new “meaning” object to store linkage to the needed Darwin Core concepts for variables ([#1574](https://github.com/BEXIS2/Core/issues/1574))
  
### Bugfixes
- Fix after changing the data structure, the validation should be triggered again ([#1729](https://github.com/BEXIS2/Core/issues/1729))
- Fix performance problems with several svelte pages ([#1717](https://github.com/BEXIS2/Core/issues/1717))
- Fix edit data structure allows edit display pattern but can't store the changes ([#1709](https://github.com/BEXIS2/Core/issues/1709))
- Fix Telerik data table not working ([#1727](https://github.com/BEXIS2/Core/issues/1727))
- Fix edit variable template: save does not get activated ([#1711](https://github.com/BEXIS2/Core/issues/1711))
- Fix create new meaning not working ([#1714](https://github.com/BEXIS2/Core/issues/1714))
- Fix return to dataset after editing data structure not working ([#1728](https://github.com/BEXIS2/Core/issues/1728))
- Fix destroyed metadata form appears when show data or other tabs in show dataset view ([#1710](https://github.com/BEXIS2/Core/issues/1710))
- Fix Read a file with delimiter inside the text quotes is not working ([#1753](https://github.com/BEXIS2/Core/issues/1753))


