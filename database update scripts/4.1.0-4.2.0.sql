BEGIN TRANSACTION;

ALTER TABLE public.dim_linkelements
    ALTER COLUMN xpath TYPE text COLLATE pg_catalog."default";

    -- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.2.0',NOW());

commit;
