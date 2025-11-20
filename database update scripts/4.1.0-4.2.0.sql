BEGIN TRANSACTION;

-- change all doi entries from 10.21373/vmy64f -> https://doi.org/10.21373/vmy64f
UPDATE dim_publications
SET externallink = 'https://doi.org/' || externallink
WHERE
externallinktype='DOI' and
externallink NOT LIKE 'http%';


ALTER TABLE public.dim_linkelements
    ALTER COLUMN xpath TYPE text COLLATE pg_catalog."default";

    -- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.2.0',NOW());

commit;
