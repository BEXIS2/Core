BEGIN TRANSACTION;


ALTER TABLE public.datasetversions
    ALTER COLUMN title TYPE character varying COLLATE pg_catalog."default";

-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '5.0.0',NOW());

commit;
