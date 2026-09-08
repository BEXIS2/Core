1. run db script
2. set extension metadatastrutcure mapping -> title and description to system title and description


## if you have allready extensions

3. change existing entitytemplates to extensions if exist
4. create a dataset with extension
5. grab metadata from database
6. change title and description in metadata
7. replace all updated existing extensions with the metadata from point 5
8. change extension datasets metadatastructure id to the metadatastructure name = 'Extension' in datasets
9.  Update entityref as extension id in entity permissions