[![N|Solid](https://github.com/BExIS2/Documents/blob/master/Images/Logo/Logo_BExIS_rgb_113x28.jpg?raw=true)](http://BExIS2.uni-jena.de/) 
# Module DQM

This repo is a DQM for a BExIS2 module to extend the functionality of the system.

The DQM consists of 1 project and 3 libaries

| Plugin | README |
| ------ | ------ |
| BExIS.Modules.DQM.UI | MVC UI project |
| BExIS.DQM.Entities | Entities associated with the module |
| BExIS.DQM.Orm.NH | Contains the nHibernate Mapping files to connect the tables with the entities in the database |
| BExIS.DQM.Services | In this Libary all managers are deposited, which provide general functionalities for the Entities. eg create, update, delete |


# How to use 

***Precondition:***  Running BExIS2 Instance in visual Studio

1. Download latest version
2. Create a folder into ***BExIS2APP\Console\BExIS.Web.Shell\Areas*** and name it like your prefered ***MODULEID*** (only Characters) and copy the downloaded DQM into this folder
    - ..\Console\BExIS.Web.Shell\Areas\\***MODULEID***\BExIS.Modules.DQM.UI
    - ..\Console\BExIS.Web.Shell\Areas\\***MODULEID***\BExIS.DQM.Entities
    - ..\Console\BExIS.Web.Shell\Areas\\***MODULEID***\BExIS.DQM.Orm.NH
    - ..\Console\BExIS.Web.Shell\Areas\\***MODULEID***\BExIS.DQM.Services
3. Run the ***ModuleDQM_Renaming.ps1*** with Power Shell to replace alle **DQM** with **MODULEID** in files and also filenames.
4. Open the BExIS2 visual studio solution
5. Create a ModuleId folder under the modules folder in the Solution
6. Add the ui project and the libaries to that folder
7. rebuild the BExIS.MODULEID.Orm.NH project and check wether the mapping files are exitsing in the workspace folder
    - ..\Workspace\Modules\\***MODULEID***\Db\\...
8.  Rebuild the web shell 
9.  Add Module in the catalog Workspace\Modules\Modules.Catalog.xml
```
<?xml version="1.0" encoding="utf-8"?>
<Modules>
  ...
  <Module id="vim" status="active" order="8" />
  <Module id="MODULEID" status="pending" order="1" path="BExIS.Modules.MODULEID.UI"/>
</Modules>
```
10. Run application
11. After the application is loaded, the status of the module in the module.catalog.xml is set from pending to inactive. change this to active and the module is ready.


