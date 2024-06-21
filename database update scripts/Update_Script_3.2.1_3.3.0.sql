BEGIN TRANSACTION;

-- change Tables
-- ********************************************--
-- brokers

ALTER TABLE IF EXISTS public.dim_brokers
    ADD COLUMN type character varying(255) COLLATE pg_catalog."default";

ALTER TABLE IF EXISTS public.dim_brokers
    ADD COLUMN repositoryref bigint;
ALTER TABLE IF EXISTS public.dim_brokers
    ADD CONSTRAINT fk_307cfbc2 FOREIGN KEY (repositoryref)
    REFERENCES public.dim_repositories (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

-- repository
ALTER TABLE IF EXISTS public.dim_repositories DROP COLUMN IF EXISTS brokerref;
ALTER TABLE IF EXISTS public.dim_repositories DROP CONSTRAINT IF EXISTS fk_579e32ac;

-- Publications
ALTER TABLE IF EXISTS public.dim_publications
    ADD COLUMN response text COLLATE pg_catalog."default";

-- Parameter
DROP TABLE IF EXISTS public.parameters CASCADE;

-- change Data
-- ********************************************--
-- brokers - repos
-- GBFIO Collections - Collections (1)
Update dim_brokers 
SET type='Collections',
repositoryref = (select id from dim_repositories where name = 'Collections')
where name = 'GFBIO';

-- GBFIO Observations - Pangaea (2)
INSERT INTO public.dim_brokers(
	versionno, name, primarydataformat,  type, repositoryref, server,username,password,metadataformat,link)
	VALUES (1, 'GFBIO', 'text/csv', 'Observations', (select id from dim_repositories where name = 'Pangaea'),'','','','','');

-- GBFIO Occurrence - GBFI (5)
Update dim_brokers 
SET type='Occurrence',
repositoryref = (select id from dim_repositories where name = 'GBIF')
where name = 'GBIF';

-- GBFIO SamplingEvent - GBFI (5)
INSERT INTO public.dim_brokers(
	versionno, name, primarydataformat,  type, repositoryref, server,username,password,metadataformat,link)
	VALUES (1, 'GBIF', 'text/csv', 'SamplingEvent', (select id from dim_repositories where name = 'GBIF'),'','','','','');

-- pensoft
Update dim_brokers 
SET type = '',
repositoryref = (select id from dim_repositories where name = 'Pensoft')
where name = 'Pensoft';

-- DOI
Update dim_brokers 
SET type = '',
repositoryref = (select id from dim_repositories where name = 'DataCiteDOI')
where name = 'DataCiteDOI';


-- ********************************************--
-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.3.0',NOW());

commit;