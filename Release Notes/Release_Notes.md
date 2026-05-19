
# BEXIS2 Release Notes - Version 4.3.1
> This release includes minor bug fixes related to dataset copying, downloading, and settings.

### Workspace changes:
- Workspace changes: [4.3.0..4.3.1](https://github.com/BEXIS2/Workspace/compare/4.3.0..4.3.1)


### Database Update(s):
- Update script from version 4.3.0 to 4.3.1: [Update_Script_4.3.0_4.3.1.sql](https://github.com/BEXIS2/Core/blob/rc/database%20update%20scripts/4.3.0-4.3.1.sql)

### Web.config changes
- ...

### Configuration
- General Settings: Security Settings


# Bugfixes and enhancements
### Bugfixes and Enhancements
#### Settings
- **Enable JWT and SMTP configurations**: Added 'isActive' property to JWT and SMTP configurations.

#### Data
- **Dataset Download**: Package with big files are not possible. ([#2460](https://github.com/BEXIS2/Core/issues/2460))
- **Dataset Copy**: Mapped system keys in the metadata are overwritten in the original dataset. ([#2462](https://github.com/BEXIS2/Core/issues/2462))




