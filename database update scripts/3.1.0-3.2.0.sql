BEGIN TRANSACTION;

-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.2.0',NOW());

commit;