$base_dir = Read-Host -Prompt 'Root Directory'
$search = Read-Host -Prompt 'Replace'
$replace_with = Read-Host -Prompt 'With'

Get-ChildItem -Recurse -Include *$search* -Exclude *.Entities,*.NH,*.Services,ModuleTemplate_Renaming* |
	Rename-Item -NewName { $_.Name -replace $search, $replace_with }

Get-ChildItem -Recurse -Include *$search* -Exclude ModuleTemplate_Renaming* |
	Rename-Item -NewName { $_.Name -replace $search, $replace_with }
	
Get-ChildItem -Recurse -Include *.* -Exclude ModuleTemplate_Renaming*,*.Entities,*.NH,*.Services |
    ForEach-Object { (Get-Content $_ | 
    ForEach-Object { $_ -replace "$search", "$replace_with" }) | 
    Set-Content $_ }