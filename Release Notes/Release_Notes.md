# BEXIS2 3.2.0 Release Notes

This release focused on updating UI libraries, fixing bugs, adding new constraint features, and adding the first version of the new primary data for testing.

### Workspace changes:
- Workspace changes: [3.1.0..3.2.0](https://github.com/BEXIS2/Workspace/compare/3.1.0..3.2.0)
- Please make sure that the credentials file in the workspace is no longer needed (since 3.1.0). Instead, the SMTP settings are entered in the general settings.json. Please enter your credentials there.

### Database Update(s):
- Update script version 3.1.0 to 3.2.0:
- [Update_Script_3.1.0beta_3.2.0.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/Update_Script_3.1.0beta_3.2.0.sql)

### Important notes:
- Data Out API: Previously missing values for dates have not been replaced, while all other types have been replaced.


## Changes
### Enhancements
- SvelteKit Update to version 2.x #1528 ([#1528](https://github.com/BEXIS2/Core/issues/1528))
- First version to replace primary data Telerik table and add a link to My Data view #747 #1665 ([#747](https://github.com/BEXIS2/Core/issues/747))([#1665](https://github.com/BEXIS2/Core/issues/1665))
- Create a domain list based on an internal dataset #1421 ([#1421](https://github.com/BEXIS2/Core/issues/1421))
- Create Dataset: Add copy a dataset (was missing after refactoring in version 3) #1634 ([#1634](https://github.com/BEXIS2/Core/issues/1634))
- Add new DateTime Pattern (yyyy-MM-dd hh:mm: ss/dd.MM.yyyy hh:mm:ss) #1677 ([#1677](https://github.com/BEXIS2/Core/issues/1677))

### Bugfixes
- Fix file reader data preview view is not scroll-able #1637 ([#1637](https://github.com/BEXIS2/Core/issues/1637))
- Fix different background colors for menu #1654 ([#1654](https://github.com/BEXIS2/Core/issues/1654))
- Fix missing values for dates are not replaced in the primary data view (within the new primary data view) #641 ([#641](https://github.com/BEXIS2/Core/issues/641))
- Fix date display patterns are not replaced in Data Out API #1549 ([#1549](https://github.com/BEXIS2/Core/issues/1549))
- Fix Structure API - Error when a variable has no variable template #1664 ([#1664](https://github.com/BEXIS2/Core/issues/1664))
