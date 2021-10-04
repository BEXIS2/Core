BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

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

COMMIT;