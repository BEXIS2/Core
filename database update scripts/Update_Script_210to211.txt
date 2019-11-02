BEGIN TRANSACTION;  
-- ROLLBACK TRANSACTION;

CREATE TABLE parties (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    extra xml,
    name character varying(255),
    alias character varying(255),
    description text,
    startdate timestamp without time zone,
    enddate timestamp without time zone,
    partytyperef bigint NOT NULL
);


ALTER TABLE parties OWNER TO postgres;


CREATE SEQUENCE parties_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE parties_id_seq OWNER TO postgres;

ALTER SEQUENCE parties_id_seq OWNED BY parties.id;


CREATE TABLE partycustomattributes (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    extra xml,
    name character varying(255),
    description text,
    validvalues character varying(255),
    isvalueoptional character(1),
    displayorder integer,
    datatype character varying(255),
    partytyperef bigint NOT NULL
);


ALTER TABLE partycustomattributes OWNER TO postgres;


CREATE SEQUENCE partycustomattributes_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE partycustomattributes_id_seq OWNER TO postgres;


ALTER SEQUENCE partycustomattributes_id_seq OWNED BY partycustomattributes.id;


CREATE TABLE partycustomattributevalues (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    extra xml,
    value character varying(255),
    customattributeref bigint NOT NULL,
    partyref bigint
);


ALTER TABLE partycustomattributevalues OWNER TO postgres;


CREATE SEQUENCE partycustomattributevalues_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE partycustomattributevalues_id_seq OWNER TO postgres;


ALTER SEQUENCE partycustomattributevalues_id_seq OWNED BY partycustomattributevalues.id;


CREATE TABLE partyrelationships (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    extra xml,
    title character varying(255),
    description text,
    startdate timestamp without time zone,
    enddate timestamp without time zone,
    scope character varying(255),
    partyrelationshiptyperef bigint NOT NULL,
    firstpartyref bigint NOT NULL,
    secondpartyref bigint NOT NULL
);


ALTER TABLE partyrelationships OWNER TO postgres;


CREATE SEQUENCE partyrelationships_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE partyrelationships_id_seq OWNER TO postgres;


ALTER SEQUENCE partyrelationships_id_seq OWNED BY partyrelationships.id;


CREATE TABLE partyrelationshiptypes (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    extra xml,
    title character varying(255),
    description text,
    indicateshierarchy character(1),
    mincardinality integer,
    maxcardinality integer
);


ALTER TABLE partyrelationshiptypes OWNER TO postgres;


CREATE SEQUENCE partyrelationshiptypes_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE partyrelationshiptypes_id_seq OWNER TO postgres;


ALTER SEQUENCE partyrelationshiptypes_id_seq OWNED BY partyrelationshiptypes.id;


CREATE TABLE partystatuses (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    extra xml,
    "timestamp" timestamp without time zone,
    description text,
    changelog xml,
    partystatustyperef bigint NOT NULL,
    partyref bigint NOT NULL
);


ALTER TABLE partystatuses OWNER TO postgres;


CREATE SEQUENCE partystatuses_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE partystatuses_id_seq OWNER TO postgres;


ALTER SEQUENCE partystatuses_id_seq OWNED BY partystatuses.id;


CREATE TABLE partystatustypes (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    extra xml,
    name character varying(255),
    description text,
    displayorder integer,
    partytyperef bigint NOT NULL
);


ALTER TABLE partystatustypes OWNER TO postgres;


CREATE SEQUENCE partystatustypes_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE partystatustypes_id_seq OWNER TO postgres;


ALTER SEQUENCE partystatustypes_id_seq OWNED BY partystatustypes.id;


CREATE TABLE partytypepairs (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    extra xml,
    title character varying(255),
    description text,
    alowedsourceref bigint NOT NULL,
    alowedtargetref bigint NOT NULL,
    partyrelationshiptyperef bigint
);


ALTER TABLE partytypepairs OWNER TO postgres;


CREATE SEQUENCE partytypepairs_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE partytypepairs_id_seq OWNER TO postgres;


ALTER SEQUENCE partytypepairs_id_seq OWNED BY partytypepairs.id;


CREATE TABLE partytypes (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    extra xml,
    title character varying(255),
    description text
);


ALTER TABLE partytypes OWNER TO postgres;


CREATE SEQUENCE partytypes_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE partytypes_id_seq OWNER TO postgres;


ALTER SEQUENCE partytypes_id_seq OWNED BY partytypes.id;


ALTER TABLE ONLY parties ALTER COLUMN id SET DEFAULT nextval('parties_id_seq'::regclass);


ALTER TABLE ONLY partycustomattributes ALTER COLUMN id SET DEFAULT nextval('partycustomattributes_id_seq'::regclass);


ALTER TABLE ONLY partycustomattributevalues ALTER COLUMN id SET DEFAULT nextval('partycustomattributevalues_id_seq'::regclass);


ALTER TABLE ONLY partyrelationships ALTER COLUMN id SET DEFAULT nextval('partyrelationships_id_seq'::regclass);


ALTER TABLE ONLY partyrelationshiptypes ALTER COLUMN id SET DEFAULT nextval('partyrelationshiptypes_id_seq'::regclass);


ALTER TABLE ONLY partystatuses ALTER COLUMN id SET DEFAULT nextval('partystatuses_id_seq'::regclass);


ALTER TABLE ONLY partystatustypes ALTER COLUMN id SET DEFAULT nextval('partystatustypes_id_seq'::regclass);


ALTER TABLE ONLY partytypepairs ALTER COLUMN id SET DEFAULT nextval('partytypepairs_id_seq'::regclass);


ALTER TABLE ONLY partytypes ALTER COLUMN id SET DEFAULT nextval('partytypes_id_seq'::regclass);


ALTER TABLE ONLY parties
    ADD CONSTRAINT parties_pkey PRIMARY KEY (id);


ALTER TABLE ONLY partycustomattributes
    ADD CONSTRAINT partycustomattributes_pkey PRIMARY KEY (id);


ALTER TABLE ONLY partycustomattributevalues
    ADD CONSTRAINT partycustomattributevalues_pkey PRIMARY KEY (id);


ALTER TABLE ONLY partyrelationships
    ADD CONSTRAINT partyrelationships_pkey PRIMARY KEY (id);


ALTER TABLE ONLY partyrelationshiptypes
    ADD CONSTRAINT partyrelationshiptypes_pkey PRIMARY KEY (id);


ALTER TABLE ONLY partystatuses
    ADD CONSTRAINT partystatuses_pkey PRIMARY KEY (id);


ALTER TABLE ONLY partystatustypes
    ADD CONSTRAINT partystatustypes_pkey PRIMARY KEY (id);


ALTER TABLE ONLY partytypepairs
    ADD CONSTRAINT partytypepairs_pkey PRIMARY KEY (id);


ALTER TABLE ONLY partytypes
    ADD CONSTRAINT partytypes_pkey PRIMARY KEY (id);

