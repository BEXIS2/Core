# BEXIS2 Release Notes - Version 5.0.0
> Within this release we continued the refactoring of the UI to Svelte. The main focus was on the metadata edit and view, the dataset details, the primary data view, the dashboard / my data page, the users and groups management and the file display module. Further we added a new feature to support entity extensions. This is for example needed for DWC datasets. In addition, we added a new tool as a prerelease to collect feedback on mapping species against an API (e.g., ChecklistBank) using the BEXIS2 stored datasets For data curators we added a new tool to compare metadata versions or metadata across different datasets. Further we added a new admin feature which allows configuring UI components for view and edit in a graphical interface. This is for example needed to configure the layout and form elements of the new metadata edit form and view. Finally we integrated and improved the citation API and the citation generation for datasets  as well as the download functionality for datasets. In addition we improved the performance of the system and fixed several bugs.

### Workspace changes:
- Workspace changes: [4.3.1..5.0.0](https://github.com/BEXIS2/Workspace/compare/4.3.1..5.0.0)


### Database Update(s):
- Update script from version 4.3.1 to 5.0.0: [Update_Script_4.3.1_5.0.0.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/4.3.1-5.0.0.sql)

### Web.config changes
- ...

### Configuration
- ....

# New and Refactored Features

- **Support Entity Extensions**: Linking / Extension Concept for Entities. This is for example needed for DWC datasets. ([#1891](https://github.com/BEXIS2/Core/issues/1891)) [Entity Extensions Subtsasks](#entity-extensions-subtasks)

- **Metadata Compare Tool**: Tool to help data curators to compare metadata versions or metadata across different datasets. ([#2530](https://github.com/BEXIS2/Core/issues/2530))([#2517](https://github.com/BEXIS2/Core/issues/2517))
  
- **Species Mapping Tool**: Species Mapping against an API (e.g. ChecklistBank) based on in BEXIS stored datasets. This is a prerelease feature for testing purposes. ([#980](https://github.com/BEXIS2/Core/issues/980))

- **Dashboard / My Data**: Refactoring of the dashboard / My Data page to Svelte. ([#2546](https://github.com/BEXIS2/Core/issues/2546))([#2546](https://github.com/BEXIS2/Core/issues/2546)) and view enhancements:
    - Show if a dataset has data. ([#540](https://github.com/BEXIS2/Core/issues/540))([#550](https://github.com/BEXIS2/Core/issues/550))
    - Different tab order
    - Show if a dataset has a tag. ([#2381](https://github.com/BEXIS2/Core/issues/2381))
    - Dataset Copy: Confirmation needed when creating dataset by copying an existing dataset. ([#2473](https://github.com/BEXIS2/Core/issues/2473))
    - Fix: Mapped system keys in the metadata are overwritten in the original dataset. ([#2462](https://github.com/BEXIS2/Core/issues/2462))

- **Users and Groups**: Refactoring of the users and groups management to Svelte. ([#997](https://github.com/BEXIS2/Core/issues/997))([#1202](https://github.com/BEXIS2/Core/issues/1202))([#1212](https://github.com/BEXIS2/Core/issues/1212))
([#1689](https://github.com/BEXIS2/Core/issues/1689))([#1690](https://github.com/BEXIS2/Core/issues/1690))

---

- **UI Component Configuration**: Admin feature which allows configuring UI components for view and edit in a graphical interface. ([#2296](https://github.com/BEXIS2/Core/issues/2296))([#2550](https://github.com/BEXIS2/Core/issues/2550))([#1817](https://github.com/BEXIS2/Core/issues/1817))
    - components can be configured for different entity templates and different types (edit & view)
    - graphical interface in a tree flow structure to configure the components (drag and drop, ...)
    - interactive search and zoom to nodes
    - select and configure different modes for each component (settings, default values, ...)
    - visualize which nodes are mapped to system keys (parties, system keys, ...) 
    - list of all new components: [New Custom UI Components](#new-custom-ui-components)

- **Metadata View**: Refactoring of the metadata view to Svelte. ([#2538](https://github.com/BEXIS2/Core/issues/2538))

- **Metadata Edit**: Refactoring of the metadata edit to Svelte ([#1013](https://github.com/BEXIS2/Core/issues/1013)) ([#2536](https://github.com/BEXIS2/Core/issues/2536)) ([#2009](https://github.com/BEXIS2/Core/issues/2009)) ([#2374](https://github.com/BEXIS2/Core/issues/2374)) ([#2385](https://github.com/BEXIS2/Core/issues/2385)) ([#2386](https://github.com/BEXIS2/Core/issues/2386)). The new edit form validates client and server side ([#2007](https://github.com/BEXIS2/Core/issues/2007)) ([#2008](https://github.com/BEXIS2/Core/issues/2008)) ([#2518](https://github.com/BEXIS2/Core/issues/2518)). In comparison to the old edit form, the new edit form is more flexible and allows to configure the layout of the form. Further it allows to configure default values, descriptions, disable options see [New Custom UI Components](#new-custom-ui-components). The new edit form also allows to use external terminology services to fill metadata fields ([#1741](https://github.com/BEXIS2/Core/issues/1741)). It is mobile friendly and works on tablets and smartphones ([#1102](https://github.com/BEXIS2/Core/issues/1102)). Few Metadata Edit related issues are also solved now  ([#2010](https://github.com/BEXIS2/Core/issues/2010)) ([#2102](https://github.com/BEXIS2/Core/issues/2102)) ([#2148](https://github.com/BEXIS2/Core/issues/2148)) ([#2187](https://github.com/BEXIS2/Core/issues/2187)) ([#2188](https://github.com/BEXIS2/Core/issues/2188)) ([#2226](https://github.com/BEXIS2/Core/issues/2226)) ([#2282](https://github.com/BEXIS2/Core/issues/2282)) ([#2377](https://github.com/BEXIS2/Core/issues/2377)) ([#393](https://github.com/BEXIS2/Core/issues/393)).

- **Dataset Details / Landing page**: Refactoring of the dataset details to Svelte  ([#907](https://github.com/BEXIS2/Core/issues/907))([#2545](https://github.com/BEXIS2/Core/issues/2545))([#2436](https://github.com/BEXIS2/Core/issues/2436)). Further the download functionality has been improved ([#2554](https://github.com/BEXIS2/Core/issues/2554))([#2460](https://github.com/BEXIS2/Core/issues/2460)). Versions and releases are shown now on the right side for easier navigation ([#2556](https://github.com/BEXIS2/Core/issues/2556)). Finally we made sure the old urls are redirected to the new ones  ([#2548](https://github.com/BEXIS2/Core/issues/2548))

- **File Display**: Refactoring the module (MMM) responsible to show uploaded files in a Svelte-based interface. ([#948](https://github.com/BEXIS2/Core/issues/948))

- **Primary Data View**: Refactoring of the primary data view to Svelte. ([#2373](https://github.com/BEXIS2/Core/issues/2373))([#2163](https://github.com/BEXIS2/Core/issues/2163))

## New Custom UI Components

- **Terminology Service**: Terminology Service Suite (TSS) widgets from TS4NFDI (https://terminology.services.base4nfdi.de/) to enable filling metadata based on external terminology services. ([#1741](https://github.com/BEXIS2/Core/issues/1741))
- **Default Values**: Allow to add default values, custom descriptions and disable options. ([#2541](https://github.com/BEXIS2/Core/issues/2541))([#686](https://github.com/BEXIS2/Core/issues/686))
- **Date Range**: Allow to validate a date range (start and end).
- **Horizontal**: Align fields horizontally (2 or 3 fields). Edit and view modes. ([#2539](https://github.com/BEXIS2/Core/issues/2539))
- **ORCID**: Support validation and search for ORCIDs. ([#2535](https://github.com/BEXIS2/Core/issues/2535))
- **ROR**: Support to search for a ROR based on text search. ([#2542](https://github.com/BEXIS2/Core/issues/2542))
- **Text Area**: By default text fields are displayed as a text, but here a text area can be enforced. ([#2543](https://github.com/BEXIS2/Core/issues/2543))
- **Link View**: Allow to display a link based on the value stored as ref in view mode.

## Entity Extensions Subtasks
  - Subtasks done are:
    - Streamlining the Workflow and Data Linking. ([#2195](https://github.com/BEXIS2/Core/issues/2195))
    - Extend links with type and categories. ([#2196](https://github.com/BEXIS2/Core/issues/2196))
    - Create a simple metadata structure and mapping as seed data. ([#2197](https://github.com/BEXIS2/Core/issues/2197))
    - Exclude extension from indexing. ([#2200](https://github.com/BEXIS2/Core/issues/2200))
    - Bi-directional storing of links. ([#2203](https://github.com/BEXIS2/Core/issues/2203))
    - Hide extension types from list when create links between datasets in default UI. ([#2205](https://github.com/BEXIS2/Core/issues/2205))
    - Add selection of extensions to entity templates. ([#2206](https://github.com/BEXIS2/Core/issues/2206))
    - Allow to add extensions on edit page. ([#2209](https://github.com/BEXIS2/Core/issues/2209))
    - Hide Options from EntityTemplate Form. ([#2233](https://github.com/BEXIS2/Core/issues/2233))
    - Add function to delete extension in entity edit view. ([#2235](https://github.com/BEXIS2/Core/issues/2235))
    - Be aware of uniqueness by adding an extension. ([#2236](https://github.com/BEXIS2/Core/issues/2236))
    - Add hooks & single pages for links, publish and permissions. ([#2256](https://github.com/BEXIS2/Core/issues/2256))
    - Return from metadata change of an extension throws an error. ([#2260](https://github.com/BEXIS2/Core/issues/2260))
    - Disable hooks in backend for extension. ([#2265](https://github.com/BEXIS2/Core/issues/2265))
    - Update seed data because of changing XSD import XPaths. ([#2289](https://github.com/BEXIS2/Core/issues/2289))
    - Update description of DWC links. ([#2521](https://github.com/BEXIS2/Core/issues/2521))
    - Upload causes an endless loop. ([#2522](https://github.com/BEXIS2/Core/issues/2522))

# Bugfixes and enhancements
### Bugfixes and Enhancements

#### Data Structure
- **Data Structure and Constraints**: Fix: Used constraints have a very long loading time. ([#2314](https://github.com/BEXIS2/Core/issues/2314))
- **Data Structure Edit**: Fix: Validation information of primary key is wrong. ([#2337](https://github.com/BEXIS2/Core/issues/2337))
- **Data Structure Detection**: Fix: Change Encoding does not trigger reload preview. ([#2428](https://github.com/BEXIS2/Core/issues/2428))
- **Data Structure**: Fix: Long loading time data structure edit. ([#2492](https://github.com/BEXIS2/Core/issues/2492))
- **Edit Data Structure**: Fix: Footer missing. ([#2519](https://github.com/BEXIS2/Core/issues/2519))
- **Data Structure Validation**: Fix: Error message too long and hard to read. ([#2525](https://github.com/BEXIS2/Core/issues/2525))
- **Data Structure Validation**: Fix: Error difficult to understand. ([#2527](https://github.com/BEXIS2/Core/issues/2527))
- **Scrolling**: Reduce needed scrolling and format. ([#2507](https://github.com/BEXIS2/Core/issues/2507))

#### File Upload
- **Upload Flow**: Reduce confirmation clicks to upload data. ([#2340](https://github.com/BEXIS2/Core/issues/2340))
- **File Upload**: Fix: Information is misleading for user. ([#2414](https://github.com/BEXIS2/Core/issues/2414))
- **File Upload**: Send an email when a file is uploaded. ([#2415](https://github.com/BEXIS2/Core/issues/2415))
- **File Upload**: Fix: Loading animation not shown. ([#2431](https://github.com/BEXIS2/Core/issues/2431))
- **File Upload**: User should get clearer feedback that the submit data was successful. ([#2432](https://github.com/BEXIS2/Core/issues/2432))
- **File Upload/Removal**: Inconsistencies in infos. ([#2437](https://github.com/BEXIS2/Core/issues/2437))

#### Search
- **Search Card View**: Add dataset ID. ([#2465](https://github.com/BEXIS2/Core/issues/2465))
- **Search Card View**: Fix: Number of displayed items not updated on change in card view. ([#2529](https://github.com/BEXIS2/Core/issues/2529))
- **Search Table View**: Table view - limit abstract length. ([#2484](https://github.com/BEXIS2/Core/issues/2484))
- **Search Table View**: Change table mode to use clientDB. ([#2534](https://github.com/BEXIS2/Core/issues/2534))


#### Citation & DOI
- **Dataset Details View**: Citation basic implementation and generate citation suggestion automatically. ([#827](https://github.com/BEXIS2/Core/issues/827)) ([#2139](https://github.com/BEXIS2/Core/issues/2139))
- **Citation API**: Consolidation and Improvement. ([#2240](https://github.com/BEXIS2/Core/issues/2240))
- **Citation API**: Fix: Projects not deserializable and the entryType should not be required. ([#2300](https://github.com/BEXIS2/Core/issues/2300))
- **Citation API**: Refactor dataset version selection and handle missing default case. ([#2520](https://github.com/BEXIS2/Core/issues/2520))
- **Citation API**: Fix: Public datasets - citation should not contain dataset ID in the end. ([#2544](https://github.com/BEXIS2/Core/issues/2544))
- **Manage DOI Requests**: Sort list descending by default. ([#2459](https://github.com/BEXIS2/Core/issues/2459))
- **Metadata API**: Ensure validity of DataCite concept model. ([#2251](https://github.com/BEXIS2/Core/issues/2251))

#### Tags
- **Tag Management**: Correct & improve release request mail. ([#2480](https://github.com/BEXIS2/Core/issues/2480))
- **Tag Management**: Fix: Add missing IDs. ([#2526](https://github.com/BEXIS2/Core/issues/2526))
- **Tags**: Hide publish tag toggle. ([#2531](https://github.com/BEXIS2/Core/issues/2531))
- **Tags**: Add "tags" as internal key to make usage possible. ([#2540](https://github.com/BEXIS2/Core/issues/2540))

#### Variable Templates & Meanings
- **Template Suggestion**: Improve matching logic from variable to variable templates. ([#2331](https://github.com/BEXIS2/Core/issues/2331))
- **Variable Template(s)**: No Persistence of Missing Value(s). ([#2369](https://github.com/BEXIS2/Core/issues/2369))
- **Variable Template**: Create new form closes randomly. ([#2401](https://github.com/BEXIS2/Core/issues/2401))
- **Meanings**: Fix: Create new freezes page. ([#2486](https://github.com/BEXIS2/Core/issues/2486))
- **Meanings**: Fix: Linked variable templates do not block delete. ([#2326](https://github.com/BEXIS2/Core/issues/2326))

#### Mail & Notifications
- **Notification Email**: Create Dataset - allow instance specific text. ([#1080](https://github.com/BEXIS2/Core/issues/1080))
- **System Mails**: Email Header unification. ([#1308](https://github.com/BEXIS2/Core/issues/1308))
- **System Mails**: Show display name instead of user name. ([#2390](https://github.com/BEXIS2/Core/issues/2390))
- **System Mails**: Add user to "Metadata updated" mail. ([#2395](https://github.com/BEXIS2/Core/issues/2395))
- **Permission Request**: Inform all owners about a request. ([#671](https://github.com/BEXIS2/Core/issues/671))

#### Configuration & Settings
- **Security Configuration**: Security configuration within General Settings. ([#2438](https://github.com/BEXIS2/Core/issues/2438))
- **Settings**: Fix: Sub-items only clickable on text. ([#2510](https://github.com/BEXIS2/Core/issues/2510))
- **Settings**: Select from list. ([#2549](https://github.com/BEXIS2/Core/issues/2549))
- **Settings JSON**: Save settings as formatted JSON. ([#2547](https://github.com/BEXIS2/Core/issues/2547))
- **Web.config**: Cleanup Web.config.sample. ([#2175](https://github.com/BEXIS2/Core/issues/2175))

#### Performance & Infrastructure
- **Table**: Handle 20 000+ rows with our current table lib client side. ([#2533](https://github.com/BEXIS2/Core/issues/2533))
- **Text Size**: Reduce globally default text size. ([#2506](https://github.com/BEXIS2/Core/issues/2506))
- **Jenkins**: Jenkins Pipeline Mail outcome check. ([#1604](https://github.com/BEXIS2/Core/issues/1604))
- **Jenkins**: Reduce long pipeline execution during UI project build. ([#2355](https://github.com/BEXIS2/Core/issues/2355))
- **Backend**: Add Session Read only to backend calls. ([#2505](https://github.com/BEXIS2/Core/issues/2505))
- **Response Times**: Fix: Very long response times for GetAntiForgeryToken & GetApplicationName. ([#2303](https://github.com/BEXIS2/Core/issues/2303))
- **Edit Page**: Fix: Long loading time for Edit page. ([#2490](https://github.com/BEXIS2/Core/issues/2490))
- **Packages**: Update packages in Svelte UI projects. ([#2491](https://github.com/BEXIS2/Core/issues/2491))
- **Redirects**: Remove Redirects if not needed. ([#2500](https://github.com/BEXIS2/Core/issues/2500))
- **Library Upgrade**: Vaelastrasz.Library upgrade from 6.2.2 to 6.3.0. ([#2524](https://github.com/BEXIS2/Core/issues/2524))
- **Unit of Work**: Change unit of work usage within manager. ([#2389](https://github.com/BEXIS2/Core/issues/2389))
- **Curation Tool**: Add group "curator" by default to demo and rc. ([#2268](https://github.com/BEXIS2/Core/issues/2268))
- **Demo Startpage**: Improvements. ([#2332](https://github.com/BEXIS2/Core/issues/2332))

#### Other
- **New Metadata Edit/View**: Add testpage. ([#2361](https://github.com/BEXIS2/Core/issues/2361))
- **DIM Seed Data**: DIM Seed Data Update. ([#2186](https://github.com/BEXIS2/Core/issues/2186))
- **Help Texts**: Improve help texts. ([#2045](https://github.com/BEXIS2/Core/issues/2045))


