# BEXIS 2 Release Notes - Version 4.1.0
> This release comes with a big reduction in data type complexity by removing unused and redundant data types. Only the most commonly used data types are now available, which simplifies the data structure creation process. Additionally, this release includes several bug fixes and enhancements to improve the overall user experience. Further we introduced a new setting to control sending error emails based from the system.

### Workspace changes:
- Workspace changes: [4.0.2..4.1.0](https://github.com/BEXIS2/Workspace/compare/4.0.2..4.1.0)

### Database Update(s):
- Update script from version 4.0.2 to 4.1.0: [Update_Script_4.0.2_4.1.0.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/4.0.2-4.1.0.sql)

### New Settings:
- **Error Email System**: A new setting has been introduced to control the sending of error emails based on the system settings. This allows administrators to manage error notifications more effectively. ([here](https://demo.bexis2.uni-jena.de/home/docs/Configuration#e-mail))
- **Citation settings**: A new setting under data discovery enables the system to provide citation formats and displays for entities. ([here](https://demo.bexis2.uni-jena.de/home/docs/Configuration#citation))

# Bugfixes and enhancements
## Metadata
- **UI Rendering Issues**: Fix high-level schema elements rendered improperly([#2096](https://github.com/BEXIS2/Core/issues/2096))

- **UI Element Duplication**: Fix random duplication of elements. ([#2097](https://github.com/BEXIS2/Core/issues/2097))

- **UI Drop-down Menu**: Fix drop-down menu for integers rendering issue. ([#2103](https://github.com/BEXIS2/Core/issues/2103))

- **Metadata Save Issue**: Fix metadata not saveable anymore after update of system values. ([#2133](https://github.com/BEXIS2/Core/issues/2133))

- **Metadata Choices**: Fix choices with default value from XSD has selected 2 values at the same time. ([#2134](https://github.com/BEXIS2/Core/issues/2134))

- **Metadata Enum Elements**: Fix Enum-Elements with an attribute are not displayed as drop-down list. ([#1265](https://github.com/BEXIS2/Core/issues/1265))

- **Metadata Default Values**: Fix choices in metadata with default value in XSD has selected 2 values at the same time. ([#2135](https://github.com/BEXIS2/Core/issues/2135))

## General
- **Menu Layout**: Fix missing nowrap in new menu. ([#2109](https://github.com/BEXIS2/Core/issues/2109))

- **Footer Update**: Change footer. ([#2054](https://github.com/BEXIS2/Core/issues/2054))


- **Svelte Menu Enhancement**: Add hover effect and increase size. ([#2071](https://github.com/BEXIS2/Core/issues/2071))

## Dataset View
- **Permissions Tab**: Fix permissions tab is not visible. ([#2118](https://github.com/BEXIS2/Core/issues/2118))

- **Dataset Links**: Fix missing in the download package. ([#981](https://github.com/BEXIS2/Core/issues/981))

- **Version Access Rights**: Fix get latest version throws an error when you have no rights to see the latest not tagged version. ([#2119](https://github.com/BEXIS2/Core/issues/2119))
  
- **Title OR Citation within Dataset View**: Added a new option to the Dataset View Page that gives you control over what information is shown at the top. You can now choose between displaying either the Dataset Title or the full Citation String. ([#2140](https://github.com/BEXIS2/Core/issues/2140))

## Dataset Edit
- **Data Types Merging**: Merging of similar data types to simplify use. ([#2117](https://github.com/BEXIS2/Core/issues/2117))

- **Attachment Upload**: Fix error while adding file description during upload of attachments. ([#2143](https://github.com/BEXIS2/Core/issues/2143))

- **Upload Validation**: Add date pattern to error message. ([#2145](https://github.com/BEXIS2/Core/issues/2145))

- **Constraint Loading**: Fix long loading times for used constraints. ([#2110](https://github.com/BEXIS2/Core/issues/2110))

- **Tag View Error**: Fix loading tag view throws error when author and description is null. ([#2144](https://github.com/BEXIS2/Core/issues/2144))

## API & Export
- **API Enhancement**: Create API should return ID and status code instead of message. ([#2121](https://github.com/BEXIS2/Core/issues/2121))

- **Dataset API Enhancement**: Add entity links. ([#2132](https://github.com/BEXIS2/Core/issues/2132))

- **ORCID and ROR IDs**: Use of ORCID and ROR IDs in the user profile and in the DOI proxy. ([#2068](https://github.com/BEXIS2/Core/issues/2068))

- **JSON Field Mapping**: Fix impossible to obtain several fields mapped into one information block in JSON. ([#2138](https://github.com/BEXIS2/Core/issues/2138))


## Maintenance
- **Error Email System**: Send error emails based on settings. ([#2136](https://github.com/BEXIS2/Core/issues/2136))

- **SQL Script Creation**: Create a SQL script that adds tags to versions in an existing instance. ([#2120](https://github.com/BEXIS2/Core/issues/2120))

- **Update Script Extension**: Extend update script to link variable templates and meanings. ([#2111](https://github.com/BEXIS2/Core/issues/2111))

- **SAM Former Members**: Fix user not findable in former members group. ([#2112](https://github.com/BEXIS2/Core/issues/2112))

- **Restore deleted Datasets**: Add new feature to the Dataset Management page that allows you to easily restore datasets that were previously marked for deletion. ([#1734](https://github.com/BEXIS2/Core/issues/1734))