ALTER TABLE ONLY partycustomattributevalues
    ADD CONSTRAINT fk381d62f469a94133 FOREIGN KEY (customattributeref) REFERENCES partycustomattributes(id);

ALTER TABLE ONLY partycustomattributevalues
    ADD CONSTRAINT fk381d62f49a4d3305 FOREIGN KEY (partyref) REFERENCES parties(id);

ALTER TABLE ONLY partyrelationships
    ADD CONSTRAINT fk3e1d9d9a32f02b6a FOREIGN KEY (secondpartyref) REFERENCES parties(id);

ALTER TABLE ONLY partyrelationships
    ADD CONSTRAINT fk3e1d9d9a8035a2e0 FOREIGN KEY (partyrelationshiptyperef) REFERENCES partyrelationshiptypes(id);

ALTER TABLE ONLY partyrelationships
    ADD CONSTRAINT fk3e1d9d9ae4e49176 FOREIGN KEY (firstpartyref) REFERENCES parties(id);

ALTER TABLE ONLY parties
    ADD CONSTRAINT fk6773b61c2c3a26bd FOREIGN KEY (partytyperef) REFERENCES partytypes(id);

ALTER TABLE ONLY partystatustypes
    ADD CONSTRAINT fk70e058d42c3a26bd FOREIGN KEY (partytyperef) REFERENCES partytypes(id);

ALTER TABLE ONLY partytypepairs
    ADD CONSTRAINT fk7f0af5433a71f844 FOREIGN KEY (alowedtargetref) REFERENCES partytypes(id);

ALTER TABLE ONLY partytypepairs
    ADD CONSTRAINT fk7f0af5439f45e6fe FOREIGN KEY (alowedsourceref) REFERENCES partytypes(id);

ALTER TABLE ONLY partytypepairs
    ADD CONSTRAINT fk7f0af543d4a0d58a FOREIGN KEY (partyrelationshiptyperef) REFERENCES partytypes(id);

ALTER TABLE ONLY partycustomattributes
    ADD CONSTRAINT fkbe430b92c3a26bd FOREIGN KEY (partytyperef) REFERENCES partytypes(id);

ALTER TABLE ONLY partystatuses
    ADD CONSTRAINT fkd713d1639a4d3305 FOREIGN KEY (partyref) REFERENCES parties(id);

ALTER TABLE ONLY partystatuses
    ADD CONSTRAINT fkd713d163c3cbd5be FOREIGN KEY (partystatustyperef) REFERENCES partystatustypes(id);



