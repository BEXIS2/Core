BEGIN TRANSACTION;
-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.0.1',NOW());

commit;