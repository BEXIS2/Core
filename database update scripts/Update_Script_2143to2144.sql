BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

ALTER TABLE REQUESTS ALTER COLUMN intention TYPE text;
ALTER TABLE Decisions ALTER COLUMN reason TYPE text;
alter table datasetversions alter column changedescription TYPE text;

-- Insert Data
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '2.14.4',NOW());

COMMIT;