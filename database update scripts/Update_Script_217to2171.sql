BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '2.17.1',NOW());

COMMIT;
