
# BEXIS 2 Release Notes - Version 4.2.0
> This version holds several UX/UI improvements, bugfixes and enhancements related to dataset creation and upload and a revised download. For example single file downloads without metadata are no longer allowed without edit permissions. The content of the download archive also have been extended und refactored. During creation you can now also enable to check if required DWC terms are assigned (show as warning). Further our new integrated curation tool has been added for testing. 

### Workspace changes:
- Workspace changes: [4.1.0..4.2.0](https://github.com/BEXIS2/Workspace/compare/4.1.0..4.2.0)

### Database Update(s):
- Update script from version 4.1.0 to 4.2.0: [Update_Script_4.1.0_4.2.0.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/4.1.0-4.2.0.sql)

### New Settings:
- **Show Darwin Core Archive Validation** (RPM)
If this flag is true, you can validate your data structure against a darwin core archive type.

- **Curation Group Name** & **Curation Labels** (DDM)
The new curation module was added for testing (Instruction: https://github.com/BEXIS2/Core/issues/2277). It is not ready for production! 

- **Data Agreement by download** (DDM)
Set up if user need to agree the data agreements before download and where the agreement is written.
"options": ["none","data policy","terms and conditions"]





# Bugfixes and enhancements
### New features


### UI / UX
- **Documentation**: Change edit and add save icon. ([#2221](https://github.com/BEXIS2/Core/issues/2221))
- **Documentation**: Fix unintended line breaks before images ([#2247](https://github.com/BEXIS2/Core/issues/2247))
- **Search UI**: Small improvements. ([#2232](https://github.com/BEXIS2/Core/issues/2232))
- **Tag Management**: UX improvements. ([#2198](https://github.com/BEXIS2/Core/issues/2198))
- **Tags on dataset landing page**: Improve UI on current dataset landing page. ([#2193](https://github.com/BEXIS2/Core/issues/2193))
- **Dataset View**: Missing icon for unstructured download. ([#2199](https://github.com/BEXIS2/Core/issues/2199))
- **Create Data Structure**: Extend info text shown in the create data structure flow. ([#2248](https://github.com/BEXIS2/Core/issues/2248))
- **Entity Template:** Fix Wrong (trash) icon for cancel button in edit mode ([#2228](https://github.com/BEXIS2/Core/issues/2228))
- **Core UI**: UX improvements across the core UI. ([#2223](https://github.com/BEXIS2/Core/issues/2223))
- **Accessibility**: Accessibility improvements for Dataset Creation / Edit / Data Structure flows. ([#2271](https://github.com/BEXIS2/Core/issues/2271))


### Search
- **Search Overview**: Fix ID is sorted text based instead by number** ([#2146](https://github.com/BEXIS2/Core/issues/2146))
 

### Authentication & login
- **External providers**: Additional text about potential external login providers. ([#2210](https://github.com/BEXIS2/Core/issues/2210))


### Public data & downloads
- **Terms**: Add terms and conditions to the download package. ([#855](https://github.com/BEXIS2/Core/issues/855))
- **Download scope**: Only the whole package should be downloadable. ([#778](https://github.com/BEXIS2/Core/issues/778))
- **Agreement checkbox**: Optional checkbox that data agreement is accepted before download. ([#779](https://github.com/BEXIS2/Core/issues/779))
- **Download info**: Extend download information shown to users. ([#2147](https://github.com/BEXIS2/Core/issues/2147))
- **Download dataset**: Allow filter data on package download ([#2238](https://github.com/BEXIS2/Core/issues/2238))
- **Download dataset**: Better filename ([#445](https://github.com/BEXIS2/Core/issues/445))

### Dataset management & safety
- **Purge confirmation**: Purge dataset action should require confirmation. ([#2239](https://github.com/BEXIS2/Core/issues/2239))


### Metadata, mapping & structure
- **Dataset API**: Fix Create dataset not possible (multiple controller) ([#2255](https://github.com/BEXIS2/Core/issues/2255))
- **Dataset Details**: Title is cut on "-" — prevent incorrect truncation. ([#2270](https://github.com/BEXIS2/Core/issues/2270))
- **Deleted dataset view**: Not shown if not logged in; searchable but metadata hidden. ([#2245](https://github.com/BEXIS2/Core/issues/2245))
- **Dataset Details**: Restore missing Metadata Edit button on details page. ([#2273](https://github.com/BEXIS2/Core/issues/2273))
- **Complex types**: Random reuse of metadata elements with the same name in complex types. ([#2189](https://github.com/BEXIS2/Core/issues/2189))
- **Concept Output**: Multi complex → one complex conversion does not work. ([#2224](https://github.com/BEXIS2/Core/issues/2224))
- **Structure Mapping**: Allow mapping multiple default sources to one target. ([#2227](https://github.com/BEXIS2/Core/issues/2227))


### Upload & parsing
- **DateTime parsing**: DateTime like "24.12.2025 17:23:00" is not working. ([#2231](https://github.com/BEXIS2/Core/issues/2231))


## Data structure / DWC
- **DWC**: Allow to check requirements during data structure creation ([#2249](https://github.com/BEXIS2/Core/issues/2249))
- **Data Structure**: Provide in another file format (and rename file extension) ([#940](https://github.com/BEXIS2/Core/issues/940))
- **DWC Export**: Fix fail because of new zip library ([#2252](https://github.com/BEXIS2/Core/issues/2252))
- **Data Structure Edit**: Fix Selecting a meaning opens the select template overlay ([#2222](https://github.com/BEXIS2/Core/issues/2222))
- **External Link**: Fix Missing validation and required fields indicator ([#2218](https://github.com/BEXIS2/Core/issues/2218))
- **Manage Meaning**: Fix Crashes when saving empty relation([#2215](https://github.com/BEXIS2/Core/issues/2215))

### DOI & Citation
- **DOI Overview**: Fix links not working. ([#2261](https://github.com/BEXIS2/Core/issues/2261))
