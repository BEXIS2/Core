
-- Add new description field and copy description from XML field (extra) to the new description column
Alter Table contentdescriptors add column description text;
Update contentdescriptors set description =  (xpath('/extra/fileDescription/text()', extra))[1]::text