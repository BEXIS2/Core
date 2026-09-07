BEGIN TRANSACTION;

-- ---------------------------
-- EXTENSIONS 
-- ---------------------------
-- create entity extension
-- entities - publication
INSERT INTO public.entities (
	versionno, 
	extra, 
	entitystoretype, 
	entitytype, 
	name, 
	securable, 
	usemetadata,
	parentref
	)
SELECT 
	1,
	'<extra><modules><module name="name" value="ddm" type="parameter" /></modules></extra>',
	'BExIS.Xml.Helpers.DatasetStore, BExIS.Xml.Helpers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null',
	'BExIS.Dlm.Entities.Data.Dataset, BExIS.Dlm.Entities, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null',
	'Extension',
	true,
	true,
	null
	
WHERE NOT EXISTS (SELECT * FROM public.entities WHERE name='Extension');


-- create or install xsd schema for extension
-- metadatastructure
INSERT INTO public.metadatastructures (
    versionno,
    extra,
    name,
    description,
    xsdfilename,
    xslfilename,
    parentref,
    m_actiontype,
    c_actiontype
)
SELECT
    1,
    '<extra><nodeReferences><nodeRef name="title" value="Metadata/metadata/metadata/title/titleXmlSchemaComplexType" type="xpath" /><nodeRef name="description" value="Metadata/metadata/metadata/description/descriptionXmlSchemaComplexType" type="xpath" /></nodeReferences><entity name="Extension" value="BExIS.Dlm.Entities.Data.Dataset" type="entity" /><convertReferences><convertRef name="Extension" value="MappingFile_extern_BEXIS2-Extension.xsd_to_intern_Extension.xml" type="mappingFileImport" /><convertRef name="Extension" value="MappingFile_intern_Extension_to_extern_BEXIS2-Extension.xsd.xml" type="mappingFileExport" /></convertReferences><parameters><parameter name="active" value="True" type="parameter" /></parameters></extra>',
    'Extension',
    'Extension schema for datasets',
    '',
    '',
    NULL,
    0,
    0
WHERE NOT EXISTS (
    SELECT 1
    FROM public.metadatastructures
    WHERE name = 'Extension'
);

-- MetadataPackage
INSERT INTO public.metadatapackages(
	versionno, extra,  c_actiontype,  m_actiontype,name, description, isenabled
)
Select 
    1, null, 0, 0, 'metadata', '', 'Y'
WHERE NOT EXISTS (
    SELECT * FROM public.metadatapackages WHERE name='metadata'
    );

-- MetadataPackageUsage
INSERT INTO public.metadatapackageusages(
	versionno, extra, mincardinality, maxcardinality, label, description, metadatastructureref, metadatapackageref)
	Select 1, null, 1, 1, 'metadata', '', (SELECT id FROM public.metadatastructures WHERE name = 'Extension'), (SELECT id FROM public.metadatapackages WHERE name = 'metadata')
    WHERE NOT EXISTS (SELECT * FROM public.metadatapackageusages WHERE metadatapackageref = (SELECT id FROM public.metadatapackages WHERE name = 'metadata'));

-- title type
INSERT INTO public.datacontainers(
	discriminator, versionno, c_actiontype, m_actiontype, name, shortname, description, ismultivalue, isbuiltin, scope, entityselectionpredicate, containertype, measurementscale, datatyperef, unitref)
	Select 'SMA', 1, 0, 0, 'titleXmlSchemaComplexType', 'titleXmlSchemaComplexType', 'A name given to the resource.', 'N', 'N', 'David Blaa', '', 0, 5, 1, 1
    WHERE NOT EXISTS (SELECT * FROM public.datacontainers WHERE name = 'titleXmlSchemaComplexType');

-- title usage
INSERT INTO public.metadataattributeusages(
	versionno, extra, mincardinality, maxcardinality, label, description, defaultvalue, fixedvalue, metadatapackageref, metadataattributeref)
	Select 1, null, 0, 0, 'title', 'Title', null, null, (SELECT id FROM public.metadatapackages WHERE name = 'metadata'), (SELECT id FROM public.datacontainers WHERE name = 'titleXmlSchemaComplexType')
    WHERE NOT EXISTS (SELECT * FROM public.metadataattributeusages WHERE metadataattributeref = (SELECT id FROM public.datacontainers WHERE name = 'titleXmlSchemaComplexType'));


-- description type
INSERT INTO public.datacontainers(
	discriminator, versionno, c_actiontype, m_actiontype, name, shortname, description, ismultivalue, isbuiltin, scope, entityselectionpredicate, containertype, measurementscale, datatyperef, unitref)
	Select 'SMA', 1, 0, 0, 'descriptionXmlSchemaComplexType', 'descriptionXmlSchemaComplexType', 'A description of the resource.', 'N', 'N', 'David Blaa', '', 0, 5, 1, 1
    WHERE NOT EXISTS (SELECT * FROM public.datacontainers WHERE name = 'descriptionXmlSchemaComplexType');

