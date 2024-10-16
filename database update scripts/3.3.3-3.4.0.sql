
BEGIN TRANSACTION;
/****************************/
-- TABLE SCHEMA CHANGES 
/****************************/
-- tags

CREATE SEQUENCE IF NOT EXISTS tags_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;



CREATE TABLE IF NOT EXISTS tags
(
    id bigint NOT NULL DEFAULT nextval('tags_id_seq'::regclass),
    nr double precision,
    releasedate timestamp without time zone,
    final boolean,
    doi character varying(255) COLLATE pg_catalog."default",
    CONSTRAINT tags_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER SEQUENCE public.tags_id_seq
    OWNED BY public.tags.id;

ALTER SEQUENCE public.tags_id_seq
    OWNER TO postgres;

ALTER TABLE IF EXISTS public.tags
    OWNER to postgres;
CREATE INDEX IF NOT EXISTS idx_entitytags_id
    ON public.tags USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;
CREATE INDEX IF NOT EXISTS idx_releasedate_timestamp
    ON public.tags USING btree
    (releasedate ASC NULLS LAST)
    TABLESPACE pg_default;


-- datasetversions table
ALTER TABLE IF EXISTS public.datasetversions
    ADD COLUMN show boolean;

ALTER TABLE IF EXISTS public.datasetversions
    ADD COLUMN tagref bigint;

ALTER TABLE IF EXISTS public.datasetversions
    ADD CONSTRAINT fk_9cc024dc FOREIGN KEY (tagref)
    REFERENCES public.tags (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

-- Users
ALTER TABLE IF EXISTS public.users DROP COLUMN IF EXISTS token;

/****************************/
-- DATA CHANGES 
/****************************/

--------------------------
-- FEATURES
--------------------------
-- search api
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Search Api', 
(
    Select id from features where name = 'Data Discovery'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Search Api');

-- tag
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Tag', 
(
    Select id from features where name = 'Data Discovery'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Tag');
-- tag/edit
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, 'Edit of the tags', 'Edit', 
(
    Select id from features where name = 'Tag'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Edit');
-- tag/view
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, 'View of the tags', 'View', 
(
    Select id from features where name = 'Tag'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='View');

--------------------------
-- OPERATIONS
--------------------------
-- api SearchApi
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'Api', 'SearchApi', '*', (Select id from features where name = 'Search Api' AND parentref = (
Select id from features where name = 'Data Discovery') )
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='Api' AND controller='SearchApi');
-- ddm Search
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DDM', 'Search', '*', (Select id from features where name = 'Search' AND parentref = (
Select id from features where name = 'Data Discovery') )
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DDM' AND controller='Search');
-- ddm TagInfo
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DDM', 'TagInfo', '*', (Select id from features where name = 'Edit' AND parentref = (
Select id from features where name = 'Tag') )
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DDM' AND controller='TagInfo');
-- api TaginfoView
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'TagInfoView', '*', (Select id from features where name = 'View' AND parentref = (
Select id from features where name = 'Tag') ) 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='TagInfoView');
-- api TaginfoEdit
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'TagInfoEdit', '*', (Select id from features where name = 'Edit' AND parentref = (
Select id from features where name = 'Tag') ) 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='TagInfoEdit');

--------------------------
-- CONCEPTS
--------------------------
-- Schema.Org
INSERT INTO public.dim_mappingconcepts(
	version, name, description, url, xsd) 
	SELECT 1,'BIOSCHEMA-Dataset', 'This concept is used to provide attributes for a dataset in the system with information based on bioschema. This makes it easier to find entities.', 'https://bioschemas.org/', ''
	WHERE NOT EXISTS (SELECT * FROM public.dim_mappingconcepts WHERE name='BIOSCHEMA-Dataset');

-- schema.org/name
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'name','A descriptive name of the dataset.', 'https://schema.org/name', false, false, (select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'), 'dataset/name'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/name');

-- schema.org/description
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'description','A description  of the dataset.','https://schema.org/description', false, false, (select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'), 'dataset/description'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/description');

-- schema.org/identifier
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'identifier','A identifier of the dataset.', 'https://schema.org/identifier', false, false, (select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'), 'dataset/identifier'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/identifier');

-- schema.org/keywords
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'keywords','Keywords of the dataset.', 'https://schema.org/keywords', false, false, (select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'), 'dataset/keywords'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/keywords');

-- schema.org/url
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'url','A url of the dataset.', 'https://schema.org/url', false, false, (select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'), 'dataset/url'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/url');

-- schema.org/citation
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'citation','A citation of the dataset.', 'https://schema.org/citation', false, false, (select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'), 'dataset/citation'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/citation');

-- schema.org/creator
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'creator','A creator of the dataset.', 'https://schema.org/creator', false, true, (select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'),				 'dataset/creator'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/creator');

-- schema.org/creator/givenName
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
    SELECT 'givenName','Given name. In the U.S., the first name of a Person.', 'https://schema.org/givenName', false, false, 
				(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'), 
				(SELECT id FROM public.dim_mappingkeys WHERE name='creator'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='BIOSCHEMA-Dataset')),
				'dataset/creator/givenName'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/creator/givenName');

-- schema.org/creator/givenName
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
    SELECT 'familyName','Family name. In the U.S., the last name of a Person.', 'https://schema.org/familyName', false, false, 
				(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'), 
				(SELECT id FROM public.dim_mappingkeys WHERE name='creator'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='BIOSCHEMA-Dataset')),
				'dataset/creator/familyName'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/creator/familyName');

-- schema.org/creator/email
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
    SELECT 'email','A email of a creator of the dataset.', 'https://schema.org/email', false, false, 
				(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'), 
				(SELECT id FROM public.dim_mappingkeys WHERE name='creator'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='BIOSCHEMA-Dataset')),
				'dataset/creator/familyName'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/creator/email');

-- schema.org/creator/affiliation
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
    SELECT 'affiliation','An organization that this person is affiliated with. For example, a school/university, a club, or a team.', 'https://schema.org/affiliation', false, false, 
				(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset'), 
				(SELECT id FROM public.dim_mappingkeys WHERE name='creator'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='BIOSCHEMA-Dataset')),
				'dataset/creator/familyName'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='BIOSCHEMA-Dataset') AND xpath='dataset/creator/affiliation');

-- Search
INSERT INTO public.dim_mappingconcepts(
	version, name, description, url, xsd) 
	SELECT 1,'Search', '', '', ''
	WHERE NOT EXISTS (SELECT * FROM public.dim_mappingconcepts WHERE name='Search');

-- search/title
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'title','Placeholder for title', '', false, false, (select id from dim_mappingconcepts where name='Search'), 'dataset/title'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Search') AND xpath='dataset/title');

-- search/description
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'description','Placeholder for description', '', false, false, (select id from dim_mappingconcepts where name='Search'), 'dataset/description'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Search') AND xpath='dataset/description');

-- search/author
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'author','Placeholder for author', '', false, false, (select id from dim_mappingconcepts where name='Search'), 'dataset/author'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Search') AND xpath='dataset/author');

-- search/license
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
    SELECT 'license','Placeholder for author', '', false, false, (select id from dim_mappingconcepts where name='Search'), 'dataset/license'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Search') AND xpath='dataset/license');


-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.4.0',NOW());

commit;
