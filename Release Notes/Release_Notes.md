# BEXIS 2.13 Release Notes

### Features

[#143](https://github.com/BEXIS2/Core/issues/143) Menu - Hide menu entries without permission

[#192](https://github.com/BEXIS2/Core/issues/192) Automatic entry of system data in the metadata that can not be changed by the user

[#193](https://github.com/BEXIS2/Core/issues/193) Users want to link dataset or other entities to datasets. White list [#374](https://github.com/BEXIS2/Core/issues/374) + Description for links [#372](https://github.com/BEXIS2/Core/issues/372)

[#250](https://github.com/BEXIS2/Core/issues/250) extend the API functionalities by writing data in bexis 2

[#260](https://github.com/BEXIS2/Core/issues/260) Dataset API

[#261](https://github.com/BEXIS2/Core/issues/261) Attachment API
		
[#262](https://github.com/BEXIS2/Core/issues/262) extend the existing metadata API functionalities by writing metadata in bexis 2

[#281](https://github.com/BEXIS2/Core/issues/281) Integrate Multimedia Module

[#283](https://github.com/BEXIS2/Core/issues/283) Switching dataset versions in show dataset

[#381](https://github.com/BEXIS2/Core/issues/381) Option to delete primary data - unstructured data

[#402](https://github.com/BEXIS2/Core/issues/402) Allow email address alternativ to account name for login



### Enhancements

[#17](https://github.com/BEXIS2/Core/issues/17) Upload data: variable names with double quotes in csv file can't be imported

[#74](https://github.com/BEXIS2/Core/issues/74) Add units of variables to downloaded primary data header

[#100](https://github.com/BEXIS2/Core/issues/100) AAA - EmailService: BCC, CC and ReplyTo optional

[#130](https://github.com/BEXIS2/Core/issues/130) Dataset request with a reason text field

[#168](https://github.com/BEXIS2/Core/issues/168) Instead of version id all enduser UIs should use the version no.

[#171](https://github.com/BEXIS2/Core/issues/171) Loading time for subjects

[#187](https://github.com/BEXIS2/Core/issues/187) my Dataset requests: Rights and Request Status is encoded in a number

[#191](https://github.com/BEXIS2/Core/issues/191) Alternativ / New Structure Metadata Edit View

[#218](https://github.com/BEXIS2/Core/issues/218) As a user I would like to see units in the downloaded excel file

[#259](https://github.com/BEXIS2/Core/issues/259) PostgreSQL >9.6 Support - Creation of materialized view adjusted

[#268](https://github.com/BEXIS2/Core/issues/268) Replace BExIS colors with CSS variables - Main CSS colours are replaced by CSS vars

[#269](https://github.com/BEXIS2/Core/issues/269) As a user I would like to see the full name in tab dataset permission, instead the username

[#274](https://github.com/BEXIS2/Core/issues/274) Long waiting times when loading & filtering the Subjects table in Feature permissions View

[#276](https://github.com/BEXIS2/Core/issues/276) Public data sets must also be accessible via api without a token.

[#277](https://github.com/BEXIS2/Core/issues/277) Support serverside paging by manage users.

[#278](https://github.com/BEXIS2/Core/issues/278) Support serverside paging by manage groups.

[#279](https://github.com/BEXIS2/Core/issues/279) Support serverside paging by manage entity permissions.

[#280](https://github.com/BEXIS2/Core/issues/280) merge external_module_branch to dev

[#282](https://github.com/BEXIS2/Core/issues/282) performance problems with autocomplete in the metadata form.

[#284](https://github.com/BEXIS2/Core/issues/284) Allow other entities to have their own details view.

[#285](https://github.com/BEXIS2/Core/issues/285) Google detects registration mails as SPAM - revised registration mail

[#392](https://github.com/BEXIS2/Core/issues/392) Title fields with line breaks can cause JS errors

[#293](https://github.com/BEXIS2/Core/issues/293) Add "id by xpath" to each element in the edit form to have a persitent identifier

[#309](https://github.com/BEXIS2/Core/issues/309) Mark datasets which have been saved with errors in the dashboard and metadata view

[#395](https://github.com/BEXIS2/Core/issues/395) Allow custom help text on top for MetadataEditor

[#423](https://github.com/BEXIS2/Core/issues/423) Its not possible to update PartyTypes while running the partyType.xml again

[#426](https://github.com/BEXIS2/Core/issues/426) Add FAQ link to the application

### Bugs

[#104](https://github.com/BEXIS2/Core/issues/104) DCM - Attachments 500 Error (not logged in)

[#183](https://github.com/BEXIS2/Core/issues/183) Delete attachement from a dataset is defined by upload inside the database

[#250](https://github.com/BEXIS2/Core/issues/250) Download dataset in text format

[#264](https://github.com/BEXIS2/Core/issues/264) The page is not refreshed after deleting a metadata structure

[#266](https://github.com/BEXIS2/Core/issues/266) For a rejected data request you can't make a new one

[#267](https://github.com/BEXIS2/Core/issues/267) Refactor Api Data Get filtering and projection

[#272](https://github.com/BEXIS2/Core/issues/272) Results from mapping to keys throw wrong values in special usecases

[#273](https://github.com/BEXIS2/Core/issues/273) The search shows only up to 1000 results.

[#275](https://github.com/BEXIS2/Core/issues/275) Attachment API should be secured

[#286](https://github.com/BEXIS2/Core/issues/286) Icons not readable on hover on link buttons

[#287](https://github.com/BEXIS2/Core/issues/287) Upload Wizard: Info messages hidden under header

[#288](https://github.com/BEXIS2/Core/issues/288) Public features not showing up in Menue

[#289](https://github.com/BEXIS2/Core/issues/289) After Metadata change, the dataset view is stilll in the previews version

[#319](https://github.com/BEXIS2/Core/issues/319) Open mapping tool - Button was hidden

[#308](https://github.com/BEXIS2/Core/issues/308) Errors in select list does not disappear as the content is validated, but not replaced

[#367](https://github.com/BEXIS2/Core/issues/367) Numeric fields - handle data type correct to set max/min

[#376](https://github.com/BEXIS2/Core/issues/376) Format stored text in test field (Enter) - Line breaks are now shown and stored

[#377](https://github.com/BEXIS2/Core/issues/377) User registration form: Do not check none displayed fields - add disabled for jQuery validation to ignore hidden fields

[#378](https://github.com/BEXIS2/Core/issues/378) Cleanup Javascript code in profile view - unused code removed / missing code added

[#400](https://github.com/BEXIS2/Core/issues/400) Length of documentation field (xsd) in database too short - varchar(250) changed to text 

[#403](https://github.com/BEXIS2/Core/issues/403) User registration: Relationship disappaers during field validation

[#403](https://github.com/BEXIS2/Core/issues/403) Title fields with line breaks can cause JS errors

[#422](https://github.com/BEXIS2/Core/issues/422) Boolean field in account registration form has no effect



>The database update script has been revised. The seed data process has not been revised.

## I. Software Information

-	Name: BEXIS
-	Version: 2.12.0
-	Application Type: Web Application
-	Platform: Windows

## II.	License Agreement
The contents included in this software are licensed to you, the end user. Your use of BEXIS software is subject to the terms of an End User License Agreement (EULA) accompanying the software and located in the \License subdirectory. You must read and accept the terms of the EULA before you access or use BEXIS software. If you do not agree to the terms of the EULA, you are not authorized to use BEXIS software.

