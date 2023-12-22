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

ALTER TABLE IF EXISTS public.entitytemplates
    ALTER COLUMN id SET DEFAULT nextval('entitytemplates_id_seq'::regclass);



CREATE TABLE IF NOT EXISTS public.accessrules
(
    id bigint NOT NULL,
    versionno integer NOT NULL,
    securitykey character varying(255) COLLATE pg_catalog."default",
    securityobjecttype integer,
    rulebody character varying(255) COLLATE pg_catalog."default",
    displayname character varying(255) COLLATE pg_catalog."default",
    parentref bigint,
    CONSTRAINT accessrules_pkey PRIMARY KEY (id),
    CONSTRAINT fk_accessruleentitys_parentaccessruleentity FOREIGN KEY (parentref)
        REFERENCES public.accessrules (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.accessrules
    OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.variable_constraints (
    constraintref bigint NOT NULL,
    variableref bigint NOT NULL,
    CONSTRAINT variable_constraints_pkey PRIMARY KEY (variableref, constraintref),
    CONSTRAINT fk_17300cc1 FOREIGN KEY (variableref) REFERENCES public.variables (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT fk_8ba3b117 FOREIGN KEY (constraintref) REFERENCES public.constraints (id) MATCH SIMPLE ON UPDATE NO ACTION ON DELETE NO ACTION
) TABLESPACE pg_default;

ALTER TABLE
    IF EXISTS public.variable_constraints OWNER to postgres;

/* VARIABLES */

ALTER TABLE
    IF EXISTS public.variables
ALTER COLUMN
    datastructureref DROP NOT NULL;

ALTER TABLE
    IF EXISTS public.variables
ADD
    COLUMN approved boolean DEFAULT true;

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

/**END VARIABLES */

/**meanings**/

CREATE TABLE IF NOT EXISTS public.rpm_prefixcategory
(
    id bigint NOT NULL,
    versionno integer NOT NULL,
    name character varying(255) COLLATE pg_catalog."default",
    description character varying(255) COLLATE pg_catalog."default",
    CONSTRAINT rpm_prefixcategory_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rpm_prefixcategory
    OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.rpm_meaning_externallink
(
    meaningsref bigint NOT NULL,
    externallinkref bigint NOT NULL,
    CONSTRAINT rpm_meaning_externallink_pkey PRIMARY KEY (meaningsref, externallinkref)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rpm_meaning_externallink
    OWNER to postgres;

/*  External LINKS */

CREATE TABLE IF NOT EXISTS public.rpm_externallink
(
    id bigint NOT NULL,
    versionno integer NOT NULL,
    uri character varying(255) COLLATE pg_catalog."default",
    name character varying(255) COLLATE pg_catalog."default",
    type integer,
    prefix bigint,
    prefixcategory bigint
);

CREATE SEQUENCE IF NOT EXISTS public.rpm_externallink_id_seq INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 OWNED BY rpm_externallink.id;

ALTER SEQUENCE public.rpm_externallink_id_seq OWNER TO postgres;

ALTER TABLE IF EXISTS public.rpm_externallink
    OWNER to postgres;
	

ALTER TABLE IF EXISTS public.rpm_externallink
    ALTER COLUMN id SET DEFAULT nextval('rpm_externallink_id_seq'::regclass);
	

ALTER TABLE IF EXISTS public.rpm_externallink
 	ADD CONSTRAINT rpm_externallink_pkey PRIMARY KEY (id);

ALTER TABLE IF EXISTS public.rpm_externallink	
	ADD CONSTRAINT fk_a4adc10f FOREIGN KEY (prefix)
        REFERENCES public.rpm_externallink (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION;	

ALTER TABLE IF EXISTS public.rpm_externallink
    ADD CONSTRAINT fk_cc3bd9e2 FOREIGN KEY (prefixcategory)
    REFERENCES public.rpm_prefixcategory (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

/* end  External LINKS */

/* MEANING ENTRY */

CREATE TABLE IF NOT EXISTS public.rpm_meaningentry
(
    id bigint NOT NULL,
    mappingrelation bigint,
    CONSTRAINT rpm_meaningentry_pkey PRIMARY KEY (id),
    CONSTRAINT fk_a9af26db FOREIGN KEY (mappingrelation)
        REFERENCES public.rpm_externallink (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
);

ALTER TABLE IF EXISTS public.rpm_meaningentry
    OWNER to postgres;
	
CREATE SEQUENCE IF NOT EXISTS public.rpm_meaningentry_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1
    OWNED BY rpm_meaningentry.id;
	
ALTER TABLE IF EXISTS public.rpm_meaningentry
    ALTER COLUMN id SET DEFAULT nextval('rpm_meaningentry_id_seq'::regclass);


ALTER SEQUENCE public.rpm_meaningentry_id_seq
    OWNER TO postgres;

/* end MEANING ENTRY  */

CREATE SEQUENCE IF NOT EXISTS public.accessrules_id_seq INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 OWNED BY accessrules.id;

ALTER SEQUENCE public.accessrules_id_seq OWNER TO postgres;
	
CREATE TABLE IF NOT EXISTS public.rpm_meanings
(
    id bigint NOT NULL,
    versionno integer NOT NULL,
    name character varying(255) COLLATE pg_catalog."default",
    shortname character varying(255) COLLATE pg_catalog."default",
    description character varying(255) COLLATE pg_catalog."default",
    approved boolean,
    selectable boolean,
    CONSTRAINT rpm_meanings_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rpm_meanings
    OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.rpm_meaning_constraints
(
    meaningref bigint NOT NULL,
    constraintref bigint NOT NULL,
    CONSTRAINT rpm_meaning_constraints_pkey PRIMARY KEY (meaningref, constraintref),
    CONSTRAINT fk_2c15262f FOREIGN KEY (constraintref)
        REFERENCES public.constraints (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_82e106c9 FOREIGN KEY (meaningref)
        REFERENCES public.rpm_meanings (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rpm_meaning_constraints
    OWNER to postgres;


CREATE TABLE IF NOT EXISTS public.rpm_meaning_meaning
(
    meaningsparentref bigint NOT NULL,
    meaningschildrenref bigint NOT NULL,
    CONSTRAINT rpm_meaning_meaning_pkey PRIMARY KEY (meaningsparentref, meaningschildrenref)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rpm_meaning_meaning
    OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.rpm_meaning_related_meaning
(
    meaningref bigint NOT NULL,
    parentmeaningref bigint NOT NULL,
    CONSTRAINT fk_a3ef456 FOREIGN KEY (meaningref)
        REFERENCES public.rpm_meanings (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_d0f1340d FOREIGN KEY (parentmeaningref)
        REFERENCES public.rpm_meanings (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rpm_meaning_related_meaning
    OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.rpm_meaningentry
(
    id bigint NOT NULL,
    mappingrelation bigint,
    CONSTRAINT rpm_meaningentry_pkey PRIMARY KEY (id),
    CONSTRAINT fk_a9af26db FOREIGN KEY (mappingrelation)
        REFERENCES public.rpm_externallink (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rpm_meaningentry
    OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.rpm_meaningentry_mappedlinks
(
    meaningentryref bigint NOT NULL,
    externallink_mapped_linkref bigint NOT NULL,
    CONSTRAINT fk_45a06b73 FOREIGN KEY (externallink_mapped_linkref)
        REFERENCES public.rpm_externallink (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_75241888 FOREIGN KEY (meaningentryref)
        REFERENCES public.rpm_meaningentry (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rpm_meaningentry_mappedlinks
    OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.rpm_meanings_meaningentry
(
    meaningref bigint NOT NULL,
    meaningentryref bigint NOT NULL,
    CONSTRAINT fk_5b58106d FOREIGN KEY (meaningref)
        REFERENCES public.rpm_meanings (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_ce0176f7 FOREIGN KEY (meaningentryref)
        REFERENCES public.rpm_meaningentry (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rpm_meanings_meaningentry
    OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.rpm_meanings_variables
(
    variableref bigint NOT NULL,
    meaningref bigint NOT NULL,
    CONSTRAINT rpm_meanings_variables_pkey PRIMARY KEY (variableref, meaningref),
    CONSTRAINT fk_5428fd5b FOREIGN KEY (meaningref)
        REFERENCES public.rpm_meanings (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_98f6228e FOREIGN KEY (variableref)
        REFERENCES public.variables (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rpm_meanings_variables
    OWNER to postgres;

/* -- */

/*end Meanings */

CREATE TABLE IF NOT EXISTS public.variable_constraints
(
    constraintref bigint NOT NULL,
    variableref bigint NOT NULL,
    CONSTRAINT variable_constraints_pkey PRIMARY KEY (variableref, constraintref),
    CONSTRAINT fk_17300cc1 FOREIGN KEY (variableref)
        REFERENCES public.variables (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_8ba3b117 FOREIGN KEY (constraintref)
        REFERENCES public.constraints (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.variable_constraints
    OWNER to postgres;

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

/* Constraints */

ALTER TABLE IF EXISTS public.constraints
    ALTER COLUMN datacontainerref DROP NOT NULL;

ALTER TABLE IF EXISTS public.constraints
    ADD COLUMN creationdate timestamp without time zone;

ALTER TABLE IF EXISTS public.constraints
    ADD COLUMN lastmodified timestamp without time zone;

ALTER TABLE IF EXISTS public.constraints
    ADD COLUMN lastmodifieduserref bigint;

ALTER TABLE IF EXISTS public.constraints
    ADD COLUMN name character varying(255) COLLATE pg_catalog."default";
ALTER TABLE IF EXISTS public.constraints DROP CONSTRAINT IF EXISTS fkb6093b2ee5c7912c;

ALTER TABLE IF EXISTS public.constraints
    ADD CONSTRAINT fk_fd8e6a17 FOREIGN KEY (datacontainerref)
    REFERENCES public.datacontainers (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


/* TABLE DROPS*/ 
DROP TABLE IF EXISTS public.parameters CASCADE;

/* SEQUENCE */

CREATE SEQUENCE IF NOT EXISTS public.entitytemplates_id_seq INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 OWNED BY entitytemplates.id;

ALTER SEQUENCE public.entitytemplates_id_seq OWNER TO postgres;

CREATE SEQUENCE IF NOT EXISTS public.rpm_prefixcategory_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1
    OWNED BY rpm_prefixcategory.id;

ALTER SEQUENCE public.rpm_prefixcategory_id_seq
    OWNER TO postgres;

CREATE SEQUENCE IF NOT EXISTS public.rpm_meanings_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1
    OWNED BY rpm_meanings.id;

ALTER SEQUENCE public.rpm_meanings_id_seq
    OWNER TO postgres;




DROP SEQUENCE IF EXISTS public.parameters_id_seq;

CREATE SEQUENCE IF NOT EXISTS public.entitytemplates_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1
    OWNED BY entitytemplates.id;

ALTER SEQUENCE public.entitytemplates_id_seq
    OWNER TO postgres;

CREATE SEQUENCE IF NOT EXISTS public.accessrules_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1
    OWNED BY accessrules.id;

ALTER SEQUENCE public.accessrules_id_seq
    OWNER TO postgres;


ALTER TABLE IF EXISTS public.rpm_meanings
    ALTER COLUMN id SET DEFAULT nextval('rpm_meanings_id_seq'::regclass);


ALTER TABLE IF EXISTS public.rpm_prefixcategory
    ALTER COLUMN id SET DEFAULT nextval('rpm_prefixcategory_id_seq'::regclass);

/* UNITS */

ALTER TABLE IF EXISTS public.units
    ADD COLUMN externallinkref bigint;

/** DROP COLUMNS */
ALTER TABLE IF EXISTS public.datacontainers DROP COLUMN IF EXISTS classifierref;




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

/*Constraint Management*/
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Constraint Management', 
(
    Select id from features where name = 'Data Planing'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Constraint Management');

/*Data Type Management*/
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Data Type Management', 
(
    Select id from features where name = 'Data Planing'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Data Type Management');

/*Data Meaning*/
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Meaning', 
(
    Select id from features where name = 'Data Planing'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Meaning');
/*Meaning API*/

INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, '', 'Meaning API', 
(
    Select id from features where name = 'Data Planing'
)
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Meaning API');

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
/* Shell Header **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'Shell', 'Header', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='Shell' AND controller='Header');
/* Shell Tokens **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'Shell', 'Tokens', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='Shell' AND controller='Tokens');
/* API Tokens **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'Api', 'Tokens', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='Api' AND controller='Tokens');
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
/* RPM	Constraints **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'Constraints', '*', (Select id from features where name = 'Constraints Management' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='Constraints');
/* RPM	Constraints **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'Constraints', '*', (Select id from features where name = 'Constraints Management' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='Constraints');
/* RPM	Data Type **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'DataType', '*', (Select id from features where name = 'Data Type Management' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='DataType');
/* API	MeaningsAdmin **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'MeaningsAdmin', '*', (Select id from features where name = 'Meaning' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='MeaningsAdmin');
/* API	Meanings **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'Meanings', '*', (Select id from features where name = 'Meaning API' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='Meanings');
/* RPM	Meaning **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'Meaning', '*', (Select id from features where name = 'Meaning' AND parentref = (
Select id from features where name = 'Data Planning'))
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='Meaning');
/* RPM	ExternalLink **/
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'ExternalLink', '*', (Select id from features where name = 'Meaning' AND parentref = (
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
/** API TOKEN **/
DELETE FROM public.operations Where module = 'Api' and controller = 'Token';



/** Entity Templates **/
/* add template for unstructured */
INSERT INTO public.entitytemplates(
	id, versionno, extra, name, description, metadatainvalidsavemode, hasdatastructure, entityref, metadatastructureref)
	VALUES (1, 1, null, 'File', 'Use this template if you want to upload files only', true, false,1,1);

/* add template for structured */
INSERT INTO public.entitytemplates(
	id, versionno, extra, name, description, metadatainvalidsavemode, hasdatastructure, entityref, metadatastructureref)
	VALUES (2, 1, null, 'Data', 'Use this template if you want to upload data', true, true,1,2);

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
	
/* set VAR_INST Datatype based on VAR_TEMPL*/
update variables as x
SET datatyperef = b.datatyperef
from  variables as b
where x.vartemplateref = b.id ;

/*  set all templates to variables */
update variables as x
SET vartemplateref = b.id
from  variables b
where x.dataattributeref = b.dataattributeref and b.variablestype = 'VAR_TEMPL';


/** update Constraints **/

/***Link constraints to varaibles*/
/* fill coupling table variables_constraint*/
INSERT INTO variable_constraints(constraintref,variableref)
Select c.id, v.id
From constraints as c, variables as v
WHERE c.datacontainerref = v.dataattributeref;

/* set name and remove datacontainer*/
UPDATE constraints as c
SET name = c.id, datacontainerref = null
where datacontainerref in (Select id from public.datacontainers Where discriminator like 'DA');

/* delete all data container with discimrinator */
DELETE from public.datacontainers Where discriminator like 'DA'


/**********************************************************************************************/
/********************** SCHEMA CHANGES AFTER DATA  ********************************************/
/**********************************************************************************************/


/** set datatyperef in variable to not null**/
ALTER TABLE
    IF EXISTS public.variables
ALTER COLUMN 
    datatyperef SET NOT NULL;

/** set type in variable to not null**/
ALTER TABLE
    IF EXISTS public.variables
ALTER
    COLUMN variablestype SET NOT NULL;

/** set entitytemplateref in datasets to not null**/
ALTER TABLE
    IF EXISTS public.datasets
ALTER
    COLUMN entitytemplateref SET NOT NULL;


/* drop all varaible links to dataattributeref*/
ALTER TABLE
    IF EXISTS public.variables DROP COLUMN IF EXISTS dataattributeref;

END;