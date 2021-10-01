BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;
alter table ContentDescriptors alter column URI TYPE text;

-- Insert Data
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '2.14.5',NOW());

COMMIT;