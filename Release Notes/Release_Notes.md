# BEXIS2 3.3.1 Release Notes
>In this small update we have fixed problems with uploading data and metadata editing and extended the concept to date cite, which allows you to send more metadata.


### Workspace changes:
- Workspace changes: [3.3.0..3.3.1](https://github.com/BEXIS2/Workspace/compare/3.3.0..3.3.1)

### Database Update(s):
- Update script version 3.3.0 to 3.3.1:
- [Update_Script_3.3.0_3.3.1.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/3.3.0_3.3.1.sql)

### Enhancements
- Add api error messages as notification to page component [#91](https://github.com/BEXIS2/bexis2-core-ui/issues/91)
  
### Bugfixes
- Fix Read a file with delimiter inside the text quotes is not working [#1753](https://github.com/BEXIS2/Core/issues/1753)
- Fix Sometimes entries in the metadata form lead to the fact that it does not continue when saving [#1766](https://github.com/BEXIS2/Core/issues/1766)
- Fix Changes to files and primary keys break during upload because the primary keys in the cache are not updated [#1769](https://github.com/BEXIS2/Core/issues/1769)


