
-- Add new description field and copy description from XML field (extra) to the new description column
Alter Table contentdescriptors add column description text;
Update contentdescriptors set description =  (xpath('/extra/fileDescription/text()', extra))[1]::text

-- Add field for filz size
Alter Table contentdescriptors add column filesize integer;

-- Add new columns for Versions
Alter Table datasetversions add column versiontype varchar(255);
Alter Table datasetversions add column versionname varchar(255);
Alter Table datasetversions add column versiondescription text;
Alter Table datasetversions add column publicaccess boolean;