-- description usage
INSERT INTO public.metadataattributeusages(
    versionno, extra, mincardinality, maxcardinality, label, description, defaultvalue, fixedvalue, metadatapackageref, metadataattributeref)
    Select 1, null, 1, 1, 'description', 'description', null, null, (SELECT id FROM public.metadatapackages WHERE name = 'metadata'), (SELECT id FROM public.datacontainers WHERE name = 'descriptionXmlSchemaComplexType')
    WHERE NOT EXISTS (SELECT * FROM public.metadataattributeusages WHERE metadataattributeref = (SELECT id FROM public.datacontainers WHERE name = 'descriptionXmlSchemaComplexType'));


-- creator type
INSERT INTO public.datacontainers(
	discriminator, versionno, c_actiontype, m_actiontype, name, shortname, description, ismultivalue, isbuiltin, scope, entityselectionpredicate, containertype, measurementscale, datatyperef, unitref)
	Select'SMA', 1, 0, 0, 'creatorXmlSchemaComplexType', 'creatorXmlSchemaComplexType', 'A creator of the resource.', 'N', 'N', 'David Blaa', '', 0, 5, 1, 1
    WHERE NOT EXISTS (SELECT * FROM public.datacontainers WHERE name = 'creatorXmlSchemaComplexType');

-- creator usage
INSERT INTO public.metadataattributeusages(
    versionno, extra, mincardinality, maxcardinality, label, description, defaultvalue, fixedvalue, metadatapackageref, metadataattributeref)
    SELECT 1, null, 0, 0, 'creator', 'creator', null, null, (SELECT id FROM public.metadatapackages WHERE name = 'metadata'), (SELECT id FROM public.datacontainers WHERE name = 'creatorXmlSchemaComplexType')
    WHERE NOT EXISTS (SELECT * FROM public.metadataattributeusages WHERE metadataattributeref = (SELECT id FROM public.datacontainers WHERE name = 'creatorXmlSchemaComplexType'));

-- date type
INSERT INTO public.datacontainers(
	discriminator, versionno, c_actiontype, m_actiontype, name, shortname, description, ismultivalue, isbuiltin, scope, entityselectionpredicate, containertype, measurementscale, datatyperef, unitref)
	Select 'SMA', 1, 0, 0, 'dateDatatype_date', 'dateDatatype_date', 'A date associated with the resource.', 'N', 'N', 'David Blaa', '', 0, 5, 3, 1
    WHERE NOT EXISTS (SELECT * FROM public.datacontainers WHERE name = 'dateDatatype_date');

-- date usage
INSERT INTO public.metadataattributeusages(
    versionno, extra, mincardinality, maxcardinality, label, description, defaultvalue, fixedvalue, metadatapackageref, metadataattributeref)
    SELECT 1, null, 0, 0, 'date', 'date', null, null, (SELECT id FROM public.metadatapackages WHERE name = 'metadata'), (SELECT id FROM public.datacontainers WHERE name = 'dateDatatype_date')
    WHERE NOT EXISTS (SELECT * FROM public.metadataattributeusages WHERE metadataattributeref = (SELECT id FROM public.datacontainers WHERE name = 'dateDatatype_date'));

-- create mapping for title and description to metadata structure -> System title and description
-- link element title
-- INSERT INTO public.dim_linkelements(
-- 	versionno, extra, name, xpath, issequence, elementid, type, complexity)
-- 	VALUES (1, null, 'title', 'Metadata/metadata/metadata/title/titleXmlSchemaComplexType', false, ((SELECT id FROM public.metadatapackages WHERE name='metadata'), 5, 1);


-- INSERT INTO public.dim_linkelements(
-- 	versionno, extra, name, xpath, issequence, elementid, type, complexity)
-- 	VALUES (1, null, 'description', 'Metadata/metadata/metadata/description/descriptionXmlSchemaComplexType', false, ((SELECT id FROM public.metadatapackages WHERE name='metadata'), 5, 1);





-- ---------------------------
-- FEATURES & OPERATIONS
-- ---------------------------
-- Add only the RC features that are missing in the demo database.
-- This keeps existing permission entries intact.
-- new features Curation, Metadiff, Species Matching
-- curation
INSERT INTO public.features (versionno, extra, description, name, parentref)
SELECT 1, NULL, 'Curation', 'Curation', (Select id from features where name = 'Data Discovery')
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Curation' and parentref = (Select id from features where name = 'Data Discovery'));
-- metadiff
INSERT INTO public.features (versionno, extra, description, name, parentref)
SELECT 1, NULL, 'Metadiff', 'Metadiff', (Select id from features where name = 'Data Discovery')
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Metadiff' and parentref = (Select id from features where name = 'Data Discovery'));
-- Species Matching
INSERT INTO public.features (versionno, extra, description, name, parentref)
SELECT 1, NULL, 'Species matching and taxonomic validation', 'Species Matching', NULL
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Species Matching' and parentref = NULL);





-- OPERATIONS
-- --------------------------

-- metadata edit
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'M', '*', (Select id from features where name = 'Data Creation' AND parentref = (
Select id from features where name = 'Data Collection') )
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='M');


-- groups
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'Groups', '*', (Select id from features where name = 'Groups' AND parentref = (
Select id from features where name = 'Administration') )
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='Groups');
-- users
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'Users', '*', (Select id from features where name = 'Users' AND parentref = (
Select id from features where name = 'Administration') )
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='Users');

