BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

-- Delete SAM Request Controller --
Delete from public.operations where module = 'SAM' and Controller = 'Requests';

-- Create Request Fetaure --
INSERT INTO public.features(
	versionno, extra, description, name, parentref)
	VALUES (1, null, '', 'Requests', (Select id from features where name = 'Data Discovery'));

-- Add Request Controller to Request Fetaure --
INSERT INTO public.operations
(versionno, extra, "module", controller, "action", featureref)
VALUES(1, NULL, 'DDM', 'Requests', '*', (
Select id from features where name = 'Requests' AND parentref = (
Select id from features where name = 'Data Discovery')));

-- change datatype of Content descriptor uri field to text
alter table ContentDescriptors alter column URI TYPE text;

-- set ShowMultimediaData to public
UPDATE public.operations
	SET featureref=null
	Where module = 'MMM' and controller = 'ShowMultimediaData';

UPDATE public.operations
	SET featureref=null
	Where module = 'DDM' and controller = 'Data';

-- Insert Data
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '2.14.5',NOW());

-- set m_comment for datasetversions 
-- if a datasetversion is linked with datatuples, this means the version change belongs data
UPDATE public.datasetversions
	SET m_comment='Data'
	WHERE id in (SELECT dsv.id
FROM public.datasetversions as dsv 
where 
m_comment IS NULL 
and
(Select count(id) from public.datatuples where datasetversionref = dsv.id) > 0);


COMMIT;