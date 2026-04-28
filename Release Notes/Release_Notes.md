
# BEXIS2 Release Notes - Version 4.3.0
> This release contains security updates and a very long list of enhancements and bugfixes across BEXIS2.

### Workspace changes:
- Workspace changes: [4.2.1..4.3.0](https://github.com/BEXIS2/Workspace/compare/4.2.1..4.3.0)


### Database Update(s):
- Update script from version 4.2.1 to 4.3.0: [Update_Script_4.2.1_4.3.0.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/4.2.1-4.3.0.sql)

### Web.config changes
- ...

### Configuration
- General Settings: Additional Help Links
- General Settings: Security Settings


# Bugfixes and enhancements


### Maintenance
#### DOI
- **Vaelastrasz.Library**: Update from 6.1.7 to 6.2.2 ([#2429](https://github.com/BEXIS2/Core/issues/2429))
- **DOI Service**: Update doi.bexis2.uni-jena.de to run v6.2.2 ([#2430](https://github.com/BEXIS2/Core/issues/2430))
#### Security
- **Identity Framework**: Modify usage to follow best practice and prepare display name retrieval ([#2411](https://github.com/BEXIS2/Core/issues/2411))
- **Identity Framework**: Dependency injection of identity-related classes (user manager, group manager, sign-in manager, etc.) ([#2412](https://github.com/BEXIS2/Core/issues/2412))

### Bugfixes and Enhancements
#### Access and navigation
- **Help Menu**: Refactor and move to general settings ([#2299](https://github.com/BEXIS2/Core/issues/2299))
- **Help Menu Entries**: Not visible on non-svelte pages ([#2397](https://github.com/BEXIS2/Core/issues/2397))
- **Main Menu**: Show display name instead of user name ([#2267](https://github.com/BEXIS2/Core/issues/2267))

#### Settings and versions
- **Settings**: Lists show key instead of title ([#2169](https://github.com/BEXIS2/Core/issues/2169))
- **Settings Button**: Add ID ([#2375](https://github.com/BEXIS2/Core/issues/2375))
- **Versions**: Release note is not editable ([#2396](https://github.com/BEXIS2/Core/issues/2396))

#### Accounts and identity
- **Parties**: When a user updates their account information (party), the start and end dates are overwritten ([#2384](https://github.com/BEXIS2/Core/issues/2384))
- **Reset Password**: Revise text ([#2152](https://github.com/BEXIS2/Core/issues/2152))
- **IControllerFactory**: Castle.Proxies.IControllerFactoryProxy did not return a controller for the name '<NAME>' ([#2416](https://github.com/BEXIS2/Core/issues/2416))

#### Metadata and data structures
- **PUM - CSV Import**: Insert id into metadata (@comment) ([#2376](https://github.com/BEXIS2/Core/issues/2376))
- **Data Structure**: Identical column name currently not checked during creation ([#2336](https://github.com/BEXIS2/Core/issues/2336))
- **Data structure**: Select linked datasets ends in Not Found Exception ([#2423](https://github.com/BEXIS2/Core/issues/2423))
- **Data structure**: Missing values are carried over when the data structure is copied.([#2444](https://github.com/BEXIS2/Core/issues/2444))
- **JsonMaxLimit**: Reached by generation of a data structure ([#2417](https://github.com/BEXIS2/Core/issues/2417))
- **Darwin Core Check**: Setting is false, but it is shown ([#2372](https://github.com/BEXIS2/Core/issues/2372))
- **Primary key**: Combination of two keys does not work ([#2419](https://github.com/BEXIS2/Core/issues/2419))
- **Entity Template Order**: Enable an actively defined order instead of magic by last edit ([#2379](https://github.com/BEXIS2/Core/issues/2379))
- **Entity Template**: If the entity template supports many file formats, the icons are not right-aligned ([#2380](https://github.com/BEXIS2/Core/issues/2380))
- **RPM Seed data**: Description improvements and boolean exists twice ([#2403](https://github.com/BEXIS2/Core/issues/2403))
- **Citation API**: Remove check if a valid token is provided for non-public entities ([#2382](https://github.com/BEXIS2/Core/issues/2382))
- **Citation API**: Expanding the API to include the ability to call tags. ([#2394](https://github.com/BEXIS2/Core/issues/2394))

#### Data
- **Data**: Data count always count of latest version ([#2441](https://github.com/BEXIS2/Core/issues/2441))
- **Display Pattern**: Add "yyyy-MM-dd hh:mm:ss" ([#2400](https://github.com/BEXIS2/Core/issues/2400))
- **Missing Values**: String values are no longer replaced by numbers and remain constant. ([#2452](https://github.com/BEXIS2/Core/issues/2452))

#### Download and upload
- **Upload Attachement**: Error shown twice ([#2191](https://github.com/BEXIS2/Core/issues/2191))
- **Download Dataset**: Add metadata as plain text ([#2243](https://github.com/BEXIS2/Core/issues/2243))
- **Download Unstructured**: Dataset contains only one file ([#2404](https://github.com/BEXIS2/Core/issues/2404))
- **Download**: Possible even without check policy by datasets with files ([#2406](https://github.com/BEXIS2/Core/issues/2406))
- **Download package**: Use tags instead of version in tag mode ([#2424](https://github.com/BEXIS2/Core/issues/2424))

#### Search and documentation
- **Documentation**: Add copy and tag icon ([#2371](https://github.com/BEXIS2/Core/issues/2371))
- **Search**: Not updated on metadata edit (old and new form) ([#2434](https://github.com/BEXIS2/Core/issues/2434))
- **Search**: Publication not added to search ([#2435](https://github.com/BEXIS2/Core/issues/2435))


