ALTER TABLE entities ADD COLUMN securable BOOLEAN DEFAULT TRUE;
ALTER TABLE entities ADD COLUMN usemetadata BOOLEAN DEFAULT TRUE;

CREATE TABLE parties (
    id bigint NOT NULL,
    versionno integer NOT NULL,
    extra xml,
    name character varying(255),
    alias character varying(255),
    description character varying(255),
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
    description character varying(255),
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
    description character varying(255),
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
    description character varying(255),
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
    description character varying(255),
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
    description character varying(255),
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
    description character varying(255),
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
    description character varying(255)
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

-----------------------------
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


