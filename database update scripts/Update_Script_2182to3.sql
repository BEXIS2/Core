 BEGIN; 
 CREATE TABLE IF NOT EXISTS public.entitytemplates
(
    id bigint NOT NULL DEFAULT nextval('entitytemplates_id_seq'::regclass),
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
    CONSTRAINT fk_a2c5bf76 FOREIGN KEY (entityref)
        REFERENCES public.entities (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_cf532bcb FOREIGN KEY (metadatastructureref)
        REFERENCES public.metadatastructures (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.entitytemplates
    OWNER to postgres;
CREATE TABLE IF NOT EXISTS public.externallink
(
    id bigint NOT NULL DEFAULT nextval('externallink_id_seq'::regclass),
    versionno integer NOT NULL,
    uri character varying(255) COLLATE pg_catalog."default",
    name character varying(255) COLLATE pg_catalog."default",
    type character varying(255) COLLATE pg_catalog."default",
    externallinkref bigint,
    CONSTRAINT externallink_pkey PRIMARY KEY (id),
    CONSTRAINT fk_6978e301 FOREIGN KEY (externallinkref)
        REFERENCES public.meanings (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.externallink
    OWNER to postgres;
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
CREATE TABLE IF NOT EXISTS public.meaning_meaning
(
    meaningsparentref bigint NOT NULL,
    meaningschildrenref bigint NOT NULL,
    CONSTRAINT meaning_meaning_pkey PRIMARY KEY (meaningsparentref, meaningschildrenref),
    CONSTRAINT fk_516f2fd8 FOREIGN KEY (meaningschildrenref)
        REFERENCES public.meanings (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_d0cc9d49 FOREIGN KEY (meaningsparentref)
        REFERENCES public.meanings (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.meaning_meaning
    OWNER to postgres;
CREATE TABLE IF NOT EXISTS public.meanings_variables
(
    variableref bigint NOT NULL,
    meaningref bigint NOT NULL,
    CONSTRAINT meanings_variables_pkey PRIMARY KEY (meaningref, variableref),
    CONSTRAINT fk_a7c43e6c FOREIGN KEY (variableref)
        REFERENCES public.variables (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_c5170336 FOREIGN KEY (meaningref)
        REFERENCES public.meanings (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.meanings_variables
    OWNER to postgres;
CREATE TABLE IF NOT EXISTS public.accessrules
(
    id bigint NOT NULL DEFAULT nextval('accessrules_id_seq'::regclass),
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
CREATE TABLE IF NOT EXISTS public.meanings
(
    id bigint NOT NULL DEFAULT nextval('meanings_id_seq'::regclass),
    versionno integer NOT NULL,
    name character varying(255) COLLATE pg_catalog."default",
    shortname character varying(255) COLLATE pg_catalog."default",
    description character varying(255) COLLATE pg_catalog."default",
    approved integer,
    selectable integer,
    CONSTRAINT meanings_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.meanings
    OWNER to postgres;
ALTER TABLE IF EXISTS public.variables DROP COLUMN IF EXISTS dataattributeref;

ALTER TABLE IF EXISTS public.variables
    ALTER COLUMN datastructureref DROP NOT NULL;

ALTER TABLE IF EXISTS public.variables
    ADD COLUMN approved boolean;

ALTER TABLE IF EXISTS public.variables
    ADD COLUMN datatyperef bigint NOT NULL;

ALTER TABLE IF EXISTS public.variables
    ADD COLUMN displaypatternid integer;

ALTER TABLE IF EXISTS public.variables
    ADD COLUMN iskey boolean;

ALTER TABLE IF EXISTS public.variables
    ADD COLUMN variablestype character varying(255) COLLATE pg_catalog."default" NOT NULL;

ALTER TABLE IF EXISTS public.variables
    ADD COLUMN vartemplateref bigint;
ALTER TABLE IF EXISTS public.variables DROP CONSTRAINT IF EXISTS fk1a5c9d114fe09c0d;

ALTER TABLE IF EXISTS public.variables DROP CONSTRAINT IF EXISTS fk1a5c9d117cadd8dc;

ALTER TABLE IF EXISTS public.variables DROP CONSTRAINT IF EXISTS fk1a5c9d1190fe2254;

ALTER TABLE IF EXISTS public.variables
    ADD CONSTRAINT fk_2abad2e6 FOREIGN KEY (vartemplateref)
    REFERENCES public.variables (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE IF EXISTS public.variables
    ADD CONSTRAINT fk_35a1bc0a FOREIGN KEY (unitref)
    REFERENCES public.units (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE IF EXISTS public.variables
    ADD CONSTRAINT fk_7f919d88 FOREIGN KEY (datastructureref)
    REFERENCES public.datastructures (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE IF EXISTS public.variables
    ADD CONSTRAINT fk_990c0cd5 FOREIGN KEY (datatyperef)
    REFERENCES public.datatypes (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;
DROP INDEX IF EXISTS public.idx_dataattributeref_variables;
DROP INDEX IF EXISTS public.idx_datastructureref_variables;
ALTER TABLE IF EXISTS public.datasets
    ALTER COLUMN datastructureref DROP NOT NULL;

ALTER TABLE IF EXISTS public.datasets
    ADD COLUMN entitytemplateref bigint NOT NULL;

ALTER TABLE IF EXISTS public.datasets
    ADD CONSTRAINT fk_d593bd3c FOREIGN KEY (entitytemplateref)
    REFERENCES public.entitytemplates (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;
ALTER TABLE IF EXISTS public.datacontainers DROP COLUMN IF EXISTS classifierref;
ALTER TABLE IF EXISTS public.datacontainers DROP CONSTRAINT IF EXISTS fk3beb06a45a7e030;

ALTER TABLE IF EXISTS public.datacontainers DROP CONSTRAINT IF EXISTS fk3beb06a7cadd8dc;

ALTER TABLE IF EXISTS public.datacontainers DROP CONSTRAINT IF EXISTS fk3beb06ac49d0116;

ALTER TABLE IF EXISTS public.datacontainers DROP CONSTRAINT IF EXISTS fk3beb06af68222;

ALTER TABLE IF EXISTS public.datacontainers
    ADD CONSTRAINT fk_5e17ebf9 FOREIGN KEY (datatyperef)
    REFERENCES public.datatypes (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE IF EXISTS public.datacontainers
    ADD CONSTRAINT fk_7330dbef FOREIGN KEY (methodologyref)
    REFERENCES public.methodologies (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE IF EXISTS public.datacontainers
    ADD CONSTRAINT fk_f9a7e19e FOREIGN KEY (unitref)
    REFERENCES public.units (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;
CREATE SEQUENCE IF NOT EXISTS public.meanings_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1
    OWNED BY meanings.id;

ALTER SEQUENCE public.meanings_id_seq
    OWNER TO postgres;
CREATE SEQUENCE IF NOT EXISTS public.externallink_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1
    OWNED BY externallink.id;

ALTER SEQUENCE public.externallink_id_seq
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
CREATE SEQUENCE IF NOT EXISTS public.entitytemplates_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1
    OWNED BY entitytemplates.id;

ALTER SEQUENCE public.entitytemplates_id_seq
    OWNER TO postgres;

 END;