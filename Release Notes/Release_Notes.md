# BEXIS 2.15 Release Notes
>Attention in this version the operation table has changed. Please run the database update script (2.14.6-2.15).

>Furthermore, the download options for ascii have changed! 
The label now shows the separator and the file type. .csv is now exported with comma and .txt with semicolon.

### Features
- API to get metadata structures as JsonSchema ([#916](https://github.com/BEXIS2/Core/issues/916))([#953](https://github.com/BEXIS2/Core/issues/953))
  
### Enhancements
- Extending the metadata api to get and update metadata based on json ([#956](https://github.com/BEXIS2/Core/issues/956))
- Modification of the ascii files download options ([#858](https://github.com/BEXIS2/Core/issues/858))


### Bugs
- Fix, system saves emails with spaces in person form ([#901](https://github.com/BEXIS2/Core/issues/901))
- Fix, export metadata page design destroyed after paging ([#955](https://github.com/BEXIS2/Core/issues/955))
- Fix, exporting data via the api now also contains the missing values. ([#942](https://github.com/BEXIS2/Core/issues/942))
