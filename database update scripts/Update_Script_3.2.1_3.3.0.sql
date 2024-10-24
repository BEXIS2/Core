BEGIN TRANSACTION;

-- change Tables

-- rename mappingconcepts to dim_mappingconcepts
ALTER TABLE IF EXISTS mappingconcepts
  RENAME TO dim_mappingconcepts;

-- rename mappingkeys to dim_mappingkeys
ALTER TABLE IF EXISTS mappingkeys
  RENAME TO dim_mappingkeys;

-- brokers
ALTER TABLE IF EXISTS public.dim_brokers
    ADD COLUMN type character varying(255) COLLATE pg_catalog."default";
ALTER TABLE IF EXISTS public.dim_brokers
    ADD COLUMN host character varying(255);

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

-- DataCite Repository Renaming
UPDATE public.dim_repositories
	SET name='datacite'
	WHERE name = 'DataCiteDOI';

-- DataCite Broker Renaming
UPDATE public.dim_brokers
	SET name='datacite'
	WHERE name = 'DataCiteDOI';

UPDATE dim_mappingconcepts
	SET name='datacite'
	WHERE name='DataCiteDoi';

-- DataCite Mapping Updates
-- PublicationYear
UPDATE public.dim_mappingkeys
	SET url='https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/publicationyear/#id1'
	WHERE xpath='data/attributes/publicationYear' and concept = (select id from dim_mappingconcepts where name='datacite');

-- Publisher
--- Mappings
DELETE
	FROM public.dim_mappings
	where sourceref = (SELECT id FROM public.dim_linkelements where elementid = (select id from public.dim_mappingkeys where name='Publisher') and type = 16) OR
	targetref = (SELECT id FROM public.dim_linkelements where elementid = (select id from public.dim_mappingkeys where name='Publisher') and type = 16);

DELETE 
    FROM public.dim_linkelements where name='Publisher' and type = 16;

DELETE 
    FROM public.dim_mappingkeys where name='Publisher' and concept = (select id from dim_mappingconcepts where name='datacite');

INSERT INTO public.dim_mappingkeys (name, url, optional, iscomplex, concept, xpath)
    SELECT 'Publisher', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/publisher/#id1', false, true, (select id from dim_mappingconcepts where name='datacite'), 'data/attributes/publisher'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='datacite') AND xpath='data/attributes/publisher');

INSERT INTO public.dim_mappingkeys (name, optional, iscomplex, concept, parentref, xpath)
    SELECT 'Name', false, false, (select id from dim_mappingconcepts where name='datacite'), (select id from dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='datacite') AND xpath='data/attributes/publisher'), 'data/attributes/publisher/name'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='datacite') AND xpath='data/attributes/publisher/name');

INSERT INTO public.dim_mappingkeys (name, optional, iscomplex, concept, parentref, xpath)
    SELECT 'PublisherIdentifier', true, false, (select id from dim_mappingconcepts where name='datacite'), (select id from dim_mappingkeys where name='Publisher' AND xpath='data/attributes/publisher'), 'data/attributes/publisher/publisherIdentifier'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='datacite') AND xpath='data/attributes/publisher/name');


INSERT INTO public.dim_mappingkeys (name, optional, iscomplex, concept, parentref, xpath)
    SELECT 'SchemeUri', true, false, (select id from dim_mappingconcepts where name='datacite'), (select id from dim_mappingkeys where name='Publisher' AND xpath='data/attributes/publisher'), 'data/attributes/publisher/schemeUri'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='datacite') AND xpath='data/attributes/publisher/schemeUri');


INSERT INTO public.dim_mappingkeys (name, optional, iscomplex, concept, parentref, xpath)
    SELECT 'Lang', true, false, (select id from dim_mappingconcepts where name='datacite'), (select id from dim_mappingkeys where name='Publisher' AND xpath='data/attributes/publisher'), 'data/attributes/publisher/lang'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='datacite') AND xpath='data/attributes/publisher/lang');


-- Title(s)
UPDATE public.dim_mappingkeys
	SET url='https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/title/#id1'
	WHERE xpath='data/attributes/titles' and concept = (select id from dim_mappingconcepts where name='datacite');

UPDATE public.dim_mappingkeys
	SET optional=true
	WHERE xpath='data/attributes/titles/lang' and concept = (select id from dim_mappingconcepts where name='datacite');

UPDATE public.dim_mappingkeys
	SET optional=true
	WHERE xpath='data/attributes/titles/titleType' and concept = (select id from dim_mappingconcepts where name='datacite');

-- Creator(s)
UPDATE public.dim_mappingkeys
	SET url='https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/creator/#id1'
	WHERE xpath='data/attributes/creators' and concept = (select id from dim_mappingconcepts where name='datacite');

UPDATE public.dim_mappingkeys
	SET optional=true
	WHERE xpath='data/attributes/creators/nameType' and concept = (select id from dim_mappingconcepts where name='datacite');

UPDATE public.dim_mappingkeys
	SET optional=true
	WHERE xpath='data/attributes/creators/givenName' and concept = (select id from dim_mappingconcepts where name='datacite');

UPDATE public.dim_mappingkeys
	SET optional=true
	WHERE xpath='data/attributes/creators/familyName' and concept = (select id from dim_mappingconcepts where name='datacite');

-- Subjects(s)
UPDATE public.dim_mappingkeys
	SET url='https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/subject/#id1'
	WHERE xpath='data/attributes/subjects' and concept = (select id from dim_mappingconcepts where name='datacite');

UPDATE public.dim_mappingkeys
	SET optional=true
	WHERE xpath='data/attributes/subjects' and concept = (select id from dim_mappingconcepts where name='datacite');

UPDATE public.dim_mappingkeys
	SET optional=true
	WHERE xpath='data/attributes/subjects/subjectScheme' and concept = (select id from dim_mappingconcepts where name='datacite');

UPDATE public.dim_mappingkeys
	SET optional=true
	WHERE xpath='data/attributes/subjects/schemeURI' and concept = (select id from dim_mappingconcepts where name='datacite');

UPDATE public.dim_mappingkeys
	SET optional=true
	WHERE xpath='data/attributes/subjects/valueURI' and concept = (select id from dim_mappingconcepts where name='datacite');

UPDATE public.dim_mappingkeys
	SET optional=true
	WHERE xpath='data/attributes/subjects/classificationCode' and concept = (select id from dim_mappingconcepts where name='datacite');

-- ********************************************--
-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.3.0',NOW());

commit;
