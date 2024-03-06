# BEXIS2 3.1.0 Release Notes

This release focused on fixing found bugs or already identified necessary changes in version 3.0.0-beta. Thanks to all testing version 3.0.0-beta!

## Instructions for updating from 3.0.0-beta to 3.0.1
- DWC terms via RPM ...

### Workspace changes:
- ???
- Workspace changes: [2.18.2..3.0.0-beta](https://github.com/BEXIS2/Workspace/compare/3.0.0-beta..3.0.0)
- ??? credential file

### Database Update(s):
- the update script from version 2.18 to 3.0.0-beta has been revised, and errors were fixed.
- [Update_Script_218to300.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/Update_Script_2182to3.sql)
- Update script version 3.0.0-beta to 3.1.0: ????
### New Settings:
- Data Structure - Enforce Primary Key (bool): To set a primary key during the creation of a data structure is/is not mandatory.
- Data Structure - Primary Key changeable (bool): Once data is uploaded, it is/is not possible to change the primary key. The new key is only saved if a validation is passed against all linked datasets.

## Changes

### Enhancements
- Add help on toggle and code editor ([#1516](https://github.com/BEXIS2/Core/issues/1516))
- Revise help texts ([#1486](https://github.com/BEXIS2/Core/issues/1486))
- Add display name to settings ([#1317](https://github.com/BEXIS2/Core/issues/1317))
- Preparations to replace primary data Telerik Table ([#747](https://github.com/BEXIS2/Core/issues/747))
- Support default values for elements and attributes during XSD import ([#1262](https://github.com/BEXIS2/Core/issues/1262))
- support XSD attributes during metadata export ([#1071](https://github.com/BEXIS2/Core/issues/1071))
- Move SMTP and LADP configuration inside general.settings.json ([#1551](https://github.com/BEXIS2/Core/issues/1551), [#796](https://github.com/BEXIS2/Core/issues/796))
- Reorder table header for Entity Links ([#1565](https://github.com/BEXIS2/Core/issues/1565))
- Create data structure: filter should show the exact match first ([#1541](https://github.com/BEXIS2/Core/issues/1541))
- Check and allow units from templates over associated dimensions instead only of the unit itself ([#1544](https://github.com/BEXIS2/Core/issues/1544))
- Add text to icon for toggle between view & edit ([#1426](https://github.com/BEXIS2/Core/issues/1426))
- Adjust UX writing under and for "Data Description" ([#1561](https://github.com/BEXIS2/Core/issues/1561))
- Change type "link" to "vocabulary" in DWC seed data ([#1584](https://github.com/BEXIS2/Core/issues/1584))
- Improvesubmit in edit dataset ([#1415](https://github.com/BEXIS2/Core/issues/1415))
- DB Update: Create meanings based on converted variable templates from existing instances ([#1538](https://github.com/BEXIS2/Core/issues/1538))
- Data Upload: Check the primary key over all uploaded files ([#1283](https://github.com/BEXIS2/Core/issues/1283))
- Add DWC terms as seed data as meanings ([#1510](https://github.com/BEXIS2/Core/issues/1510))
- Add help within create & edit data structure ([#1490](https://github.com/BEXIS2/Core/issues/1490))
- Update constraints with warning ([#1420](https://github.com/BEXIS2/Core/issues/1420))
- File upload: Add description to file and show under view ([#1570](https://github.com/BEXIS2/Core/issues/1570))
- Several small improvements ([#1545](https://github.com/BEXIS2/Core/issues/1545))
- Entity templates: Extend setting defaults permission by other options ([#1575](https://github.com/BEXIS2/Core/issues/1575))
- Detect and handle different encodings of uploaded files ([#1617](https://github.com/BEXIS2/Core/issues/1617))

  
### Bugfixes
- Fix missing measurement system validation when creating and editing a unit Type ([#1598](https://github.com/BEXIS2/Core/issues/1598))
- Fix missing entity ref in datasets or other entity instance dependencies([#1600](https://github.com/BEXIS2/Core/issues/1600))
- Fix load metadata failed when loading different datasets with different metadata structures ([#1597](https://github.com/BEXIS2/Core/issues/1597))
- Fix search index failed when the template of variable is null Type ([#1591](https://github.com/BEXIS2/Core/issues/1591))
- Fix several download problems ([#1587](https://github.com/BEXIS2/Core/issues/1587))
- Fix metadata attributes of container elements that don`t appear in the metadata form ([#1263](https://github.com/BEXIS2/Core/issues/1263))
- Fix if the data structure is created from empty, the meanings are not stored ([#1580](https://github.com/BEXIS2/Core/issues/1580))
- Fix required fields in the entity template are empty if no mapping to system keys exists at all ([#1558](https://github.com/BEXIS2/Core/issues/1558))
- Fix token link is broken in Svelte layout ([#1520](https://github.com/BEXIS2/Core/issues/1520))
- Fix update issues: Metadata can not be edited ([#1557](https://github.com/BEXIS2/Core/issues/1557))
- Fix metadata export for new imported metadata structure are not working([#1572](https://github.com/BEXIS2/Core/issues/1572))
- Fix create page has a dependency on EntityTemplate management ([#1577](https://github.com/BEXIS2/Core/issues/1577))
- Fix send request is not available ([#1576](https://github.com/BEXIS2/Core/issues/1576))
- Fix Error on API data out when the header is used and the first column/variable is not of type string ([#1546](https://github.com/BEXIS2/Core/issues/1546))
- Fix data API: token invalid ([#1562](https://github.com/BEXIS2/Core/issues/1562))
- Fix API upload doesn't update system variables in metadata ([#1391](https://github.com/BEXIS2/Core/issues/1391))
- Fix filter position for search is not correct ([#1559](https://github.com/BEXIS2/Core/issues/1559))
- Fix manual metadata and data is not linked ([#1540](https://github.com/BEXIS2/Core/issues/1540))
- Fix Swagger API view is not working ([#1536](https://github.com/BEXIS2/Core/issues/1536))
- Fix the name of the version view model (VersionsModel -> ReadVersionsModel) ([#1530](https://github.com/BEXIS2/Core/issues/1530))