CREATE TABLE datasetviews  ( 
	constrainttype	varchar(3) NOT NULL,
	containerselectioncriterion	varchar(255) NULL,
	contentselectioncriterion	varchar(255) NULL,
	datasetref	int8 NULL,
	extra	xml NULL,
	id	bigserial NOT NULL,
	name	varchar(255) NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;

ALTER TABLE datasetviews
	ADD CONSTRAINT fk_views_datasets
	FOREIGN KEY(datasetref)
	REFERENCES datasets(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION ;

	
	
	
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON datasetviews TO postgres WITH GRANT OPTION;
GRANT SELECT(name), INSERT(name), UPDATE(name), REFERENCES(name) ON datasetviews TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON datasetviews TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON datasetviews TO postgres WITH GRANT OPTION;
GRANT SELECT(datasetref), INSERT(datasetref), UPDATE(datasetref), REFERENCES(datasetref) ON datasetviews TO postgres WITH GRANT OPTION;
GRANT SELECT(contentselectioncriterion), INSERT(contentselectioncriterion), UPDATE(contentselectioncriterion), REFERENCES(contentselectioncriterion) ON datasetviews TO postgres WITH GRANT OPTION;
GRANT SELECT(containerselectioncriterion), INSERT(containerselectioncriterion), UPDATE(containerselectioncriterion), REFERENCES(containerselectioncriterion) ON datasetviews TO postgres WITH GRANT OPTION;
GRANT SELECT(constrainttype), INSERT(constrainttype), UPDATE(constrainttype), REFERENCES(constrainttype) ON datasetviews TO postgres WITH GRANT OPTION;




CREATE INDEX idx_datasetviews_id
	ON datasetviews USING btree (id int8_ops);

	
	
	
CREATE INDEX idx_datasetviews_name
	ON datasetviews USING btree (name text_ops);




CREATE TABLE decisions  ( 
	decisiondate	timestamp NULL,
	decisionmakerref	int8 NULL,
	extra	xml NULL,
	id	bigserial NOT NULL,
	reason	varchar(255) NULL,
	requestref	int8 NULL,
	status	int4 NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;




ALTER TABLE decisions
	ADD CONSTRAINT fk9ae16a6de5b6ce97
	FOREIGN KEY(decisionmakerref)
	REFERENCES users(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
	
	



CREATE TABLE dim_brokers  ( 
	extra	xml NULL,
	id	bigserial NOT NULL,
	metadataformat	varchar(255) NULL,
	name	varchar(255) NULL,
	password	varchar(255) NULL,
	primarydataformat	varchar(255) NULL,
	server	varchar(255) NULL,
	username	varchar(255) NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;


GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON dim_brokers TO postgres WITH GRANT OPTION;
GRANT SELECT(username), INSERT(username), UPDATE(username), REFERENCES(username) ON dim_brokers TO postgres WITH GRANT OPTION;
GRANT SELECT(server), INSERT(server), UPDATE(server), REFERENCES(server) ON dim_brokers TO postgres WITH GRANT OPTION;
GRANT SELECT(primarydataformat), INSERT(primarydataformat), UPDATE(primarydataformat), REFERENCES(primarydataformat) ON dim_brokers TO postgres WITH GRANT OPTION;
GRANT SELECT(password), INSERT(password), UPDATE(password), REFERENCES(password) ON dim_brokers TO postgres WITH GRANT OPTION;
GRANT SELECT(name), INSERT(name), UPDATE(name), REFERENCES(name) ON dim_brokers TO postgres WITH GRANT OPTION;
GRANT SELECT(metadataformat), INSERT(metadataformat), UPDATE(metadataformat), REFERENCES(metadataformat) ON dim_brokers TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON dim_brokers TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON dim_brokers TO postgres WITH GRANT OPTION;



	
CREATE TABLE dim_linkelements  ( 
	complexity	int4 NULL,
	elementid	int8 NULL,
	extra	xml NULL,
	id	bigserial NOT NULL,
	issequence	bool NULL,
	name	varchar(255) NULL,
	type	int4 NULL,
	versionno	int4 NOT NULL,
	xpath	varchar(255) NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;



GRANT SELECT(xpath), INSERT(xpath), UPDATE(xpath), REFERENCES(xpath) ON dim_linkelements TO postgres WITH GRANT OPTION;
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON dim_linkelements TO postgres WITH GRANT OPTION;
GRANT SELECT(type), INSERT(type), UPDATE(type), REFERENCES(type) ON dim_linkelements TO postgres WITH GRANT OPTION;
GRANT SELECT(name), INSERT(name), UPDATE(name), REFERENCES(name) ON dim_linkelements TO postgres WITH GRANT OPTION;
GRANT SELECT(issequence), INSERT(issequence), UPDATE(issequence), REFERENCES(issequence) ON dim_linkelements TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON dim_linkelements TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON dim_linkelements TO postgres WITH GRANT OPTION;
GRANT SELECT(elementid), INSERT(elementid), UPDATE(elementid), REFERENCES(elementid) ON dim_linkelements TO postgres WITH GRANT OPTION;
GRANT SELECT(complexity), INSERT(complexity), UPDATE(complexity), REFERENCES(complexity) ON dim_linkelements TO postgres WITH GRANT OPTION;




CREATE TABLE dim_mappings  ( 
	extra	xml NULL,
	id	bigserial NOT NULL,
	level	int8 NULL,
	parentref	int8 NULL,
	sourceref	int8 NULL,
	targetref	int8 NULL,
	transformationruleref	int8 NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;




ALTER TABLE dim_mappings
	ADD CONSTRAINT fk103ffbe8e801f3c1
	FOREIGN KEY(targetref)
	REFERENCES dim_linkelements(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
	
	
ALTER TABLE dim_mappings
	ADD CONSTRAINT fk103ffbe875d6a60f
	FOREIGN KEY(sourceref)
	REFERENCES dim_linkelements(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
	
ALTER TABLE dim_mappings
	ADD CONSTRAINT fk103ffbe844dec3ce
	FOREIGN KEY(parentref)
	REFERENCES dim_mappings(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
	
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON dim_mappings TO postgres WITH GRANT OPTION;
GRANT SELECT(transformationruleref), INSERT(transformationruleref), UPDATE(transformationruleref), REFERENCES(transformationruleref) ON dim_mappings TO postgres WITH GRANT OPTION;
GRANT SELECT(targetref), INSERT(targetref), UPDATE(targetref), REFERENCES(targetref) ON dim_mappings TO postgres WITH GRANT OPTION;
GRANT SELECT(sourceref), INSERT(sourceref), UPDATE(sourceref), REFERENCES(sourceref) ON dim_mappings TO postgres WITH GRANT OPTION;
GRANT SELECT(parentref), INSERT(parentref), UPDATE(parentref), REFERENCES(parentref) ON dim_mappings TO postgres WITH GRANT OPTION;
GRANT SELECT(level), INSERT(level), UPDATE(level), REFERENCES(level) ON dim_mappings TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON dim_mappings TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON dim_mappings TO postgres WITH GRANT OPTION;




CREATE TABLE dim_metadatastructuretorepositories  ( 
	extra	xml NULL,
	id	bigserial NOT NULL,
	metadatastructureid	int8 NULL,
	repositoryid	int8 NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;



GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON dim_metadatastructuretorepositories TO postgres WITH GRANT OPTION;
GRANT SELECT(repositoryid), INSERT(repositoryid), UPDATE(repositoryid), REFERENCES(repositoryid) ON dim_metadatastructuretorepositories TO postgres WITH GRANT OPTION;
GRANT SELECT(metadatastructureid), INSERT(metadatastructureid), UPDATE(metadatastructureid), REFERENCES(metadatastructureid) ON dim_metadatastructuretorepositories TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON dim_metadatastructuretorepositories TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON dim_metadatastructuretorepositories TO postgres WITH GRANT OPTION;


CREATE TABLE dim_publications  ( 
	brokerref	int8 NULL,
	datasetversionref	int8 NULL,
	doi	varchar(255) NULL,
	externallink	varchar(255) NULL,
	extra	xml NULL,
	filepath	varchar(255) NULL,
	id	bigserial NOT NULL,
	name	varchar(255) NULL,
	repositoryobjectid	int8 NULL,
	repositoryref	int8 NULL,
	researchobjectid	int8 NULL,
	status	varchar(255) NULL,
	timestamp	timestamp NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;




ALTER TABLE dim_publications
	ADD CONSTRAINT fkdba13b59d9b7f9fc
	FOREIGN KEY(brokerref)
	REFERENCES dim_brokers(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

		
ALTER TABLE dim_publications
	ADD CONSTRAINT fkdba13b5938e553c8
	FOREIGN KEY(datasetversionref)
	REFERENCES datasetversions(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;


	
	
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(timestamp), INSERT(timestamp), UPDATE(timestamp), REFERENCES(timestamp) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(status), INSERT(status), UPDATE(status), REFERENCES(status) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(researchobjectid), INSERT(researchobjectid), UPDATE(researchobjectid), REFERENCES(researchobjectid) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(repositoryref), INSERT(repositoryref), UPDATE(repositoryref), REFERENCES(repositoryref) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(repositoryobjectid), INSERT(repositoryobjectid), UPDATE(repositoryobjectid), REFERENCES(repositoryobjectid) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(name), INSERT(name), UPDATE(name), REFERENCES(name) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(filepath), INSERT(filepath), UPDATE(filepath), REFERENCES(filepath) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(externallink), INSERT(externallink), UPDATE(externallink), REFERENCES(externallink) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(doi), INSERT(doi), UPDATE(doi), REFERENCES(doi) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(datasetversionref), INSERT(datasetversionref), UPDATE(datasetversionref), REFERENCES(datasetversionref) ON dim_publications TO postgres WITH GRANT OPTION;
GRANT SELECT(brokerref), INSERT(brokerref), UPDATE(brokerref), REFERENCES(brokerref) ON dim_publications TO postgres WITH GRANT OPTION;


	
CREATE TABLE dim_repositories  ( 
	brokerref	int8 NULL,
	extra	xml NULL,
	id	bigserial NOT NULL,
	name	varchar(255) NULL,
	url	varchar(255) NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;


ALTER TABLE dim_repositories
	ADD CONSTRAINT fkf833fbebd9b7f9fc
	FOREIGN KEY(brokerref)
	REFERENCES dim_brokers(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
	
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON dim_repositories TO postgres WITH GRANT OPTION;
GRANT SELECT(url), INSERT(url), UPDATE(url), REFERENCES(url) ON dim_repositories TO postgres WITH GRANT OPTION;
GRANT SELECT(name), INSERT(name), UPDATE(name), REFERENCES(name) ON dim_repositories TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON dim_repositories TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON dim_repositories TO postgres WITH GRANT OPTION;
GRANT SELECT(brokerref), INSERT(brokerref), UPDATE(brokerref), REFERENCES(brokerref) ON dim_repositories TO postgres WITH GRANT OPTION;
	
	
ALTER TABLE dim_publications
ADD CONSTRAINT fkdba13b5924447f65
FOREIGN KEY(repositoryref)
REFERENCES dim_repositories(id)
ON DELETE NO ACTION 
ON UPDATE NO ACTION;
	
	
CREATE TABLE dim_transformationrules  ( 
	extra	xml NULL,
	id	bigserial NOT NULL,
	mask	varchar(255) NULL,
	regex	varchar(255) NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;

GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON dim_transformationrules TO postgres WITH GRANT OPTION;
GRANT SELECT(regex), INSERT(regex), UPDATE(regex), REFERENCES(regex) ON dim_transformationrules TO postgres WITH GRANT OPTION;
GRANT SELECT(mask), INSERT(mask), UPDATE(mask), REFERENCES(mask) ON dim_transformationrules TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON dim_transformationrules TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON dim_transformationrules TO postgres WITH GRANT OPTION;

CREATE TABLE entitypermissions  ( 
	entityref	int8 NULL,
	extra	xml NULL,
	id	bigserial NOT NULL,
	key	int8 NULL,
	rights	int4 NULL,
	subjectref	int8 NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;

ALTER TABLE entitypermissions
	ADD CONSTRAINT fk8626ee03f8e8356a
	FOREIGN KEY(entityref)
	REFERENCES entities(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION; 

	
	
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON entitypermissions TO postgres WITH GRANT OPTION;
GRANT SELECT(subjectref), INSERT(subjectref), UPDATE(subjectref), REFERENCES(subjectref) ON entitypermissions TO postgres WITH GRANT OPTION;
GRANT SELECT(rights), INSERT(rights), UPDATE(rights), REFERENCES(rights) ON entitypermissions TO postgres WITH GRANT OPTION;
GRANT SELECT(key), INSERT(key), UPDATE(key), REFERENCES(key) ON entitypermissions TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON entitypermissions TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON entitypermissions TO postgres WITH GRANT OPTION;
GRANT SELECT(entityref), INSERT(entityref), UPDATE(entityref), REFERENCES(entityref) ON entitypermissions TO postgres WITH GRANT OPTION;


CREATE TABLE hibernate_unique_key  ( 
	next_hi	int8 NULL 
	)
WITHOUT OIDS 
TABLESPACE pg_default;

-- Set default values for hibernate_unique_key for users/groups
INSERT INTO hibernate_unique_key(next_hi)
VALUES (2);

GRANT SELECT(next_hi), INSERT(next_hi), UPDATE(next_hi), REFERENCES(next_hi) ON hibernate_unique_key TO postgres WITH GRANT OPTION;

	
CREATE TABLE logins  ( 
	extra	xml NULL,
	id	bigserial NOT NULL,
	loginprovider	varchar(255) NULL,
	providerkey	varchar(255) NULL,
	userref	int8 NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;




ALTER TABLE logins
	ADD CONSTRAINT fk9fe967efbc78f175
	FOREIGN KEY(userref)
	REFERENCES users(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION; 

	
	
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON logins TO postgres WITH GRANT OPTION;
GRANT SELECT(userref), INSERT(userref), UPDATE(userref), REFERENCES(userref) ON logins TO postgres WITH GRANT OPTION;
GRANT SELECT(providerkey), INSERT(providerkey), UPDATE(providerkey), REFERENCES(providerkey) ON logins TO postgres WITH GRANT OPTION;
GRANT SELECT(loginprovider), INSERT(loginprovider), UPDATE(loginprovider), REFERENCES(loginprovider) ON logins TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON logins TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON logins TO postgres WITH GRANT OPTION;



CREATE TABLE operations  ( 
	action	varchar(255) NULL,
	controller	varchar(255) NULL,
	extra	xml NULL,
	featureref	int8 NULL,
	id	bigserial NOT NULL,
	module	varchar(255) NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;



ALTER TABLE operations
	ADD CONSTRAINT fke58bbff8b5c93c76
	FOREIGN KEY(featureref)
	REFERENCES features(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
	
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON operations TO postgres WITH GRANT OPTION;
GRANT SELECT(module), INSERT(module), UPDATE(module), REFERENCES(module) ON operations TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON operations TO postgres WITH GRANT OPTION;
GRANT SELECT(featureref), INSERT(featureref), UPDATE(featureref), REFERENCES(featureref) ON operations TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON operations TO postgres WITH GRANT OPTION;
GRANT SELECT(controller), INSERT(controller), UPDATE(controller), REFERENCES(controller) ON operations TO postgres WITH GRANT OPTION;
GRANT SELECT(action), INSERT(action), UPDATE(action), REFERENCES(action) ON operations TO postgres WITH GRANT OPTION;


CREATE TABLE partyusers  ( 
	extra	xml NULL,
	id	bigserial NOT NULL,
	partyid	int8 NULL,
	partyref	int8 NOT NULL,
	userid	int8 NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;



ALTER TABLE partyusers
	ADD CONSTRAINT fk8c784cab9a4d3305
	FOREIGN KEY(partyref)
	REFERENCES parties(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON partyusers TO postgres WITH GRANT OPTION;
GRANT SELECT(userid), INSERT(userid), UPDATE(userid), REFERENCES(userid) ON partyusers TO postgres WITH GRANT OPTION;
GRANT SELECT(partyref), INSERT(partyref), UPDATE(partyref), REFERENCES(partyref) ON partyusers TO postgres WITH GRANT OPTION;
GRANT SELECT(partyid), INSERT(partyid), UPDATE(partyid), REFERENCES(partyid) ON partyusers TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON partyusers TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON partyusers TO postgres WITH GRANT OPTION;



CREATE TABLE requests  ( 
	applicantref	int8 NULL,
	entityref	int8 NULL,
	extra	xml NULL,
	id	bigserial NOT NULL,
	intention	varchar(255) NULL,
	key	int8 NULL,
	requestdate	timestamp NULL,
	rights	int2 NULL,
	status	int4 NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
)
WITHOUT OIDS 
TABLESPACE pg_default;



ALTER TABLE requests
	ADD CONSTRAINT fkcaafaec6f8e8356a
	FOREIGN KEY(entityref)
	REFERENCES entities(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION; 

	
	
ALTER TABLE requests
	ADD CONSTRAINT fkcaafaec687e8c6c6
	FOREIGN KEY(applicantref)
	REFERENCES users(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
	
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON requests TO postgres WITH GRANT OPTION;
GRANT SELECT(status), INSERT(status), UPDATE(status), REFERENCES(status) ON requests TO postgres WITH GRANT OPTION;
GRANT SELECT(rights), INSERT(rights), UPDATE(rights), REFERENCES(rights) ON requests TO postgres WITH GRANT OPTION;
GRANT SELECT(requestdate), INSERT(requestdate), UPDATE(requestdate), REFERENCES(requestdate) ON requests TO postgres WITH GRANT OPTION;
GRANT SELECT(key), INSERT(key), UPDATE(key), REFERENCES(key) ON requests TO postgres WITH GRANT OPTION;
GRANT SELECT(intention), INSERT(intention), UPDATE(intention), REFERENCES(intention) ON requests TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON requests TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON requests TO postgres WITH GRANT OPTION;
GRANT SELECT(entityref), INSERT(entityref), UPDATE(entityref), REFERENCES(entityref) ON requests TO postgres WITH GRANT OPTION;
GRANT SELECT(applicantref), INSERT(applicantref), UPDATE(applicantref), REFERENCES(applicantref) ON requests TO postgres WITH GRANT OPTION;




CREATE FUNCTION updatepartyname () RETURNS trigger AS
'
      BEGIN
      UPDATE public.parties
      SET name= (SELECT string_agg(value , '' '') AS mainValues
      FROM public.partycustomattributevalues
      inner join public.partycustomattributes on partycustomattributes.id=partycustomattributevalues.customattributeref
      where partycustomattributes.ismain=''Y'' and partyref=NEW.partyref)
      WHERE id=NEW.partyref;
      RETURN NEW;
      END;
      '
LANGUAGE 'plpgsql';



CREATE TRIGGER updatepartyname
	AFTER INSERT OR UPDATE ON public.partycustomattributevalues FOR EACH ROW
	 EXECUTE PROCEDURE updatepartyname();

	 
	 
ALTER TABLE decisions
	ADD CONSTRAINT fk9ae16a6ddcc12c4d
	FOREIGN KEY(requestref)
	REFERENCES requests(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
	
GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON decisions TO postgres WITH GRANT OPTION;
GRANT SELECT(status), INSERT(status), UPDATE(status), REFERENCES(status) ON decisions TO postgres WITH GRANT OPTION;
GRANT SELECT(requestref), INSERT(requestref), UPDATE(requestref), REFERENCES(requestref) ON decisions TO postgres WITH GRANT OPTION;
GRANT SELECT(reason), INSERT(reason), UPDATE(reason), REFERENCES(reason) ON decisions TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON decisions TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON decisions TO postgres WITH GRANT OPTION;
GRANT SELECT(decisionmakerref), INSERT(decisionmakerref), UPDATE(decisionmakerref), REFERENCES(decisionmakerref) ON decisions TO postgres WITH GRANT OPTION;
GRANT SELECT(decisiondate), INSERT(decisiondate), UPDATE(decisiondate), REFERENCES(decisiondate) ON decisions TO postgres WITH GRANT OPTION;

	
	
CREATE TABLE users_groups  ( 
	groupref	int8 NOT NULL,
	userref	int8 NOT NULL,
	PRIMARY KEY(userref,groupref)
)
WITHOUT OIDS 
TABLESPACE pg_default;



ALTER TABLE users_groups
	ADD CONSTRAINT fka8a8eed7bc78f175
	FOREIGN KEY(userref)
	REFERENCES users(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
	
ALTER TABLE users_groups
	ADD CONSTRAINT fka8a8eed741ebee1c
	FOREIGN KEY(groupref)
	REFERENCES groups(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;

	
	
GRANT SELECT(userref), INSERT(userref), UPDATE(userref), REFERENCES(userref) ON users_groups TO postgres WITH GRANT OPTION;
GRANT SELECT(groupref), INSERT(groupref), UPDATE(groupref), REFERENCES(groupref) ON users_groups TO postgres WITH GRANT OPTION;


ALTER TABLE datastructures_views ADD datasetviewref int8 NOT NULL;

ALTER TABLE datastructures_views ADD FOREIGN KEY(viewref) REFERENCES datasetviews(id);

ALTER TABLE datastructures_views ADD FOREIGN KEY(datasetviewref) REFERENCES datasetviews(id);

GRANT SELECT(datasetviewref), INSERT(datasetviewref), UPDATE(datasetviewref), REFERENCES(datasetviewref) ON datastructures_views TO postgres WITH GRANT OPTION;

ALTER TABLE entities ADD COLUMN entitystoretype varchar(255) NULL;
ALTER TABLE entities ADD COLUMN entitytype varchar(255) NULL;
ALTER TABLE entities ADD COLUMN parentref int8 NULL;

ALTER TABLE entities ADD FOREIGN KEY(parentref) REFERENCES entities(id);
	
GRANT SELECT(parentref), INSERT(parentref), UPDATE(parentref), REFERENCES(parentref) ON entities TO postgres WITH GRANT OPTION;

GRANT SELECT(entitytype), INSERT(entitytype), UPDATE(entitytype), REFERENCES(entitytype) ON entities TO postgres WITH GRANT OPTION;

GRANT SELECT(entitystoretype), INSERT(entitystoretype), UPDATE(entitystoretype), REFERENCES(entitystoretype) ON entities TO postgres WITH GRANT OPTION;

ALTER TABLE featurepermissions ADD COLUMN extra	xml;
ALTER TABLE featurepermissions ADD COLUMN permissiontype int4;
ALTER TABLE featurepermissions ADD COLUMN subjectref int8;
ALTER TABLE featurepermissions ADD COLUMN versionno int4;

GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON featurepermissions TO postgres WITH GRANT OPTION;


GRANT SELECT(subjectref), INSERT(subjectref), UPDATE(subjectref), REFERENCES(subjectref) ON featurepermissions TO postgres WITH GRANT OPTION;

GRANT SELECT(permissiontype), INSERT(permissiontype), UPDATE(permissiontype), REFERENCES(permissiontype) ON featurepermissions TO postgres WITH GRANT OPTION;

GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON featurepermissions TO postgres WITH GRANT OPTION;

GRANT SELECT(featureref), INSERT(featureref), UPDATE(featureref), REFERENCES(featureref) ON featurepermissions TO postgres WITH GRANT OPTION;

GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON featurepermissions TO postgres WITH GRANT OPTION;


ALTER TABLE groups ADD COLUMN extra xml NULL;
ALTER TABLE groups ADD COLUMN issystemgroup char(1) NULL;
ALTER TABLE groups ADD COLUMN isvalid char(1) NULL;
ALTER TABLE groups ADD COLUMN name varchar(255) NULL;
ALTER TABLE groups ADD COLUMN versionno	int4;

GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON groups TO postgres WITH GRANT OPTION;

GRANT SELECT(name), INSERT(name), UPDATE(name), REFERENCES(name) ON groups TO postgres WITH GRANT OPTION;

GRANT SELECT(isvalid), INSERT(isvalid), UPDATE(isvalid), REFERENCES(isvalid) ON groups TO postgres WITH GRANT OPTION;

GRANT SELECT(issystemgroup), INSERT(issystemgroup), UPDATE(issystemgroup), REFERENCES(issystemgroup) ON groups TO postgres WITH GRANT OPTION;

GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON groups TO postgres WITH GRANT OPTION;

ALTER TABLE parameters ADD COLUMN orderno int4 NULL;

GRANT SELECT(orderno), INSERT(orderno), UPDATE(orderno), REFERENCES(orderno) ON parameters TO postgres WITH GRANT OPTION;

ALTER TABLE parties ADD COLUMN istemp char(1) NULL;

GRANT SELECT(istemp), INSERT(istemp), UPDATE(istemp), REFERENCES(istemp) ON parties TO postgres WITH GRANT OPTION;

ALTER TABLE partycustomattributes ADD COLUMN displayname varchar(255) NULL;
ALTER TABLE partycustomattributes ADD COLUMN ismain char(1) NULL;
ALTER TABLE partycustomattributes ADD COLUMN isunique char(1) NULL;

GRANT SELECT(isunique), INSERT(isunique), UPDATE(isunique), REFERENCES(isunique) ON partycustomattributes TO postgres WITH GRANT OPTION;

GRANT SELECT(ismain), INSERT(ismain), UPDATE(ismain), REFERENCES(ismain) ON partycustomattributes TO postgres WITH GRANT OPTION;

GRANT SELECT(displayname), INSERT(displayname), UPDATE(displayname), REFERENCES(displayname) ON partycustomattributes TO postgres WITH GRANT OPTION;

ALTER TABLE partyrelationshiptypes ADD COLUMN displayname varchar(255) NULL;

GRANT SELECT(displayname), INSERT(displayname), UPDATE(displayname), REFERENCES(displayname) ON partyrelationshiptypes TO postgres WITH GRANT OPTION;

ALTER TABLE partytypepairs ADD COLUMN partyrelationshiptypedefault char(1) NULL;

ALTER TABLE partytypepairs ADD FOREIGN KEY(alowedsourceref) REFERENCES partytypes(id);

ALTER TABLE partytypepairs ADD FOREIGN KEY(partyrelationshiptyperef) REFERENCES partyrelationshiptypes(id);

GRANT SELECT(partyrelationshiptypedefault), INSERT(partyrelationshiptypedefault), UPDATE(partyrelationshiptypedefault), REFERENCES(partyrelationshiptypedefault) ON partytypepairs TO postgres WITH GRANT OPTION;

ALTER TABLE partytypes ADD COLUMN displayname varchar(255) NULL;

GRANT SELECT(displayname), INSERT(displayname), UPDATE(displayname), REFERENCES(displayname) ON partytypes TO postgres WITH GRANT OPTION;

ALTER TABLE users ADD COLUMN accessfailedcount int4 NULL;
ALTER TABLE users ADD COLUMN extra xml NULL;
ALTER TABLE users ADD COLUMN isemailconfirmed char(1) NULL;
ALTER TABLE users ADD COLUMN isphonenumberconfirmed char(1) NULL;
ALTER TABLE users ADD COLUMN istwofactorenabled char(1) NULL;
ALTER TABLE users ADD COLUMN lockoutenabled char(1) NULL;
ALTER TABLE users ADD COLUMN lockoutenddate timestamp NULL;
ALTER TABLE users ADD COLUMN name varchar(255) NULL;
ALTER TABLE users ADD COLUMN phonenumber varchar(255) NULL;
ALTER TABLE users ADD COLUMN securitystamp varchar(255) NULL;
ALTER TABLE users ADD COLUMN versionno int4;


GRANT SELECT(versionno), INSERT(versionno), UPDATE(versionno), REFERENCES(versionno) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(securitystamp), INSERT(securitystamp), UPDATE(securitystamp), REFERENCES(securitystamp) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(phonenumber), INSERT(phonenumber), UPDATE(phonenumber), REFERENCES(phonenumber) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(password), INSERT(password), UPDATE(password), REFERENCES(password) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(name), INSERT(name), UPDATE(name), REFERENCES(name) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(lockoutenddate), INSERT(lockoutenddate), UPDATE(lockoutenddate), REFERENCES(lockoutenddate) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(lockoutenabled), INSERT(lockoutenabled), UPDATE(lockoutenabled), REFERENCES(lockoutenabled) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(istwofactorenabled), INSERT(istwofactorenabled), UPDATE(istwofactorenabled), REFERENCES(istwofactorenabled) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(isphonenumberconfirmed), INSERT(isphonenumberconfirmed), UPDATE(isphonenumberconfirmed), REFERENCES(isphonenumberconfirmed) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(isemailconfirmed), INSERT(isemailconfirmed), UPDATE(isemailconfirmed), REFERENCES(isemailconfirmed) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(id), INSERT(id), UPDATE(id), REFERENCES(id) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(extra), INSERT(extra), UPDATE(extra), REFERENCES(extra) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(email), INSERT(email), UPDATE(email), REFERENCES(email) ON users TO postgres WITH GRANT OPTION;
GRANT SELECT(accessfailedcount), INSERT(accessfailedcount), UPDATE(accessfailedcount), REFERENCES(accessfailedcount) ON users TO postgres WITH GRANT OPTION;

UPDATE users 
    SET name = (
        SELECT name
        FROM subjects
        WHERE subjects.id = users.id
    );

ALTER TABLE variables ADD COLUMN orderno int4 NULL;

GRANT SELECT(orderno), INSERT(orderno), UPDATE(orderno), REFERENCES(orderno) ON variables TO postgres WITH GRANT OPTION;
  
ALTER TABLE datastructures_views
	ADD CONSTRAINT fk4c16bfbdc6fcb6ca
	FOREIGN KEY(viewref)
	REFERENCES datasetviews(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION;
	
	
ALTER TABLE datastructures_views
	ADD CONSTRAINT fk4c16bfbd6c7b7a32
	FOREIGN KEY(datasetviewref)
	REFERENCES datasetviews(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION ;
	
ALTER TABLE datastructures_views
	ADD FOREIGN KEY(datastructureref)
	REFERENCES datastructures(id);
	
ALTER TABLE dim_mappings
	ADD CONSTRAINT fk103ffbe8e6235279
	FOREIGN KEY(transformationruleref)
	REFERENCES dim_transformationrules(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION ;
	
ALTER TABLE dim_publications ADD FOREIGN KEY(repositoryref) REFERENCES dim_repositories(id);
	
ALTER TABLE partytypepairs
	ADD FOREIGN KEY(alowedsourceref)
	REFERENCES partytypes(id);

ALTER TABLE partytypepairs
	ADD FOREIGN KEY(partyrelationshiptyperef)
	REFERENCES partyrelationshiptypes(id);
	
	
UPDATE researchplans 
  SET versionno=1, extra=NULL, title='none', description='If no research plan is used.' WHERE id=1;
INSERT INTO researchplans (id, versionno, extra, title, description)
  VALUES (2,1,NULL,'Research plan','');	
  
  
ALTER TABLE users DROP CONSTRAINT IF EXISTS fk2c1c7fe54884f298;
ALTER TABLE users DROP CONSTRAINT fk2c1c7fe5639f638e;
ALTER TABLE users DROP CONSTRAINT fk2c1c7fe598856323;

--- part two --

ALTER TABLE partycustomattributevalues
ALTER	value TYPE	text ;

ALTER TABLE partystatustypes   
ALTER	description TYPE	text ;

ALTER TABLE dim_brokers   
ADD	link	varchar(255) ;

ALTER TABLE partytypepairs   
ADD	conditionsource varchar(255),
ADD	conditiontarget varchar(255),
ALTER	description	TYPE text;
	
INSERT INTO logins (versionno,loginprovider,providerkey,userref) 
Select 1, 'ldap', 'ldap', id From Users where authenticatorref = 2;

Select * from logins;
	
ALTER TABLE users
DROP COLUMN authenticatorref,
DROP COLUMN fullname,
DROP COLUMN isapproved,
DROP COLUMN isblocked,
DROP COLUMN islockedout,
DROP COLUMN lastactivitydate,
DROP COLUMN lastlockoutdate	,
DROP COLUMN lastlogindate	 ,
DROP COLUMN lastpasswordchangedate	,
DROP COLUMN lastpasswordfailuredate	,
DROP COLUMN lastsecurityanswerfailuredate,
DROP COLUMN passwordfailurecount,
DROP COLUMN passwordsalt,
DROP COLUMN registrationdate,
DROP COLUMN securityanswer,
DROP COLUMN securityanswerfailurecount,
DROP COLUMN securityanswersalt,
DROP COLUMN securityquestionref,
ALTER versionno TYPE int4 ;

DROP TABLE logentries;
CREATE TABLE logentries  ( 
	assemblyname	varchar(255) NULL,
	assemblyversion	varchar(255) NULL,
	classname	varchar(255) NULL,
	cultureid	varchar(255) NULL,
	desription	varchar(255) NULL,
	destinationobjectid	varchar(255) NULL,
	destinationobjecttype	varchar(255) NULL,
	discriminator	varchar(10) NOT NULL,
	entitystate	int4 NULL,
	environemt	varchar(1000) NULL,
	extrainfo	varchar(255) NULL,
	groupid	varchar(255) NULL,
	id	bigserial NOT NULL,
	logtype	int4 NULL,
	methodname	varchar(255) NULL,
	objectid	varchar(255) NULL,
	objecttype	varchar(255) NULL,
	parameters	varchar(255) NULL,
	parametervalues	varchar(255) NULL,
	processingtime	int8 NULL,
	relationstate	int4 NULL,
	requesturl	varchar(255) NULL,
	returntype	varchar(255) NULL,
	returnvalue	varchar(255) NULL,
	sourceobjectid	varchar(255) NULL,
	sourceobjecttype	varchar(255) NULL,
	userid	varchar(255) NULL,
	utcdate	timestamp NULL,
	versionno	int4 NOT NULL,
	PRIMARY KEY(id)
);

ALTER TABLE partystatuses   
ALTER	description	TYPE text ;

ALTER TABLE partytypes   
ALTER	description	TYPE text;

-- BEGIN TRANSACTION;  
-- ROLLBACK TRANSACTION;

-- PERMISSION STUFF 
-- features
-- BEGIN TRANSACTION;  
-- ROLLBACK TRANSACTION;

-- create Modules Features
INSERT INTO features (name,description,parentref, extra,versionno) 
VALUES
('Data Discovery', 'Data Discovery', null, null, 1),
('Data Dissemination', 'Data Dissemination', null, null, 1),
('Data Planning Management', 'Data Planning Management', null, null, 1);

--update
UPDATE features SET name='Users', description='', parentref=2 WHERE name='Users Management' AND parentref = 2;
UPDATE features SET name='Groups', description='', parentref=2 WHERE name='Groups Management' AND parentref = 2;
UPDATE features SET name='Entity Permissions', description='', parentref=2 WHERE name='Data Management' AND parentref = 2;

--search
UPDATE features SET name ='Search Management', parentref=
(Select id from features where name = 'Data Discovery') 
WHERE name='Search' AND parentref = 2;

UPDATE features SET name ='Search', parentref=
(Select id from features where name = 'Data Discovery') 
WHERE name='Search' AND parentref = 1;

UPDATE features SET parentref=null WHERE name='Data Collection' AND parentref = 1;
UPDATE features SET name='Dataset Upload', description='' WHERE name='Dataset Submission' AND parentref = 
(Select id from features where name = 'Data Collection');

UPDATE features SET name='Data Planning Management', description='' WHERE name='Research Plan' AND parentref = 1;
-- get data from permissions and featurepermissions table and store it into the featurepermissions

CREATE TABLE featurepermissions2
(
  id bigserial NOT NULL,
  versionno integer NOT NULL,
  extra xml,
  featureref bigint,
  permissiontype integer NOT NULL,
  subjectref bigint,
  CONSTRAINT featurepermissions_pkey2 PRIMARY KEY (id),
  CONSTRAINT fk4491d68bb5c93c763 FOREIGN KEY (featureref)
      REFERENCES features (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
);

-- copy data

INSERT INTO featurepermissions2(featureref,permissiontype,subjectref,versionno)
SELECT featureref, p.permissiontype as permissiontype,p.subjectref as subjectref,1 as versionno 
from featurepermissions, permissions as p WHERE p.id = featurepermissions.id;

DROP TABLE featurepermissions;

ALTER TABLE featurepermissions2 RENAME TO featurepermissions;  

ALTER TABLE featurepermissions
RENAME CONSTRAINT featurepermissions_pkey2 To featurepermissions_pkey;
ALTER TABLE featurepermissions
RENAME CONSTRAINT fk4491d68bb5c93c763 To fk4491d68bb5c93c76;

ALTER SEQUENCE featurepermissions2_id_seq RENAME To featurepermissions_id_seq;

-- end features

-- data permissions

-- set data permissions to entity permissions
INSERT INTO entitypermissions(entityref, extra ,key,rights,subjectref,versionno)
Select 1 as entityref, null as extra ,key ,rights ,subjectref,1 as versionno
FROM
(
Select filteredTabelle.key as key, filteredTabelle.subjectref as subjectref, (SUM(filteredTabelle.newrights)) as rights
FROM  
(
Select distinct dp.dataid as key, p.subjectref as subjectref, CASE WHEN dp.righttype=1 THEN 1
	WHEN dp.righttype=2 THEN 4
	WHEN dp.righttype=3 THEN 8
	WHEN dp.righttype=4 THEN 2
	WHEN dp.righttype=5 THEN 16
	ELSE 0
	END as newrights
	FROM (datapermissions as dp inner join permissions as p ON dp.id = p.id)
order by key, subjectref, newrights	
) as filteredTabelle
GROUP BY filteredTabelle.key, filteredTabelle.subjectref
) as result
WHERE rights>0;
-- end data permissions

-- entities
UPDATE entities
   SET entitystoretype='BExIS.Xml.Helpers.DatasetStore, BExIS.Xml.Helpers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null', 
   entitytype='BExIS.Dlm.Entities.Data.Dataset, BExIS.Dlm.Entities, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
 WHERE id = 1;
--

ALTER TABLE permissions
DROP CONSTRAINT fkea223c4ce251315c;


ALTER TABLE datapermissions
DROP CONSTRAINT fkd489fdda5f494856;
DROP TABLE permissions;

-- END PERMISSION STUFF

ALTER TABLE variables 
ALTER description	TYPE varchar(255);

ALTER TABLE partyrelationshiptypes 
ALTER	description	TYPE text;

update groups set versionno = 1;
ALTER TABLE groups 
ALTER	versionno	SET NOT NULL;

ALTER TABLE entities   
DROP COLUMN assemblypath,
DROP COLUMN classpath,
ALTER	securable	SET DEFAULT false,
ALTER	usemetadata	SET DEFAULT false;

ALTER TABLE entities
	ADD CONSTRAINT fk341d04684991bf3f
	FOREIGN KEY (parentref)
	REFERENCES entities(id)
	ON DELETE NO ACTION 
	ON UPDATE NO ACTION; 
	
ALTER TABLE partycustomattributes   
ADD	condition varchar(255),
ALTER	validvalues TYPE	text ;

--ALTER SEQUENCE partycustomattributes_id_seq RESTART WITH 5;
--ALTER SEQUENCE partytypes_id_seq RESTART WITH 3;
--ALTER SEQUENCE partystatustypes_id_seq RESTART WITH 5;
--ALTER SEQUENCE partytypepairs_id_seq RESTART WITH 10;


ALTER TABLE datacontainers
ALTER scope TYPE varchar(255) ;

ALTER TABLE parameters
ALTER description	TYPE varchar(255) ;
	

ALTER TABLE partyrelationships   
ALTER	description	TYPE text;

ALTER TABLE partytypepairs
DROP CONSTRAINT IF EXISTS partytypepairs_partyrelationshiptyperef_fkey1,
DROP CONSTRAINT IF EXISTS partytypepairs_partyrelationshiptyperef_fkey,
DROP CONSTRAINT IF EXISTS partytypepairs_alowedsourceref_fkey1,
DROP CONSTRAINT IF EXISTS partytypepairs_alowedsourceref_fkey,
DROP CONSTRAINT IF EXISTS fk7f0af543d4a0d58a;


ALTER TABLE units
ALTER COLUMN dimensionref SET NOT NULL;

UPDATE units SET name ='none'
WHERE name='None';

update users set versionno = 1;
ALTER TABLE users 
ALTER COLUMN versionno SET NOT NULL;	
	
-- BEGIN TRANSACTION;  
-- ROLLBACK TRANSACTION;
DROP TABLE groups_users;


Update groups
SET name = s.name
FROM subjects as s
WHERE groups.id = s.id;


ALTER TABLE groups
DROP CONSTRAINT fkba94c18e4884f298;
DROP TABLE subjects;
	
DROP TABLE securityquestions;

ALTER TABLE datastructures_views
DROP CONSTRAINT fk4c16bfbd5a69dbe6;
DROP TABLE dataviews;

DROP TABLE authenticators;
DROP TABLE tasks;
DROP TABLE datapermissions;

-- drop constraints
ALTER TABLE datastructures_views
DROP CONSTRAINT datastructures_views_viewref_fkey;
ALTER TABLE datastructures_views
DROP CONSTRAINT datastructures_views_datastructureref_fkey;
ALTER TABLE datastructures_views
DROP CONSTRAINT datastructures_views_datasetviewref_fkey;
ALTER TABLE dim_publications
DROP CONSTRAINT dim_publications_repositoryref_fkey;
ALTER TABLE entities
DROP CONSTRAINT entities_parentref_fkey;

DROP INDEX IF EXISTS idx_entities_id;
DROP INDEX IF EXISTS idx_entities_name;
DROP INDEX IF EXISTS idx_features_id;
DROP INDEX IF EXISTS idx_features_name;
DROP INDEX IF EXISTS idx_users_email;

CREATE MATERIALIZED VIEW mvdataset1
	AS
	SELECT t.id,
    t.orderno,
    t."timestamp",
    t.datasetversionref AS versionid,
        CASE
            WHEN ((xpath('/Content/Item[Property[@Name="VariableId" and @value="1"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::text = '{""}'::text) THEN NULL::character varying
            WHEN ((xpath('/Content/Item[Property[@Name="VariableId" and @value="1"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::text = '{_null_null}'::text) THEN NULL::character varying
            ELSE (unnest((xpath('/Content/Item[Property[@Name="VariableId" and @value="1"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::character varying[]))::character varying(255)
        END AS var1,
        CASE
            WHEN ((xpath('/Content/Item[Property[@Name="VariableId" and @value="2"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::text = '{""}'::text) THEN NULL::character varying
            WHEN ((xpath('/Content/Item[Property[@Name="VariableId" and @value="2"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::text = '{_null_null}'::text) THEN NULL::character varying
            ELSE (unnest((xpath('/Content/Item[Property[@Name="VariableId" and @value="2"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::character varying[]))::character varying(255)
        END AS var2,
        CASE
            WHEN ((xpath('/Content/Item[Property[@Name="VariableId" and @value="3"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::text = '{""}'::text) THEN NULL::character varying
            WHEN ((xpath('/Content/Item[Property[@Name="VariableId" and @value="3"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::text = '{_null_null}'::text) THEN NULL::character varying
            ELSE (unnest((xpath('/Content/Item[Property[@Name="VariableId" and @value="3"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::character varying[]))::character varying(255)
        END AS var3,
        CASE
            WHEN ((xpath('/Content/Item[Property[@Name="VariableId" and @value="4"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::text = '{""}'::text) THEN NULL::character varying
            WHEN ((xpath('/Content/Item[Property[@Name="VariableId" and @value="4"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::text = '{_null_null}'::text) THEN NULL::character varying
            ELSE (unnest((xpath('/Content/Item[Property[@Name="VariableId" and @value="4"]][1]/Property[@Name="Value"]/@value'::text, t.xmlvariablevalues))::character varying[]))::character varying(255)
        END AS var4
   FROM (datasetversions v
     JOIN datatuples t ON ((t.datasetversionref = v.id)))
  WHERE (((v.datasetref = 1) AND (v.status = 2)) OR ((v.datasetref = 1) AND (v.status = 0)))
WITH DATA;

-->update public data and remove everyone group <--
UPDATE entitypermissions
SET subjectref = 0
Where subjectref = 
(Select id 
From groups w
Where name = 'everyone');

DELETE FROM groups
WHERE name = 'everyone'; 

UPDATE users
SET 
isemailconfirmed = 'Y',
accessfailedcount = 0,
lockoutenabled = 'N',
securitystamp = (SELECT md5(password||id)::uuid);

COMMIT;