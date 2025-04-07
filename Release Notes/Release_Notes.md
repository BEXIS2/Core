> This version mainly contains fixed bugs and usability improvements of the new functions and features, focusing on the area of dataset creation and upload introduced since version 3.0.0. and a refactored documentation.

<b>Important:</b> web.config.samples and the workspace contain important changes.

### Workspace changes:
- Workspace changes: [3.4.0..4.0.0](https://github.com/BEXIS2/Workspace/compare/3.4.0..4.0.0)

### Database Update(s):
- Update script from version 3.4.3 to 4.0.0: [Update_Script_3.4.0_4.0.0.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/3.4.0-4.0.0.sql)

### New Settings
__gbifCollectionArea__ (_Settings -> Application Settings -> Data Dissemination_) (default: "export/gbif")

All created Darwin Core archive dataset exports are saved in this specified directory. It starts from the predefined data folder as default. It is also possible to specify a sequential folder path. default: {Data}/export/gbif".

__GBIF API Credentials__ (_Settings -> Application Settings -> Data Dissemination_) (default: "export/gbif")

__Update Variable Description by Template selection__ (_Settings -> Application Settings -> Data Structure_) (default: true)

If the template support is activated, then empty unit & datatype fields with information from the template are set when the data structure is created.

# Bugfixes and enhancements
## Metadata
- **Improve Metadata Loading Time**. ([#1970](https://github.com/BEXIS2/Core/issues/10970))
- **Choices and default values are not working**: Fix so that all choices will be generated. ([#2040](https://github.com/BEXIS2/Core/issues/2040))
- **Load metadata email** Fix it breaks when email address has no "@" inside. ([#2058](https://github.com/BEXIS2/Core/issues/2058))
- **Metadata Form**: Fix after selection of an autocomplete value; the reload gets stuck. ([#2031](https://github.com/BEXIS2/Core/issues/2031))
- **Session Problems**: Fix when two different metadata forms open; save does not work correctly. ([#93](https://github.com/BEXIS2/Core/issues/93))
  **Show Dataset**: Fix warning that metadata is not complete is missing. ([#2001](https://github.com/BEXIS2/Core/issues/2001))
- **Metadata Validation**: Fix new validation against JSON schema not working with GBIF schema. ([#2043](https://github.com/BEXIS2/Core/issues/2043))
- **Metadata View**: Fix last tag is always displayed. ([#2067](https://github.com/BEXIS2/Core/issues/2067))

## Links
- **Link Dataset**: Fix cancel button does not work. ([#1569](https://github.com/BEXIS2/Core/issues/1569))
- **Linking Datasets**: Fix not the newest version taken. ([#1152](https://github.com/BEXIS2/Core/issues/1152))

## Download
- **Download Zip**: Fix it should not include data with units of older versions. ([#1321](https://github.com/BEXIS2/Core/issues/1321))
- **Download Filtered Data**: Fix not working. ([#1963](https://github.com/BEXIS2/Core/issues/1963))


## Entity Template
- **Entity Template**: Enhancement: Add option to de/activate. ([#1897](https://github.com/BEXIS2/Core/issues/1897))
- **Manage Entity Templates**: Enhancement: Improvements. ([#1973](https://github.com/BEXIS2/Core/issues/1973))
- **Reorder Entity Template Edit From**: Enhancement: reorder Edit view. ([#1972](https://github.com/BEXIS2/Core/issues/1972))
- **Entity Template**: Fix description field is too short. ([#2072](https://github.com/BEXIS2/Core/issues/2072))

## Dataset Management
- **Deleted Datasets**: Fix public datasets that got deleted are not visible anymore. ([#1812](https://github.com/BEXIS2/Core/issues/1812))
- **Dataset Creation**: Enhancement: Add more descriptive text. ([#1976](https://github.com/BEXIS2/Core/issues/1976))
- **Dataset Edit Mode**: Enhancement: Improve the visibility/comprehensibility of file lists. ([#1987](https://github.com/BEXIS2/Core/issues/1987))
- **Uploaded Files**:Enhancement: Uploaded files not visible in edit mode. ([#1975](https://github.com/BEXIS2/Core/issues/1975))
- **Uploaded Files**: Enhancement: Uploaded files as primary data should be editable like attachments. ([#1867](https://github.com/BEXIS2/Core/issues/1867))
- **Dataset History**: Fix attachment actions that create a history bug. ([#1985](https://github.com/BEXIS2/Core/issues/1985))
- **Permission Request**: Fix should not be possible if no data is available. ([#1602](https://github.com/BEXIS2/Core/issues/1602))
- **500 Error**: Fix if you open a dataset without permission. ([#1915](https://github.com/BEXIS2/Core/issues/1915))
- **Edit Constraints**: Correct typos. ([#1977](https://github.com/BEXIS2/Core/issues/1977))
- **Edit Dataset Changes**: ([#1898](https://github.com/BEXIS2/Core/issues/1898))
- **New Primary Data View**: Fix header with "dot" inside breaks the table. ([#2074](https://github.com/BEXIS2/Core/issues/2074))
- **Order of Action Buttons**: Adjust order of action buttons in "My Data".([#2064](https://github.com/BEXIS2/Core/issues/2064))

## Data Structure
- **Date Pattern**: Fix using date pattern `yyyy,MM` creates invalid primary data display. ([#1870](https://github.com/BEXIS2/Core/issues/1870))
- **Date Validation**: Fix validation not working for the format `yy-MM-dd`. ([#2063](https://github.com/BEXIS2/Core/issues/2063))
- **Add Description**: Enhancement: Add description to data structure download. ([#1780](https://github.com/BEXIS2/Core/issues/1780))
- **Change of Delimiter**: Fix change of delimiter while creating data structure doesn't work correctly. ([#2047](https://github.com/BEXIS2/Core/issues/2047))
- **Error Message**: Fix the error message when creating a data structure. ([#2046](https://github.com/BEXIS2/Core/issues/2046))
- **Order Structures**: Enhancement: Order structures in the table by ID. ([#2048](https://github.com/BEXIS2/Core/issues/2048))
- **Suggestion with Negative Number**: Fix suggestion with a negative number breaks the application. ([#2057](https://github.com/BEXIS2/Core/issues/2057))
- **Edit Data Structure**: Enhancement: Remove string warning/preview/MV (no preview data). ([#2002](https://github.com/BEXIS2/Core/issues/2002))
- **Create New from Edit Dataset**: Fix after saving, the user lands on the data structures page. ([#2038](https://github.com/BEXIS2/Core/issues/2038))
- **Primary Key Warning**: Enhancement: Add warning if no primary key is set on submit. ([#2034](https://github.com/BEXIS2/Core/issues/2034))
- **After Deleting a Variable**: Fix save is not possible after deleting a variable. ([#2026](https://github.com/BEXIS2/Core/issues/2026))
- **Check Cancel Button**: Fix cancel button behavior in file info selection. ([#20](https://github.com/BEXIS2/Core/issues/20))
- **Correct Encoding Typo**: Fix encoding typo in "New Data Type". ([#1962](https://github.com/BEXIS2/Core/issues/1962))
- **Constraint Selection**: Fix constraints break layout. ([#1896](https://github.com/BEXIS2/Core/issues/1896))
- **Edit Meaning Name**: Fix when editing a meaning, the name has to be changed to enable save. ([#1775](https://github.com/BEXIS2/Core/issues/1775))
- **Help/Table View**: Enhancement: Add help/table view to templates during data structure edit. ([#1886](https://github.com/BEXIS2/Core/issues/1886))
- **Improve Upload Mails**: Enhance the content and formatting of upload emails. ([#1854](https://github.com/BEXIS2/Core/issues/1854))
- **Variable Template**: Fix sorting by data type and unit does not work. ([#1999](https://github.com/BEXIS2/Core/issues/1999))

## Search
- **All IDs**: Fix all IDs (from every entity) should be part of the search by default. ([#1894](https://github.com/BEXIS2/Core/issues/1894))
- **Public Search**: Fix missed refactor. ([#2056](https://github.com/BEXIS2/Core/issues/2056))
- **Search Refactoring**: Refactor UI based on BiodivBank example. ([#2039](https://github.com/BEXIS2/Core/issues/2039))
- **Card Metadata Value**: Fix cards show "null" if metadata value is empty. ([#2035](https://github.com/BEXIS2/Core/issues/2035))
- **Edit Search Attribute**: Remove seated placeholder. ([#2036](https://github.com/BEXIS2/Core/issues/2036))
- **Cards License**: Fix license not shown on cards. ([#2037](https://github.com/BEXIS2/Core/issues/2037))
- **Search Page**: Fix table sort ID like a string. ([#2013](https://github.com/BEXIS2/Core/issues/2013))
- **Search Page**: Fix card view shows incorrect content. ([#2019](https://github.com/BEXIS2/Core/issues/2019))
- **Search Page**: Fix facet items with "-" are not encoded correctly. ([#1908](https://github.com/BEXIS2/Core/issues/1908))
- **Search Page**: Fix Add missing title/label for screen reader. ([#1910](https://github.com/BEXIS2/Core/issues/1910))
- **Search Page**: Change button text. ([#102](https://github.com/BEXIS2/bexis2-core-ui/issues/102))
- **Search Page**: Fix facet not sorted according to the amount of hits. ([#103](https://github.com/BEXIS2/bexis2-core-ui/issues/103))
- **Search Page**: Fix long names need to be cut. ([#100](https://github.com/BEXIS2/bexis2-core-ui/issues/100))
- **Search Page**: Fix order items column-wise instead of row-wise. ([#101](https://github.com/BEXIS2/bexis2-core-ui/issues/101))
- **Search Page**: Fix the table outside the view area. ([#1904](https://github.com/BEXIS2/Core/issues/1904))
- **Search Index**: Fix bigger search index not working (no response). ([#1995])(https://github.com/BEXIS2/Core/issues/1995)


## UI/UX
- **Option to Show Items**: Enhancement: Show the number of items instead of the number of pages in the table. ([#114](https://github.com/BEXIS2/bexis2-core-ui/issues/114))
- **Export as JSON**: Enhancement: Add "Export as JSON" feature. ([#1953](https://github.com/BEXIS2/Core/issues/1953))
- **Add Toggle for Table**:  Enhancement: Add toggle for disabling/enabling show/hide columns button. ([#115](https://github.com/BEXIS2/bexis2-core-ui/issues/115))
- **Adjust Default Footer**:  Enhancement: Adjust default footer. ([#2055](https://github.com/BEXIS2/Core/issues/2055))
- **Help Menu Items**: Fix help menu items should open in a new tab (new layout). ([#1878](https://github.com/BEXIS2/Core/issues/1878))
- **Svelte Menu**: Fix log off does not work. ([#2029](https://github.com/BEXIS2/Core/issues/2029))
- **Add Option**: Enhancement: Add option to hide header and footer on the landing page. ([#2024](https://github.com/BEXIS2/Core/issues/2024))
- **Store Svelte Menu in Session**: Enhancement: Store the Svelte menu state in the session. ([#1980](https://github.com/BEXIS2/Core/issues/1980))
- **Core UI Table Issues**: ([#112](https://github.com/BEXIS2/bexis2-core-ui/issues/112))
- **Displaying Number of Items**: Fix table does not update correctly. ([#117](https://github.com/BEXIS2/bexis2-core-ui/issues/117))
- **Help & Scroll Button**: Fix help & scroll to top button too small. ([#1899](https://github.com/BEXIS2/Core/issues/1899))
---

## DOI & Publishing
- **DWC export with extensions** Enhancements and fixes ([#1911](https://github.com/BEXIS2/Core/issues/1911))
- **Retrieval of Correct Broker**: Fix retrieval of the correct and appropriate broker. ([#1929](https://github.com/BEXIS2/Core/issues/1929))
- **Refactor Publisher Preparing Page**: Refactor the publisher preparing page. Enhancements
- **DOI**: Enhancements: Functions to update single fields in metadata without new version ([#1111](https://github.com/BEXIS2/Core/issues/1111))
- **Publishing**: Enhancements: Add external link type, dataset and tag for selection ([#1111](https://github.com/BEXIS2/Core/issues/1111))


## Permission & User
- **Can't Delete User**: Cannot delete user. ([#2022](https://github.com/BEXIS2/Core/issues/2022))
- **Check/Get Entity Permission**: Based on entity type (and specific entity name). ([#1902](https://github.com/BEXIS2/Core/issues/1902))
- **Retrieval of User**: Fix retrieval of user within feature permissions check fails. ([#1978](https://github.com/BEXIS2/Core/issues/1978))
- **Wrong Return Value**: Fix the wrong return value for the feature permission manager function. ([#1922](https://github.com/BEXIS2/Core/issues/1922))
- **Feature Permissions**: Fix cannot remove rights. ([#2066](https://github.com/BEXIS2/Core/issues/2066))

## General & Maintenance
- **Check Libraries**: Check libraries for updates. ([#1991](https://github.com/BEXIS2/Core/issues/1991))
- **Binding Redirect**: Redirect of Iesi.Collections and NHibernate within app.config(s). ([#2003](https://github.com/BEXIS2/Core/issues/2003))
- **Maintenance**: Clean-up and removal of warnings within the whole solution. ([#1954](https://github.com/BEXIS2/Core/issues/1954))
- **Maintenance**: NuGet package version reference of Iesi.Collections differs. ([#1948](https://github.com/BEXIS2/Core/issues/1948))
- **Maintenance**: Package updates/alternatives based on Dependabot alerts. ([#1956](https://github.com/BEXIS2/Core/issues/1956))
- **Maintenance**: Unity start session level container exception. ([#1930](https://github.com/BEXIS2/Core/issues/1930))
- **Version Reference**: Version reference of system.web.http differs. ([#1949](https://github.com/BEXIS2/Core/issues/1949))
- **Version Reference**: Version reference of system.buffers differs. ([#1950](https://github.com/BEXIS2/Core/issues/1950))
- **Prepare Solution**: Prepare solution with MVCBuildViews in release mode. ([#1846](https://github.com/BEXIS2/Core/issues/1846))
- **Package Replacement**: Replace DotNetZip package. ([#1958](https://github.com/BEXIS2/Core/issues/1958))
- **Documentation View**: Fix Requires login; adjust documentation link; fix rendering error. ([#2069](https://github.com/BEXIS2/Core/issues/2069))
- **Add Publication to Seed Data**: Enhancement: Add publication to seed data for entity templates. ([#1990](https://github.com/BEXIS2/Core/issues/1990))
- **Add Schema.org Seed Data**:Enhancement: Add seed data for Schema.org. ([#2073](https://github.com/BEXIS2/Core/issues/2073))

## API
- **Improve Swagger API Action List View**. ([#2011](https://github.com/BEXIS2/Core/issues/2011))
- **Dataset API**: Fix creating a dataset without a data structure is not possible. ([#2059](https://github.com/BEXIS2/Core/issues/2059))
- **Dataset API**: Fix to throw an error if the dataset has no data structure. ([#2033](https://github.com/BEXIS2/Core/issues/2033))
- **Metadata Schema**: Fix Out API JSON schema; type element in root node is missing. ([#1952](https://github.com/BEXIS2/Core/issues/1952))
- **Data Structure API**: Fix meanings and constraints missing.([#2079](https://github.com/BEXIS2/Core/issues/2079))

## Documentation
- **Docs**: Refactor documentation ([#2028](https://github.com/BEXIS2/Core/issues/2028))
