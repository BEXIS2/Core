BEGIN TRANSACTION;

-- change all doi entries from 10.21373/vmy64f -> https://doi.org/10.21373/vmy64f
UPDATE dim_publications
SET externallink = 'https://doi.org/' || externallink
WHERE
externallinktype='DOI' and
externallink NOT LIKE 'http%';


ALTER TABLE public.dim_linkelements
    ALTER COLUMN xpath TYPE text COLLATE pg_catalog."default";

-- curation

CREATE SEQUENCE IF NOT EXISTS public.curationentries_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;


CREATE SEQUENCE IF NOT EXISTS public.curationnotes_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;


CREATE TABLE IF NOT EXISTS public.curationentries
(
    id bigint NOT NULL DEFAULT nextval('curationentries_id_seq'::regclass),
    versionno integer NOT NULL,
    extra xml,
    topic text COLLATE pg_catalog."default",
    type integer,
    name character varying(255) COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    solution character varying(255) COLLATE pg_catalog."default",
    "position" integer,
    source character varying(255) COLLATE pg_catalog."default",
    creationdate timestamp without time zone,
    userisdone boolean,
    isapproved boolean,
    lastchangedatetime_user timestamp without time zone,
    lastchangedatetime_curator timestamp without time zone,
    datasetref bigint,
    userref bigint,
    CONSTRAINT curationentries_pkey PRIMARY KEY (id),
    CONSTRAINT fk_183adac3 FOREIGN KEY (userref)
        REFERENCES public.users (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_5d430561 FOREIGN KEY (datasetref)
        REFERENCES public.datasets (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

CREATE TABLE IF NOT EXISTS public.curationnotes
(
    id bigint NOT NULL DEFAULT nextval('curationnotes_id_seq'::regclass),
    versionno integer NOT NULL,
    extra xml,
    usertype integer,
    creationdate timestamp without time zone,
    comment text COLLATE pg_catalog."default",
    userref bigint,
    curationentryref bigint,
    CONSTRAINT curationnotes_pkey PRIMARY KEY (id),
    CONSTRAINT fk_2b898659 FOREIGN KEY (curationentryref)
        REFERENCES public.curationentries (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_5c66acff FOREIGN KEY (userref)
        REFERENCES public.users (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.curationnotes
    OWNER to postgres;
CREATE INDEX IF NOT EXISTS idx_curationnote_id
    ON public.curationnotes USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;



ALTER SEQUENCE public.curationentries_id_seq
    OWNED BY public.curationentries.id;

ALTER SEQUENCE public.curationentries_id_seq
    OWNER TO postgres;


ALTER TABLE IF EXISTS public.curationentries
    OWNER to postgres;

CREATE INDEX IF NOT EXISTS idx_curationentry_id
    ON public.curationentries USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;


ALTER SEQUENCE public.curationnotes_id_seq
    OWNED BY public.curationnotes.id;

ALTER SEQUENCE public.curationnotes_id_seq
    OWNER TO postgres;


-- concept updates about curation

--insert citation concepts for all provided citation formats

INSERT INTO public.dim_mappingconcepts(
version, name, description, url, xsd) 
SELECT 1,'Citation_RIS', 'The concept is needed to create a citation string in RIS format', '', ''
WHERE NOT EXISTS (SELECT * FROM public.dim_mappingconcepts WHERE name='Citation_RIS');

INSERT INTO public.dim_mappingconcepts(
version, name, description, url, xsd) 
SELECT 1,'Citation_Bibtex', 'The concept is needed to create a citation string in Bibtex format', '', ''
WHERE NOT EXISTS (SELECT * FROM public.dim_mappingconcepts WHERE name='Citation_Bibtex');

INSERT INTO public.dim_mappingconcepts(
version, name, description, url, xsd) 
SELECT 1,'Citation_Text', 'The concept is needed to create a citation string in Text format', '', ''
WHERE NOT EXISTS (SELECT * FROM public.dim_mappingconcepts WHERE name='Citation_Text');

INSERT INTO public.dim_mappingconcepts(
version, name, description, url, xsd) 
SELECT 1,'Citation_APA', 'The concept is needed to create a citation string in APA format', '', ''
WHERE NOT EXISTS (SELECT * FROM public.dim_mappingconcepts WHERE name='Citation_APA');


--title
INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Title','Title of citation string.', '', false, false, (select id from dim_mappingconcepts where name='Citation_RIS'), 'data/title'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/title');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Title','Title of citation string.', '', false, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/title'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/title');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Title','Title of citation string.', '', false, false, (select id from dim_mappingconcepts where name='Citation_Text'), 'data/title'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/title');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Title','Title of citation string.', '', false, false, (select id from dim_mappingconcepts where name='Citation_APA'), 'data/title'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/title');

--version

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Version','Dataset version of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_RIS'), 'data/version'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/version');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Version','Dataset version of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/version'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/version');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Version','Dataset version of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Text'), 'data/version'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/version');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Version','Dataset version of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_APA'), 'data/version'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/version');

--year

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Year','Dataset publish year of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_RIS'), 'data/year'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/year');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Year','Dataset publish year of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/year'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/year');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Year','Dataset publish year of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_APA'), 'data/year'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/year');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Year','Dataset publish year of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Text'), 'data/year'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/year');

