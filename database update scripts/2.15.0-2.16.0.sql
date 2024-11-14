BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '2.16',NOW());

COMMIT;
