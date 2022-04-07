
-- Add new description field and copy description from XML field (extra) to the new description column
Alter Table contentdescriptors add column description text;
Update contentdescriptors set description =  (xpath('/extra/fileDescription/text()', extra))[1]::text;

-- Add field for filz size
Alter Table contentdescriptors add column filesize integer;

-- Add new columns for Versions
Alter Table datasetversions add column versiontype varchar(255);
Alter Table datasetversions add column versionname varchar(255);
Alter Table datasetversions add column versiondescription text;
Alter Table datasetversions add column publicaccess boolean;
Alter Table datasetversions add column publicaccessdate timestamp without time zone;

-- Change/Add splitted requests
INSERT INTO public.features(
	versionno, extra, description, name, parentref)
	VALUES (1, null, 'Allow to send requests', 'Requests Send', (Select id from features where name = 'Data Discovery'));

INSERT INTO public.operations
(versionno, extra, "module", controller, "action", featureref)
VALUES(1, NULL, 'DDM', 'RequestsSend', '*', (
Select id from features where name = 'Requests Send' AND parentref = (
Select id from features where name = 'Data Discovery')));

Update features set name = 'Requests Manage' where name = 'Requests';
Update features set description = 'Manange requests by user' where name = 'Requests Manage';

update operations set controller = 'RequestsManage' where controller = 'Requests';

-- Set old permission settings from requests manange to send as well? 

-- Add Creation Date to entitypermissions
Alter TABLE entitypermissions add column creationdate timestamp without time zone;

-- Add Entry for api/token
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'Token', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='Token');

-- Add Entry for datacite doi inside dim
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DIM', 'DataCiteDoi', '*', (Select id from features where name = 'Data Dissemination')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DIM' AND controller='DataCiteDoi');