-- entityType

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'entityType','Dataset entity type.', '', true, false, (select id from dim_mappingconcepts where name='Citation_RIS'), 'data/entityType'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/entityType');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'entityType','Dataset entity type.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/entityType'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/entityType');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'entityType','Dataset entity type.', '', true, false, (select id from dim_mappingconcepts where name='Citation_APA'), 'data/entityType'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/entityType');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'entityType','Dataset entity type.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Text'), 'data/entityType'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/entityType');

--entryType


INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'entryType','Dataset entry type.', '', false, false, (select id from dim_mappingconcepts where name='Citation_RIS'), 'data/entryType'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/entryType');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'entryType','Dataset entry type.', '', false, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/entryType'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/entryType');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'entryType','Dataset entry type.', '', true, false, (select id from dim_mappingconcepts where name='Citation_APA'), 'data/entryType'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/entryType');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'entryType','Dataset entry type.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Text'), 'data/entryType'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/entryType');

--publisher


INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'publisher','Dataset publisher.', '', false, false, (select id from dim_mappingconcepts where name='Citation_RIS'), 'data/publisher'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/publisher');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'publisher','Dataset publisher.', '', false, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/publisher'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/publisher');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'publisher','Dataset publisher.', '', false, false, (select id from dim_mappingconcepts where name='Citation_APA'), 'data/publisher'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/publisher');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'publisher','Dataset publisher.', '', false, false, (select id from dim_mappingconcepts where name='Citation_Text'), 'data/publisher'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/publisher');

--keyword (only bibtex)

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'keyword','keyword', '', false, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/keyword'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/keyword');

--note

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'note','note', '', true, false, (select id from dim_mappingconcepts where name='Citation_RIS'), 'data/note'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/note');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'note','note', '', true, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/note'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/note');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'note','note', '', true, false, (select id from dim_mappingconcepts where name='Citation_APA'), 'data/note'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/note');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'note','note', '', true, false, (select id from dim_mappingconcepts where name='Citation_Text'), 'data/note'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/note');

--doi

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Doi','Dataset DOI of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_RIS'), 'data/doi'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/doi');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Doi','Dataset DOI of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/doi'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/doi');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Doi','Dataset DOI of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_APA'), 'data/doi'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/doi');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Doi','Dataset DOI of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Text'), 'data/doi'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/doi');

