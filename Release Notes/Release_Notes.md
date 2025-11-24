
# BEXIS 2 Release Notes - Version 4.2.0
> ....

### Workspace changes:
- Workspace changes: [4.1.0..4.2.0](https://github.com/BEXIS2/Workspace/compare/4.1.0..4.2.0)

### Database Update(s):
- Update script from version 4.1.0 to 4.2.0: [Update_Script_4.1.0_4.2.0.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/4.1.0-4.2.0.sql)

### New Settings:
- **...**

# Bugfixes and enhancements
### New features
- **Species Mapping Module (SMM)**: Create Species Mapping Module. ([#2201](https://github.com/BEXIS2/Core/issues/2201))

### UI / UX
- **Documentation**: Change edit and add save icon. ([#2221](https://github.com/BEXIS2/Core/issues/2221))
- **Search UI**: Small improvements. ([#2232](https://github.com/BEXIS2/Core/issues/2232))
- **Tag Management**: UX improvements. ([#2198](https://github.com/BEXIS2/Core/issues/2198))
- **Dataset View**: Missing icon for unstructured download. ([#2199](https://github.com/BEXIS2/Core/issues/2199))
- **Create Data Structure**: Extend info text shown in the create data structure flow. ([#2248](https://github.com/BEXIS2/Core/issues/2248))

### Authentication & login
- **External providers**: Additional text about potential external login providers. ([#2210](https://github.com/BEXIS2/Core/issues/2210))

### Public data & downloads
- **Terms**: Add terms and conditions to the download package. ([#855](https://github.com/BEXIS2/Core/issues/855))
- **Download scope**: Only the whole package should be downloadable. ([#778](https://github.com/BEXIS2/Core/issues/778))
- **Agreement checkbox**: Optional checkbox that data agreement is accepted before download. ([#779](https://github.com/BEXIS2/Core/issues/779))
- **Download info**: Extend download information shown to users. ([#2147](https://github.com/BEXIS2/Core/issues/2147))

### Dataset management & safety
- **Purge confirmation**: Purge dataset action should require confirmation. ([#2239](https://github.com/BEXIS2/Core/issues/2239))

### Metadata, mapping & structure
- **Complex types**: Random reuse of metadata elements with the same name in complex types. ([#2189](https://github.com/BEXIS2/Core/issues/2189))
- **Concept Output**: Multi complex → one complex conversion does not work. ([#2224](https://github.com/BEXIS2/Core/issues/2224))
- **Structure Mapping**: Allow mapping multiple default sources to one target. ([#2227](https://github.com/BEXIS2/Core/issues/2227))

### Upload & parsing
- **DateTime parsing**: DateTime like "24.12.2025 17:23:00" is not working. ([#2231](https://github.com/BEXIS2/Core/issues/2231))