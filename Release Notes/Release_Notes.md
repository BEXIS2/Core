# BEXIS 2.13 Release Notes

### Features

- Allow email address in addition to account name for login ([#402](https://github.com/BEXIS2/Core/issues/402))

- Allow deleting unstructured primary data ([#381](https://github.com/BEXIS2/Core/issues/381))

- Hide menu entries without permission ([#143](https://github.com/BEXIS2/Core/issues/143))

- Allow to switch between dataset versions and show version information ([#283](https://github.com/BEXIS2/Core/issues/283),[#306](https://github.com/BEXIS2/Core/issues/306))

- Allow to map system data (e.g. version, last modification date) in metadata form (automatically filled; not editable for user) ([#192](https://github.com/BEXIS2/Core/issues/192))

- Allow to link datasets or other entities (e.g. publications) via metadata entry or external link ([#193](https://github.com/BEXIS2/Core/issues/193)). Entity white list ([#374](https://github.com/BEXIS2/Core/issues/374)) to exclude entities + Description for link types ([#372](https://github.com/BEXIS2/Core/issues/372))

- Integration of Multimedia Module ([#281](https://github.com/BEXIS2/Core/issues/281))

- API to read and write data ([#250](https://github.com/BEXIS2/Core/issues/250)) and metadata ([#262](https://github.com/BEXIS2/Core/issues/262)) and a new API for Datasets [#260](https://github.com/BEXIS2/Core/issues/260) and Attachments [#261](https://github.com/BEXIS2/Core/issues/261).


### Enhancements

[#17](https://github.com/BEXIS2/Core/issues/17) Upload data: variable names with double quotes in csv file can't be imported

[#74](https://github.com/BEXIS2/Core/issues/74) Add units of variables to downloaded primary data header
[#218](https://github.com/BEXIS2/Core/issues/218) As a user I would like to see units in the downloaded excel file

- Add BCC, CC and ReplyTo optional to EmailService ([#100](https://github.com/BEXIS2/Core/issues/100))

- Add reason to dataset request ([#130](https://github.com/BEXIS2/Core/issues/130))

- Show version number instead of id all ([#168](https://github.com/BEXIS2/Core/issues/168))

- Add 1st Level tabs in Metadata Edit View ([#191](https://github.com/BEXIS2/Core/issues/191))

- PostgreSQL >9.6 Support (creation of materialized view) ([#259](https://github.com/BEXIS2/Core/issues/259))

- Main CSS colors are replaced by CSS variables ([#268](https://github.com/BEXIS2/Core/issues/268))

- Full name for dataset, entity and feature permission instead of the username ([#269](https://github.com/BEXIS2/Core/issues/269))

[#274](https://github.com/BEXIS2/Core/issues/274) Long waiting times when loading & filtering the Subjects table in Feature permissions View
[#171](https://github.com/BEXIS2/Core/issues/171) Loading time for subjects

[#276](https://github.com/BEXIS2/Core/issues/276) Public data sets must also be accessible via API without a token.

- Support server side-paging for manage users ([#277](https://github.com/BEXIS2/Core/issues/277)), groups ([#278](https://github.com/BEXIS2/Core/issues/278)) and entities ([#279](https://github.com/BEXIS2/Core/issues/279))

[#280](https://github.com/BEXIS2/Core/issues/280) merge external_module_branch to dev

- Improve performance for autocomplete in the metadata form ([#282](https://github.com/BEXIS2/Core/issues/282))

- Allow other entities to have their own details view ([#284](https://github.com/BEXIS2/Core/issues/284))

- Revised registration mail content ([#285](https://github.com/BEXIS2/Core/issues/285))

- Add persistent identifier ("id by xpath") to each metadata element ([#293](https://github.com/BEXIS2/Core/issues/293))

- Mark datasets which have been saved with errors in the dashboard and metadata view ([#309](https://github.com/BEXIS2/Core/issues/309))

- Allow custom help text on top for MetadataEditor ([#395](https://github.com/BEXIS2/Core/issues/395))

- Allow to update PartyTypes while running the partyType.xml again ([#423](https://github.com/BEXIS2/Core/issues/423))

- Add FAQ link to the application ([#426](https://github.com/BEXIS2/Core/issues/426) )

### Bugs
- Fix rights and request Status are encoded in a number in the permission request view ([#187](https://github.com/BEXIS2/Core/issues/187))

[#104](https://github.com/BEXIS2/Core/issues/104) DCM - Attachments 500 Error (not logged in)

[#183](https://github.com/BEXIS2/Core/issues/183) Delete attachment from a dataset is defined by upload inside the database

[#250](https://github.com/BEXIS2/Core/issues/250) Download dataset in text format
- Fix the page is not refreshed after deleting a metadata structure ([#264](https://github.com/BEXIS2/Core/issues/264))

- Fix for a rejected data request you can't make a new one ([#266](https://github.com/BEXIS2/Core/issues/266))

[#267](https://github.com/BEXIS2/Core/issues/267) Refactor Api Data Get filtering and projection

[#272](https://github.com/BEXIS2/Core/issues/272) Results from mapping to keys throw wrong values in special use cases

- Fix search is limited to 1000 results ([#273](https://github.com/BEXIS2/Core/issues/273))

[#275](https://github.com/BEXIS2/Core/issues/275) Attachment API should be secured
- Fix icons not readable on hover on link buttons ([#286](https://github.com/BEXIS2/Core/issues/286))

- Fix info messages is hidden under header in Upload Wizard ([#287](https://github.com/BEXIS2/Core/issues/287))

- Fix public features are not shown in menu ([#288](https://github.com/BEXIS2/Core/issues/288))

[#289](https://github.com/BEXIS2/Core/issues/289) After Metadata change, the dataset view is still in the previews version

[#319](https://github.com/BEXIS2/Core/issues/319) Open mapping tool - Button was hidden

- Fix identified/shown errors in metadata edit form does not disappear after correction ([#308](https://github.com/BEXIS2/Core/issues/308))

- Fix handle data type numeric correct in metadata edit form ([#367](https://github.com/BEXIS2/Core/issues/367))
 
- Fix show formated text in text fields (e.g line breaks) ([#376](https://github.com/BEXIS2/Core/issues/376))

- Fix length of documentation field (xsd) in database only 255 -> changed to text ([#400](https://github.com/BEXIS2/Core/issues/400))

- Fix relationship disappears during field validation during user registration ([#403](https://github.com/BEXIS2/Core/issues/403))

- Fix user registration form validation by jQuery. Do not check non-displayed/hidden fields([#377](https://github.com/BEXIS2/Core/issues/377))

- Fix boolean field in account registration form not working ([#422](https://github.com/BEXIS2/Core/issues/422))

- Fix Javascript code in profile view - unused code removed / missing code added ([#378](https://github.com/BEXIS2/Core/issues/378))

- Fix title fields with line breaks can cause JS errors ([#403](https://github.com/BEXIS2/Core/issues/403), [#392](https://github.com/BEXIS2/Core/issues/392))

>The database update script has been revised. The seed data process has not been revised.

## I. Software Information

-	Name: BEXIS
-	Version: 2.12.0
-	Application Type: Web Application
-	Platform: Windows

## II.	License Agreement
The contents included in this software are licensed to you, the end user. Your use of BEXIS software is subject to the terms of an End User License Agreement (EULA) accompanying the software and located in the \License subdirectory. You must read and accept the terms of the EULA before you access or use BEXIS software. If you do not agree to the terms of the EULA, you are not authorized to use BEXIS software.

