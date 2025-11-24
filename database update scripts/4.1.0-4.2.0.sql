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

ALTER SEQUENCE public.curationentries_id_seq
    OWNED BY public.curationentries.id;

ALTER SEQUENCE public.curationentries_id_seq
    OWNER TO postgres;

CREATE SEQUENCE IF NOT EXISTS public.curationnotes_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.curationnotes_id_seq
    OWNED BY public.curationnotes.id;

ALTER SEQUENCE public.curationnotes_id_seq
    OWNER TO postgres;

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

ALTER TABLE IF EXISTS public.curationentries
    OWNER to postgres;
CREATE INDEX IF NOT EXISTS idx_curationentry_id
    ON public.curationentries USING btree
    (id ASC NULLS LAST)
    TABLESPACE pg_default;


    -- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.2.0',NOW());

commit;
