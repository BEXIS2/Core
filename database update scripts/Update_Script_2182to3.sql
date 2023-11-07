BEGIN;

/**********************************************************************************************/
/********************** SCHEMA CHANGES BEFORE DATA ********************************************/
/**********************************************************************************************/

CREATE TABLE IF NOT EXISTS public.entitytemplates (
    id bigint NOT NULL, 
    versionno integer NOT NULL,
    extra xml,
    name character varying(255) COLLATE pg_catalog."default",
    description character varying(255) COLLATE pg_catalog."default",
    metadatainvalidsavemode boolean,
    hasdatastructure boolean,
    jsondatastructurelist character varying(255) COLLATE pg_catalog."default",
    jsonallowedfiletypes character varying(255) COLLATE pg_catalog."default",
    jsondisabledhooks character varying(255) COLLATE pg_catalog."default",
    jsonnotificationgroups character varying(255) COLLATE pg_catalog."default",
    jsonpermissiongroups character varying(255) COLLATE pg_catalog."default",
    jsonmetadatafields character varying(255) COLLATE pg_catalog."default",
    entityref bigint NOT NULL,
    metadatastructureref bigint NOT NULL,
    CONSTRAINT entitytemplates_pkey PRIMARY KEY (id),
    CONSTRAINT fk_a2c5bf76 FOREIGN KEY (entityref) REFERENCES public.entities (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT fk_cf532bcb FOREIGN KEY (metadatastructureref) REFERENCES public.metadatastructures (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION
) TABLESPACE pg_default;

ALTER TABLE
    IF EXISTS public.entitytemplates OWNER to postgres;


CREATE TABLE IF NOT EXISTS public.variable_constraints (
    constraintref bigint NOT NULL,
    variableref bigint NOT NULL,
    CONSTRAINT variable_constraints_pkey PRIMARY KEY (variableref, constraintref),
    CONSTRAINT fk_17300cc1 FOREIGN KEY (variableref) REFERENCES public.variables (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT fk_8ba3b117 FOREIGN KEY (constraintref) REFERENCES public.constraints (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION
) TABLESPACE pg_default;

ALTER TABLE
    IF EXISTS public.variable_constraints OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.meanings (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    name character varying(255) COLLATE pg_catalog."default",
    shortname character varying(255) COLLATE pg_catalog."default",
    description character varying(255) COLLATE pg_catalog."default",
    approved integer,
    selectable integer,
    CONSTRAINT meanings_pkey PRIMARY KEY (id)
) TABLESPACE pg_default;

ALTER TABLE
    IF EXISTS public.meanings OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.externallink (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    uri character varying(255) COLLATE pg_catalog."default",
    name character varying(255) COLLATE pg_catalog."default",
    type character varying(255) COLLATE pg_catalog."default",
    externallinkref bigint,
    CONSTRAINT externallink_pkey PRIMARY KEY (id),
    CONSTRAINT fk_6978e301 FOREIGN KEY (externallinkref) REFERENCES public.meanings (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION
) TABLESPACE pg_default;

ALTER TABLE
    IF EXISTS public.externallink OWNER to postgres;



CREATE TABLE IF NOT EXISTS public.meaning_meaning (
    meaningsparentref bigint NOT NULL,
    meaningschildrenref bigint NOT NULL,
    CONSTRAINT meaning_meaning_pkey PRIMARY KEY (meaningsparentref, meaningschildrenref),
    CONSTRAINT fk_516f2fd8 FOREIGN KEY (meaningschildrenref) REFERENCES public.meanings (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT fk_d0cc9d49 FOREIGN KEY (meaningsparentref) REFERENCES public.meanings (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION
) TABLESPACE pg_default;

ALTER TABLE
    IF EXISTS public.meaning_meaning OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.meanings_variables (
    variableref bigint NOT NULL,
    meaningref bigint NOT NULL,
    CONSTRAINT meanings_variables_pkey PRIMARY KEY (meaningref, variableref),
    CONSTRAINT fk_a7c43e6c FOREIGN KEY (variableref) REFERENCES public.variables (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT fk_c5170336 FOREIGN KEY (meaningref) REFERENCES public.meanings (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION
) TABLESPACE pg_default;

ALTER TABLE
    IF EXISTS public.meanings_variables OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.accessrules (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    securitykey character varying(255) COLLATE pg_catalog."default",
    securityobjecttype integer,
    rulebody character varying(255) COLLATE pg_catalog."default",
    displayname character varying(255) COLLATE pg_catalog."default",
    parentref bigint,
    CONSTRAINT accessrules_pkey PRIMARY KEY (id),
    CONSTRAINT fk_accessruleentitys_parentaccessruleentity FOREIGN KEY (parentref) REFERENCES public.accessrules (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION
) TABLESPACE pg_default;

ALTER TABLE
    IF EXISTS public.accessrules OWNER to postgres;


ALTER TABLE
    IF EXISTS public.variables
ALTER COLUMN
    datastructureref DROP NOT NULL;

ALTER TABLE
    IF EXISTS public.variables
ADD
    COLUMN approved boolean;

ALTER TABLE
    IF EXISTS public.variables
ADD
    COLUMN datatyperef bigint;

ALTER TABLE
    IF EXISTS public.variables
ADD
    COLUMN displaypatternid integer;

ALTER TABLE
    IF EXISTS public.variables
ADD
    COLUMN iskey boolean;

ALTER TABLE
    IF EXISTS public.variables
ADD
    COLUMN variablestype character varying(255) COLLATE pg_catalog."default";

ALTER TABLE
    IF EXISTS public.variables
ADD
    COLUMN vartemplateref bigint;

ALTER TABLE
    IF EXISTS public.variables DROP CONSTRAINT IF EXISTS fk1a5c9d114fe09c0d;

ALTER TABLE
    IF EXISTS public.variables DROP CONSTRAINT IF EXISTS fk1a5c9d117cadd8dc;

ALTER TABLE
    IF EXISTS public.variables DROP CONSTRAINT IF EXISTS fk1a5c9d1190fe2254;

ALTER TABLE
    IF EXISTS public.variables
ADD
    CONSTRAINT fk_2abad2e6 FOREIGN KEY (vartemplateref) REFERENCES public.variables (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE
    IF EXISTS public.variables
ADD
    CONSTRAINT fk_35a1bc0a FOREIGN KEY (unitref) REFERENCES public.units (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE
    IF EXISTS public.variables
ADD
    CONSTRAINT fk_7f919d88 FOREIGN KEY (datastructureref) REFERENCES public.datastructures (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE
    IF EXISTS public.variables
ADD
    CONSTRAINT fk_990c0cd5 FOREIGN KEY (datatyperef) REFERENCES public.datatypes (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

DROP INDEX IF EXISTS public.idx_dataattributeref_variables;

DROP INDEX IF EXISTS public.idx_datastructureref_variables;

ALTER TABLE
    IF EXISTS public.datasets
ALTER COLUMN
    datastructureref DROP NOT NULL;

ALTER TABLE
    IF EXISTS public.datasets
ADD
    COLUMN entitytemplateref bigint;

ALTER TABLE
    IF EXISTS public.datasets
ADD
    CONSTRAINT fk_d593bd3c FOREIGN KEY (entitytemplateref) REFERENCES public.entitytemplates (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE
    IF EXISTS public.datacontainers DROP COLUMN IF EXISTS classifierref;

ALTER TABLE
    IF EXISTS public.datacontainers DROP CONSTRAINT IF EXISTS fk3beb06a45a7e030;

ALTER TABLE
    IF EXISTS public.datacontainers DROP CONSTRAINT IF EXISTS fk3beb06a7cadd8dc;

ALTER TABLE
    IF EXISTS public.datacontainers DROP CONSTRAINT IF EXISTS fk3beb06ac49d0116;

ALTER TABLE
    IF EXISTS public.datacontainers DROP CONSTRAINT IF EXISTS fk3beb06af68222;

ALTER TABLE
    IF EXISTS public.datacontainers
ADD
    CONSTRAINT fk_5e17ebf9 FOREIGN KEY (datatyperef) REFERENCES public.datatypes (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE
    IF EXISTS public.datacontainers
ADD
    CONSTRAINT fk_7330dbef FOREIGN KEY (methodologyref) REFERENCES public.methodologies (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE
    IF EXISTS public.datacontainers
ADD
    CONSTRAINT fk_f9a7e19e FOREIGN KEY (unitref) REFERENCES public.units (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION;

CREATE SEQUENCE IF NOT EXISTS public.meanings_id_seq INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 OWNED BY meanings.id;

ALTER SEQUENCE public.meanings_id_seq OWNER TO postgres;

CREATE SEQUENCE IF NOT EXISTS public.externallink_id_seq INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 OWNED BY externallink.id;

ALTER SEQUENCE public.externallink_id_seq OWNER TO postgres;

CREATE SEQUENCE IF NOT EXISTS public.accessrules_id_seq INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 OWNED BY accessrules.id;

ALTER SEQUENCE public.accessrules_id_seq OWNER TO postgres;

CREATE SEQUENCE IF NOT EXISTS public.entitytemplates_id_seq INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 OWNED BY entitytemplates.id;

ALTER SEQUENCE public.entitytemplates_id_seq OWNER TO postgres;


/**********************************************************************************************/
/********************** DATA CHANGES  *********************************************************/
/**********************************************************************************************/

/*Update features*/
/*Settings*/
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Settings', null
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Settings');
/*Entity Template Management*/
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Entity Template Management', 
(
    Select id from features where name = 'Data Collection'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Entity Template Management');
/*Dimension Management*/
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Dimension Management', 
(
    Select id from features where name = 'Data Planing'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Dimension Management');
/*Data Meaning*/
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Data Meaning', 
(
    Select id from features where name = 'Data Planing'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Data Meaning');
/*Data Meaning (public)*/

INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Data Meaning (public)', 
(
    Select id from features where name = 'Data Planing'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Data Meaning (public)');

/* Update Operations */
/*  ADD **/
/* Shell Ldap **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'Shell', 'Ldap', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='Shell' AND controller='Ldap');
/* Shell Menu **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'Shell', 'Menu', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='Shell' AND controller='Menu');
/* Shell Settings **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'Shell', 'Settings', '*', (Select id from features where name = 'Settings' AND parentref = (
Select id from features where name = 'BExIS'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='Shell' AND controller='Settings');
/* DCM	Create **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'Create', '*', (Select id from features where name = 'Data Creation' AND parentref = (
Select id from features where name = 'Data Collection'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='Create');
/* DCM	Edit **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'Edit', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='Edit');
/* DCM	View **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'View', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='View');
/* DCM	Metadata **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'Metadata', '*', (Select id from features where name = 'Data Creation' AND parentref = (
Select id from features where name = 'Data Collection'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='Metadata');
/* Api	File */
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'File', '*', (Select id from features where name = 'Data Upload' AND parentref = (
Select id from features where name = 'Data Collection'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='File');
/* DCM	FileUpload **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'FileUpload', '*', (Select id from features where name = 'Data Upload' AND parentref = (
Select id from features where name = 'Data Collection'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='FileUpload');
/* DCM	AttachmentUpload **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'AttachmentUpload', '*', (Select id from features where name = 'Data Upload' AND parentref = (
Select id from features where name = 'Data Collection'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='AttachmentUpload');
/* DCM	Validation **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'Validation', '*', (Select id from features where name = 'Data Upload' AND parentref = (
Select id from features where name = 'Data Collection'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='Validation');
/* DCM	Messages **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'Messages', '*', (Select id from features where name = 'Data Upload' AND parentref = (
Select id from features where name = 'Data Collection'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='Messages');
/* DCM	DataDescription **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'DataDescription', '*', (Select id from features where name = 'Data Upload' AND parentref = (
Select id from features where name = 'Data Collection'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='DataDescription');
/* DCM	EntityTemplates **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'EntityTemplates', '*', (Select id from features where name = 'Entity Template Management' AND parentref = (
Select id from features where name = 'Data Collection'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='EntityTemplates');
/* RPM	DataStructure **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'DataStructure', '*', (Select id from features where name = 'Datastructure Management' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='DataStructure');
/* RPM	VariableTemplate **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'VariableTemplate', '*', (Select id from features where name = 'Variables Template Management' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='VariableTemplate');
/* RPM	Dimension **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'Dimension', '*', (Select id from features where name = 'Unit Management' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='Dimension');
/* API	MeaningsAdmin **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'MeaningsAdmin', '*', (Select id from features where name = 'Data Meaning' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='MeaningsAdmin');
/* API	Meanings **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'Meanings', '*', (Select id from features where name = 'Data Meaning (public)' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='Meanings');
/* RPM	Meaning **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'Meaning', '*', (Select id from features where name = 'Data Meaning (public)' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='Meaning');
/* RPM	ExternalLink **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'ExternalLink', '*', (Select id from features where name = 'Data Meaning (public)' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='ExternalLink');
/* DIM	Publish **/ 
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DIM', 'Publish', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DIM' AND controller='Publish');

/** REMOVE **/
/** DCM	SubmitChooseUpdateMethod **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'SubmitChooseUpdateMethod';
/** DCM	SubmitGetFileInformation **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'SubmitGetFileInformation';
/** DCM	SubmitSelectAFile **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'SubmitSelectAFile';
/** DCM	SubmitSpecifyDataset **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'SubmitSpecifyDataset';
/** DCM	SubmitSummary **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'SubmitSummary';
/** DCM	SubmitValidation **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'SubmitValidation';
/** DCM	Push **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'Push';
/** DCM	EasyUpload **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'EasyUpload';
/** DCM	EasyUploadSelectAFile **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'EasyUploadSelectAFile';
/** DCM	EasyUploadSelectAreas **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'EasyUploadSelectAreas';
/** DCM	EasyUploadSheetDataStructure **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'EasyUploadSheetDataStructure';
/** DCM	EasyUploadSheetSelectMetaData **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'EasyUploadSheetSelectMetaData';
/** DCM	EasyUploadSummary **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'EasyUploadSummary';
/** DCM	EasyUploadVerification **/
DELETE FROM public.operations Where module = 'DCM' and controller = 'EasyUploadVerification';




/** Entity Templates **/
/* add template for unstructured */
INSERT INTO public.entitytemplates(
	id, versionno, extra, name, description, metadatainvalidsavemode, hasdatastructure, entityref, metadatastructureref)
	VALUES (1, 1, null, 'File', 'Use this template if you want to upload files only', true, false,1,1);

/* add template for structured */
INSERT INTO public.entitytemplates(
	id, versionno, extra, name, description, metadatainvalidsavemode, hasdatastructure, entityref, metadatastructureref)
	VALUES (2, 1, null, 'Data', 'Use this template if you want to upload data', true, true,1,1);

/* update datasets */
Update Datasets SET entitytemplateref=1 where datastructureref in (select id from datastructures where datastructuretype like 'UnS');
Update Datasets SET entitytemplateref=2 where datastructureref in (select id from datastructures where datastructuretype like 'STR');


/** Variable Templates & Variables Update from DataContainer **/
/** create variable Templates  -type = VAR_TEMPL**/

/*  set all existing varaibles type to VAR_INST */
Update public.variables
set variablestype = 'VAR_INST';

/*  add all da to VAR_TEMPL */
INSERT INTO public.variables(
		versionno, extra, label, description, dataattributeref, unitref, datatyperef, variablestype, approved)
Select	versionno, extra, name, description, id, unitref, datatyperef, 'VAR_TEMPL', true
From public.datacontainers Where discriminator like 'DA';
	

/*  set all templates to variables */
update variables as x
SET vartemplateref = b.id
from  variables b
where x.dataattributeref = b.dataattributeref and b.variablestype = 'VAR_TEMPL';


/** update Constraints **/

/***Link constraints to varaibles*/


/* Seed data dwc terms as meanings */

/**********************************************************************************************/
/********************** SCHEMA CHANGES AFTER DATA  ********************************************/
/**********************************************************************************************/


/** set datatyperef in variable to not null**/
ALTER TABLE
    IF EXISTS public.variables
ALTER COLUMN 
    datatyperef bigint NOT NULL;

/** set type in variable to not null**/
ALTER TABLE
    IF EXISTS public.variables
ALTER
    COLUMN variablestype character varying(255) COLLATE pg_catalog."default" NOT NULL;

/** set entitytemplateref in datasets to not null**/
ALTER TABLE
    IF EXISTS public.datasets
ADD
    COLUMN entitytemplateref bigint NOT NULL;


/* drop all varaible links to dataattributeref*/
ALTER TABLE
    IF EXISTS public.variables DROP COLUMN IF EXISTS dataattributeref;


END;