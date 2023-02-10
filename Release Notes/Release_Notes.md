# BEXIS 2.17 Release Notes
>Web.config update to add Exceptionless -> see: https://github.com/BEXIS2/Core/commit/8b50510f30bd9c00a20faedb9fecf9e255005085

**Workspace changes:** https://github.com/BEXIS2/Workspace/compare/2.16...2.17

### Features
- Adding Exceptionless for System Loggging/Monitoring ([#1040](https://github.com/BEXIS2/Core/issues/1040))
- Add attributes during XSD import, show and edit ([#1050](https://github.com/BEXIS2/Core/issues/1050))

  
### Enhancements
- Metadata API: Add check for public ([#1037](https://github.com/BEXIS2/Core/issues/1037))
- Metadata API: Add request by version ([#1036](https://github.com/BEXIS2/Core/issues/1036))
- Dataset API: Add title ([#1075](https://github.com/BEXIS2/Core/issues/1075))
- Metadata Statistic API: Exclude ds, add regex and metadata structure ([#1014](https://github.com/BEXIS2/Core/issues/1014))
- Schema Mapping: User should be able to map a key to a default value if no field matches ([#1048](https://github.com/BEXIS2/Core/issues/1048))
- Schema Mapping: (Automatic) bidirectional mapping ([#1008](https://github.com/BEXIS2/Core/issues/1008))
- Schema Mapping: Convert hard coded system keys of B2 to a dynamic list enhancement ([#963](https://github.com/BEXIS2/Core/issues/963))
- SAM - Former member feature: Add tests ([#920](https://github.com/BEXIS2/Core/issues/920))
- SAM - Former member feature: Add documentation ([#921](https://github.com/BEXIS2/Core/issues/921))
- LDAP Login: Check of "null" or "empty" email addresses ([#1067](https://github.com/BEXIS2/Core/issues/1067))
- Set/Unset Public: Send email to Admin ([#1026](https://github.com/BEXIS2/Core/issues/1026))
- Dataset Download: Adapt text in Download-Button of unstructured datasets ([#1030](https://github.com/BEXIS2/Core/issues/1030))
- Code Cleanup: Removal of unnecessary rpm projects ([#1065](https://github.com/BEXIS2/Core/issues/1065))
- Wiki page for E2E tests recordings for Data Manager ([#1010](https://github.com/BEXIS2/Core/issues/1010))

 
### Bugs
- Fix row count to hide Excel download not always correct bug ([#1046](https://github.com/BEXIS2/Core/issues/1046))
- Fix wrong mail subject if you revoke status (Former Members) ([#1073](https://github.com/BEXIS2/Core/issues/1073))
- Fix change to former member return always true (Former Members) ([#1043](https://github.com/BEXIS2/Core/issues/1043))
- Fix after deleting a dataset users are not able to change the permissions ([#1033](https://github.com/BEXIS2/Core/issues/1033))
- Fix attachment uploads does not trigger system key (e.g. version) update in metadata ([#1045](https://github.com/BEXIS2/Core/issues/1045))
- Fix Swagger API should not read non-XML files ([#1035](https://github.com/BEXIS2/Core/issues/1035))
- Fix empty metadata container ([#1039](https://github.com/BEXIS2/Core/issues/1039))
- Fix XSD import set elements as complex type, when no datatype is defined ([#1034](https://github.com/BEXIS2/Core/issues/1034))
- Fix during user registration relationships title has an additional colon ([#1057](https://github.com/BEXIS2/Core/issues/1057))
- Fix prevent send notification email on video preview ([#1024](https://github.com/BEXIS2/Core/issues/1024))
- Fix authorization via API: Make http authorization header field name handling case-insensitive ([#1023](https://github.com/BEXIS2/Core/issues/1023))
- Fix missing try/catch within FromExcelSerialDate(data import) ([#1068](https://github.com/BEXIS2/Core/issues/1068))
- Fix missing try/catch for better error handling (email service) ([#1060](https://github.com/BEXIS2/Core/issues/1060))