--projects

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Projects','Dataset projects of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_RIS'), 'data/projects'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/projects');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'Project','Dataset project of citation string.', '', true, false, 
			(select id from dim_mappingconcepts where name='Citation_RIS'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='Projects'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation_RIS')),
			'data/projects/project'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/projects/project');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Projects','Dataset projects of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/projects'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/projects');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'Project','Dataset project of citation string.', '', true, false, 
			(select id from dim_mappingconcepts where name='Citation_Bibtex'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='Projects'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation_Bibtex')),
			'data/projects/project'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/projects/project');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Projects','Dataset projects of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_APA'), 'data/projects'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/projects');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'Project','Dataset project of citation string.', '', true, false, 
			(select id from dim_mappingconcepts where name='Citation_APA'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='Projects'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation_APA')),
			'data/projects/project'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/projects/project');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Projects','Dataset projects of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation_Text'), 'data/projects'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/projects');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'Project','Dataset project of citation string.', '', true, false, 
			(select id from dim_mappingconcepts where name='Citation_Text'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='Projects'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation_Text')),
			'data/projects/project'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/projects/project');

--authorNames

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'AuthorNames','Dataset author names of citation string.', '', false, false, (select id from dim_mappingconcepts where name='Citation_RIS'), 'data/authorNames'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/authorNames');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'AuthorName','Dataset author name of citation string.', '', false, false, 
			(select id from dim_mappingconcepts where name='Citation_RIS'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='AuthorNames'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation_RIS')),
			'data/authorNames/authorName'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_RIS') AND xpath='data/authorNames/authorName');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'AuthorNames','Dataset author names of citation string.', '', false, false, (select id from dim_mappingconcepts where name='Citation_Bibtex'), 'data/authorNames'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/authorNames');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'AuthorName','Dataset author name of citation string.', '', false, false, 
			(select id from dim_mappingconcepts where name='Citation_Bibtex'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='AuthorNames'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation_Bibtex')),
			'data/authorNames/authorName'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Bibtex') AND xpath='data/authorNames/authorName');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'AuthorNames','Dataset author names of citation string.', '', false, false, (select id from dim_mappingconcepts where name='Citation_APA'), 'data/authorNames'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/authorNames');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'AuthorName','Dataset author name of citation string.', '', false, false, 
			(select id from dim_mappingconcepts where name='Citation_APA'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='AuthorNames'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation_APA')),
			'data/authorNames/authorName'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_APA') AND xpath='data/authorNames/authorName');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'AuthorNames','Dataset author names of citation string.', '', false, false, (select id from dim_mappingconcepts where name='Citation_Text'), 'data/authorNames'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/authorNames');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'AuthorName','Dataset author name of citation string.', '', false, false, 
			(select id from dim_mappingconcepts where name='Citation_Text'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='AuthorNames'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation_Text')),
			'data/authorNames/authorName'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation_Text') AND xpath='data/authorNames/authorName');




--- delete old citanion concept

-- select all child mappings
DELETE from dim_mappings where 
(sourceRef in (select id from dim_linkelements where type in (16) and elementid in (select id from dim_mappingkeys where concept = (select id from dim_mappingconcepts where name = 'Citation'))) or
targetRef in (select id from dim_linkelements where type in (16) and elementid in (select id from dim_mappingkeys where concept = (select id from dim_mappingconcepts where name = 'Citation'))) ) and
level = 2;

DELETE from dim_mappings where 
(sourceRef in (select id from dim_linkelements where type in (16) and elementid in (select id from dim_mappingkeys where concept = (select id from dim_mappingconcepts where name = 'Citation'))) or
targetRef in (select id from dim_linkelements where type in (16) and elementid in (select id from dim_mappingkeys where concept = (select id from dim_mappingconcepts where name = 'Citation'))) ) and
level = 1;

DELETE from dim_mappings where sourceRef = (select id from dim_linkelements where type = 15 and name = 'Citation') or targetRef = (select id from dim_linkelements where type = 15 and name = 'Citation'); 


-- delete link elements

DELETE from dim_linkelements where type in (16) and elementid in (select id from dim_mappingkeys where concept = (select id from dim_mappingconcepts where name = 'Citation'));
-- select concepte element
DELETE from dim_linkelements where type = 15 and name = 'Citation';

Delete from dim_mappingkeys where concept = (Select id from dim_mappingconcepts where name = 'Citation');
Delete from dim_mappingconcepts where name = 'Citation';


-- operation für curation
-- add docs operation
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'Api', 'CurationEntry', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='Api' AND controller='CurationEntry');


-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.2.0',NOW());

commit;
