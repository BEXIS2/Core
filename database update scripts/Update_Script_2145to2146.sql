BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;
-- Add new description field and copy description from XML field (extra) to the new description column
Alter Table contentdescriptors add column description text;
Update contentdescriptors set description =  (xpath('/extra/fileDescription/text()', extra))[1]::text;

-- Add field for filz size
Alter Table contentdescriptors add column filesize integer;

-- Add new columns for Versions
Alter Table datasetversions add column versiontype varchar(255);
Alter Table datasetversions add column versionname varchar(255);
Alter Table datasetversions add column versiondescription text;
Alter Table datasetversions add column publicaccess boolean;
Alter Table datasetversions add column publicaccessdate timestamp without time zone;

-- Change/Add splitted requests
INSERT INTO public.features(
	versionno, extra, description, name, parentref)
	VALUES (1, null, 'Allow to send requests', 'Requests Send', (Select id from features where name = 'Data Discovery'));

INSERT INTO public.operations
(versionno, extra, "module", controller, "action", featureref)
VALUES(1, NULL, 'DDM', 'RequestsSend', '*', (
Select id from features where name = 'Requests Send' AND parentref = (
Select id from features where name = 'Data Discovery')));

Update features set name = 'Requests Manage' where name = 'Requests';
Update features set description = 'Manange requests by user' where name = 'Requests Manage';

update operations set controller = 'RequestsManage' where controller = 'Requests';

-- Set old permission settings from requests manange to send as well? 

-- Add Creation Date to entitypermissions
Alter TABLE entitypermissions add column creationdate timestamp without time zone;

-- Add Entry for api/token
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'Token', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='Token');

-- Add Entry for datacite doi inside dim
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DIM', 'DataCiteDoi', '*', (Select id from features where name = 'Data Dissemination')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DIM' AND controller='DataCiteDoi');

-- Add Entry for DOI as broker inside dim
INSERT INTO public.dim_brokers(
	versionno, extra, name, server, username, password, metadataformat, primarydataformat, link)
	VALUES (1, null, 'DOI', '', '', '', '', '', '');

-- Add Entry for DataCite repository inside dim
insert into public.dim_repositories(
	versionno, extra, name, url, brokerref)
select 
    1, null, 'DataCite', '', id
from public.dim_brokers where name = 'DOI';


-- Add Entry for FormerMembers
INSERT INTO public.features(versionno, extra, description, name, parentref)
VALUES (1, null, 'Former Member Management', 'Former Member Management', (Select id from features where name = 'Administration'));
	
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'SAM', 'FormerMember', '*', (Select id from features where name = 'Former Member Management')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DIM' AND controller='FormerMember');

CREATE SEQUENCE public.entitypermissions_formermember_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.entitypermissions_formermember_id_seq
    OWNER TO postgres;
	
CREATE SEQUENCE public.featurepermissions_formermember_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.featurepermissions_formermember_id_seq
    OWNER TO postgres;

CREATE SEQUENCE public.users_groups_formermember_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.users_groups_formermember_id_seq
    OWNER TO postgres;

CREATE TABLE public.entitypermissions_formermember
(
    id bigint NOT NULL DEFAULT nextval('entitypermissions_formermember_id_seq'::regclass),
    versionno integer NOT NULL,
    extra xml,
    entityref bigint,
    key bigint,
    rights integer,
    subjectref bigint,
    CONSTRAINT entitypermissions_formermember_pkey PRIMARY KEY (id),
    CONSTRAINT fk75c8c89d9f11e324 FOREIGN KEY (entityref)
        REFERENCES public.entities (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE public.entitypermissions_formermember
    OWNER to postgres;

CREATE TABLE public.featurepermissions_formermember
(
    id bigint NOT NULL DEFAULT nextval('featurepermissions_formermember_id_seq'::regclass),
    versionno integer NOT NULL,
    extra xml,
    featureref bigint,
    permissiontype integer NOT NULL,
    subjectref bigint,
    CONSTRAINT featurepermissions_formermember_pkey PRIMARY KEY (id),
    CONSTRAINT fk4c9ab452b32ba4aa FOREIGN KEY (featureref)
        REFERENCES public.features (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE public.featurepermissions_formermember
    OWNER to postgres;
	
CREATE TABLE public.users_groups_formermember
(
    id bigint NOT NULL DEFAULT nextval('users_groups_formermember_id_seq'::regclass),
    versionno integer NOT NULL,
    extra xml,
    userref bigint,
    groupref bigint,
    CONSTRAINT users_groups_formermember_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE public.users_groups_formermember
    OWNER to postgres;

-- Publication 
ALTER TABLE public.dim_publications DROP COLUMN doi;


-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '2.14.6',NOW());

COMMIT;