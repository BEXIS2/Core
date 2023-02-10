BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '2.17',NOW());


-- CREATE TABLES

-- MAPPING CONCEPT

CREATE SEQUENCE public.mappingconcepts_id_seq
	INCREMENT 1
	START 1
	MINVALUE 1
	MAXVALUE 9223372036854775807
	CACHE 1;

ALTER SEQUENCE public.mappingconcepts_id_seq
    OWNER TO postgres;


CREATE TABLE public.mappingconcepts
(
    id bigint NOT NULL DEFAULT nextval('mappingconcepts_id_seq'::regclass),
    version integer NOT NULL,
    name character varying(255) COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    url character varying(255) COLLATE pg_catalog."default",
    CONSTRAINT mappingconcepts_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE public.mappingconcepts
    OWNER to postgres;
CREATE INDEX idx_mappingconcepts_id
    ON public.mappingconcepts USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;
CREATE INDEX idx_mappingconcepts_name
    ON public.mappingconcepts USING btree
    (name COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;


-- MAPPING KEY

CREATE SEQUENCE public.mappingkeys_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.mappingkeys_id_seq
    OWNER TO postgres;

CREATE TABLE public.mappingkeys
(
    id bigint NOT NULL DEFAULT nextval('mappingkeys_id_seq'::regclass),
    name character varying(255) COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    url character varying(255) COLLATE pg_catalog."default",
    optional boolean,
    iscomplex boolean,
    concept bigint NOT NULL,
    parentref bigint,
    CONSTRAINT mappingkeys_pkey PRIMARY KEY (id),
    CONSTRAINT fk1e8edae822003ee7 FOREIGN KEY (parentref)
        REFERENCES public.mappingkeys (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk1e8edae8aaab514d FOREIGN KEY (concept)
        REFERENCES public.mappingconcepts (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE public.mappingkeys
    OWNER to postgres;
CREATE INDEX idx_concept_mappingkeys
    ON public.mappingkeys USING btree
    (concept ASC NULLS LAST)
    TABLESPACE pg_default;
CREATE INDEX idx_keys_name
    ON public.mappingkeys USING btree
    (name COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
CREATE INDEX idx_mappingkeys_id
    ON public.mappingkeys USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;


-- Metadata Parameter Usages
CREATE SEQUENCE public.metadataparameterusages_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.metadataparameterusages_id_seq
    OWNER TO postgres;

CREATE TABLE public.metadataparameterusages
(
    id bigint NOT NULL DEFAULT nextval('metadataparameterusages_id_seq'::regclass),
    versionno integer NOT NULL,
    extra xml,
    mincardinality integer,
    maxcardinality integer,
    label character varying(255) COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    metadataattributeref bigint NOT NULL,
    metadataparameterref bigint NOT NULL,
    CONSTRAINT metadataparameterusages_pkey PRIMARY KEY (id),
    CONSTRAINT fk3d55e972649c48b FOREIGN KEY (metadataattributeref)
        REFERENCES public.datacontainers (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk3d55e972714dc81d FOREIGN KEY (metadataparameterref)
        REFERENCES public.datacontainers (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE public.metadataparameterusages
    OWNER to postgres;
CREATE INDEX idx_metadataparameterusages_id
    ON public.metadataparameterusages USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;
CREATE INDEX idx_metadataparameterusages_label
    ON public.metadataparameterusages USING btree
    (label COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;




COMMIT;