-- pum/home
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'PUM', 'Home', '*', NULL
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='PUM' AND controller='Home');
-- smm/home
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'SMM', 'Home', '*', (Select id from features where name = 'Species Matching')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='SMM' AND controller='Home');
-- smm/Species
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'SMM', 'Species', '*', (Select id from features where name = 'Species Matching')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='SMM' AND controller='Species');
-- smm/DatasetsOverview
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'SMM', 'DatasetsOverview', '*', (Select id from features where name = 'Species Matching')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='SMM' AND controller='DatasetsOverview');
-- smm/Headermapping
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'SMM', 'Headermapping', '*', (Select id from features where name = 'Species Matching')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='SMM' AND controller='Headermapping');
-- smm/Matchingresult
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'SMM', 'Matchingresult', '*', (Select id from features where name = 'Species Matching')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='SMM' AND controller='Matchingresult');
-- smm/ProgressOverview
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'SMM', 'ProgressOverview', '*', (Select id from features where name = 'Species Matching')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='SMM' AND controller='ProgressOverview');
-- smm/TailorView
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'SMM', 'TailorView', '*', (Select id from features where name = 'Species Matching')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='SMM' AND controller='TailorView');



-- ---------------------------
-- Species Matching	Result Table
CREATE SEQUENCE IF NOT EXISTS public.smm_species_matching_result_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

CREATE TABLE IF NOT EXISTS public.smm_species_matching_result
(
    id bigint NOT NULL DEFAULT nextval('smm_species_matching_result_id_seq'::regclass),
    original_name character varying(255) COLLATE pg_catalog."default",
    edited_name character varying(255) COLLATE pg_catalog."default",
    matched_name character varying(255) COLLATE pg_catalog."default",
    status character varying(255) COLLATE pg_catalog."default",
    match_type character varying(255) COLLATE pg_catalog."default",
    match_rank character varying(255) COLLATE pg_catalog."default",
    match_id character varying(255) COLLATE pg_catalog."default",
    match_authorship character varying(255) COLLATE pg_catalog."default",
    accepted_scientific_name character varying(255) COLLATE pg_catalog."default",
    accepted_id character varying(255) COLLATE pg_catalog."default",
    accepted_authorship character varying(255) COLLATE pg_catalog."default",
    taxon_kingdom character varying(255) COLLATE pg_catalog."default",
    taxon_phylum character varying(255) COLLATE pg_catalog."default",
    taxon_class character varying(255) COLLATE pg_catalog."default",
    taxon_order character varying(255) COLLATE pg_catalog."default",
    taxon_family character varying(255) COLLATE pg_catalog."default",
    taxon_genus character varying(255) COLLATE pg_catalog."default",
    timestamp_match timestamp without time zone,
    match_source character varying(255) COLLATE pg_catalog."default",
    match_source_version character varying(255) COLLATE pg_catalog."default",
    confirmed_by_user boolean,
    step_id bigint,
    dataset_version_id bigint,
    datasetref bigint,
    CONSTRAINT smm_species_matching_result_pkey PRIMARY KEY (id),
    CONSTRAINT fk_bb39d87a FOREIGN KEY (datasetref)
        REFERENCES public.datasets (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
);

ALTER SEQUENCE public.smm_species_matching_result_id_seq
    OWNED BY public.smm_species_matching_result.id;

ALTER SEQUENCE IF EXISTS public.smm_species_matching_result_id_seq
    OWNER TO postgres;


-- ENTITY LINKS				
ALTER TABLE IF EXISTS public.entitytemplates
    ADD COLUMN hasextension boolean DEFAULT false;
ALTER TABLE IF EXISTS public.entitytemplates
    ADD COLUMN jsonextensionlist character varying(255) default '' COLLATE pg_catalog."default";
	
ALTER TABLE IF EXISTS public.entityreferences
    ADD COLUMN category character varying(255) COLLATE pg_catalog."default";
ALTER TABLE IF EXISTS public.entityreferences
    ADD COLUMN linktype character varying(255) COLLATE pg_catalog."default";

-- ---------------------------
-- DATASETVERSION TITLE COLUMN UPDATE
ALTER TABLE public.datasetversions
    ALTER COLUMN title TYPE character varying COLLATE pg_catalog."default";




-- ---------------------------
-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '5.0.0',NOW());

commit;

--End TRANSACTION;

