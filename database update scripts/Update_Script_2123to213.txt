BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

-- DROP TABLE public.entityreferences;

CREATE TABLE IF NOT EXISTS public.entityreferences (
	id bigserial NOT NULL,
	versionno int4 NOT NULL,
	extra xml NULL,
	sourceid int8 NULL,
	sourceentityid int8 NULL,
	sourceversion int4 NULL,
	targetid int8 NULL,
	targetentityid int8 NULL,
	targetversion int4 NULL,
	context varchar(255) NULL,
	referencetype varchar(255) NULL,
	CONSTRAINT entityreferences_pkey PRIMARY KEY (id)
);


-- DROP SEQUENCE public.entityreferences_id_seq;

CREATE SEQUENCE IF NOT EXISTS public.entityreferences_id_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 9223372036854775807
	START 1;


-- DROP TABLE public.missingvalues;

CREATE TABLE IF NOT EXISTS public.missingvalues (
	id bigserial NOT NULL,
	displayname varchar(255) NULL,
	placeholder varchar(255) NULL,
	description varchar(255) NULL,
	variableref int8 NOT NULL,
	CONSTRAINT missingvalues_pkey PRIMARY KEY (id)
);

ALTER TABLE public.missingvalues DROP CONSTRAINT IF EXISTS fk9b8ae33d70fe39a;

ALTER TABLE public.missingvalues ADD CONSTRAINT fk9b8ae33d70fe39a FOREIGN KEY (variableref) REFERENCES variables(id);


-- DROP SEQUENCE public.missingvalues_id_seq;

CREATE SEQUENCE IF NOT EXISTS public.missingvalues_id_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 9223372036854775807
	START 1;


ALTER TABLE metadataattributeusages ALTER COLUMN description TYPE text;
ALTER TABLE MetadataNestedAttributeUsages ALTER COLUMN description TYPE text;
ALTER TABLE MetadataPackageUsages ALTER COLUMN description TYPE text;
ALTER TABLE MetadataPackages ALTER COLUMN description TYPE text;


-- INSERT Data --
INSERT INTO public.operations
(versionno, extra, "module", controller, "action", featureref)
VALUES(1, NULL, 'Shell', 'Help', '*', NULL);


-- UPDATE Data --
UPDATE public.operations
	SET featureref=null
	WHERE module='DCM' AND controller='Attachments';

UPDATE public.operations
	SET featureref=null
	WHERE module='DIM' AND controller='Submission';
   
--COMMIT;
