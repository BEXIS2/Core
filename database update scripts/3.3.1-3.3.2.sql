BEGIN TRANSACTION;

-- Datacite Mapping Concept


-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.3.2',NOW());

commit;