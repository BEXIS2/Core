
-- Alter Table datatuples ADD COLUMN jsonvariablevalues Text;
-- Alter Table datatuples DROP COLUMN IF EXISTS jsonvariablevalues;


-- Alter Table datatuples ADD COLUMN values Text;
-- Alter Table datatuples DROP COLUMN IF EXISTS values;

-- Select * from datatuples;

-- DELETE XmlVariableValuesColumn and all Materialized Views
-- Alter Table datatuples DROP COLUMN IF EXISTS xmlvariablevalues CASCADE;
-- Alter Table datatupleversions DROP COLUMN IF EXISTS xmlvariablevalues;

DO
$do$
DECLARE

varIds text[];
values text[];
xml_data xml;
datatuple_ids bigint[];
datatupleAsJson text;
valuesAsJson text; 
vv text;
vv2 text;
value text;

-- for datasetversion 
datasetversion_ids bigint[];
t text;
d text;

-- User DisplayName
pu_user_ids bigint[];
partyid bigint;
partyname text;
x bigint;


BEGIN
    
	-- DATATUPLES
	
	Alter Table datatuples ADD COLUMN jsonvariablevalues Text;
	Alter Table datatuples ADD COLUMN values Text;
	
    datatuple_ids := ARRAY(Select id from datatuples as t);
    IF array_length(datatuple_ids, 1)>0
    THEN
        for y in 1..array_upper(datatuple_ids,1)
        LOOP
            x = datatuple_ids[y];
            --get one datatuple
            xml_data := (Select xmlvariablevalues from datatuples as t where t.id=x);
            
            -- get ids
            varIds := (select xpath('/Content/Item/Property[@Name="VariableId"]/@value',xml_data)::text[] x);
            values := (select xpath('/Content/Item/Property[@Name="Value"]/@value',xml_data)::text[] x);
            
            -- raise notice 'DATATUPLE START: %',x;

            datatupleAsJson := '';
            valuesAsJson := '';
            
            for i in 1..array_upper(varIds,1) loop
                                
            -- raise notice '{"vid": % , "v": % }', varIds[i],values[i];
            
            -- json column
            vv := (Select CONCAT('{ "vid": "',varIds[i],'", "v": "',values[i],'"}'));
            
            datatupleAsJson := (SELECT
            CASE WHEN i=1 Then (Select CONCAT(datatupleAsJson,vv))
                ELSE (Select CONCAT(datatupleAsJson, ',',vv))
            END);

            -- values column
            value = values[i];

                -- if  value is empty
                IF (value = '') IS NOT FALSE  THEN value = 'null';
                END IF;
                -- if value is _null_null
                IF (value = '_null_null') THEN value = 'null';   
                END IF;

                IF (value = 'null') THEN vv2 :=(Select CONCAT(value));
                ELSE vv2 :=(Select CONCAT('"',value,'"'));
                END IF;

            valuesAsJson := (SELECT
            CASE WHEN i=1 Then (Select CONCAT(valuesAsJson,vv2))
                ELSE (Select CONCAT(valuesAsJson, ',',vv2))
            END);

            end loop;

            datatupleAsJson := (Select CONCAT('[',datatupleAsJson, ']'));
            valuesAsJson := (Select CONCAT('{',valuesAsJson, '}'));
            
            -- raise notice 'JSON : % ',datatupleAsJson;
            -- raise notice 'VALUES : % ', valuesAsJson;
            
            -- raise notice '------------------------';
            
            -- insert json into json variable column
            
            UPDATE datatuples
            SET jsonvariablevalues = datatupleAsJson, values = valuesAsJson
            WHERE id = x;
            
        
            
        END LOOP;
    END IF;
	
	-- DATATUPLES END
	-- DATATUPLEVERSIONS

    Alter Table datatupleversions ADD COLUMN jsonvariablevalues Text;
    Alter Table datatupleversions ADD COLUMN values Text;

	datatuple_ids := ARRAY(Select id from datatupleversions as t);

	IF array_length(datatuple_ids, 1)>0
    THEN
        for y in 1..array_upper(datatuple_ids,1)
        LOOP

            x = datatuple_ids[y];
            --get one datatuple
            xml_data := (Select xmlvariablevalues from datatupleversions as t where t.id=x);
            
            -- get ids
            varIds := (select xpath('/Content/Item/Property[@Name="VariableId"]/@value',xml_data)::text[] x);
            values := (select xpath('/Content/Item/Property[@Name="Value"]/@value',xml_data)::text[] x);
            
            -- raise notice 'DATATUPLE Versions START: %',x;

            datatupleAsJson := '';
            valuesAsJson := '';
            
            for i in 1..array_upper(varIds,1) loop
                                
            -- raise notice '{"vid": % , "v": % }', varIds[i],values[i];
            


            -- json column
            vv := (Select CONCAT('{ "vid": "',varIds[i],'", "v": "',values[i],'"}'));
            
            datatupleAsJson := (SELECT
            CASE WHEN i=1 Then (Select CONCAT(datatupleAsJson,vv))
                ELSE (Select CONCAT(datatupleAsJson, ',',vv))
            END);

            -- values column

            value = values[i];

                -- if  value is empty
                IF (value = '') IS NOT FALSE  THEN value = 'null';
                END IF;
                -- if value is _null_null
                IF (value = '_null_null') THEN value = 'null';   
                END IF;

                IF (value = 'null') THEN vv2 :=(Select CONCAT(value));
                ELSE vv2 :=(Select CONCAT('"',value,'"'));
                END IF;

            valuesAsJson := (SELECT
            CASE WHEN i=1 Then (Select CONCAT(valuesAsJson,vv2))
                ELSE (Select CONCAT(valuesAsJson, ',',vv2))
            END);

            end loop;

            datatupleAsJson := (Select CONCAT('[',datatupleAsJson, ']'));
            valuesAsJson := (Select CONCAT('{',valuesAsJson, '}'));
            
            -- raise notice 'JSON : % ',datatupleAsJson;
            -- raise notice 'VALUES : % ', valuesAsJson;
            
            -- raise notice '------------------------';
            
            -- insert json into json variable column
            
            UPDATE datatupleversions
            SET jsonvariablevalues = datatupleAsJson, values = valuesAsJson
            WHERE id = x;
            
        END LOOP;
    END IF;
	-- DATATUPLEVERSIONS END
	
-- Select jsonvariablevalues, values from datatuples;
-- Select jsonvariablevalues, values from datatupleversions;

Alter Table datatuples DROP COLUMN IF EXISTS xmlvariablevalues CASCADE;
Alter Table datatupleversions DROP COLUMN IF EXISTS xmlvariablevalues;


-- Datasetversions - add title & description column and fill it
Alter Table datasetversions ADD COLUMN Title Text;
Alter Table datasetversions ADD COLUMN Description Text;

datasetversion_ids:= ARRAY(select id from datasetversions);

for y in 1..array_upper(datasetversion_ids,1)
LOOP
    x = datasetversion_ids[y];   
t := (Select (xpath('//' || (SELECT
  		(xpath('//nodeRef/@value',xml_element))[1] AS "value"
        FROM (
          SELECT unnest(xpath('//nodeReferences',m.extra)) AS xml_element FROM datasetversions dv left join datasets ds  on ds.id = dv.datasetref left join metadatastructures m  on ds.metadatastructureref= m.id  where dv.id = x
        ) t where (xpath('//nodeRef/@name',xml_element))[1]::varchar = 'title') || '/text()', metadata))[1] as "test"  From  datasetversions dv  where dv.id = x
    	);
d := (Select (xpath('//' || (SELECT
  		(xpath('//nodeRef/@value',xml_element))[2] AS "value"
        FROM (
          SELECT unnest(xpath('//nodeReferences',m.extra)) AS xml_element FROM datasetversions dv left join datasets ds  on ds.id = dv.datasetref left join metadatastructures m  on ds.metadatastructureref= m.id  where dv.id = x
        ) t where (xpath('//nodeRef/@name',xml_element))[2]::varchar = 'description') || '/text()', metadata))[1] as "test"  From  datasetversions dv  where dv.id = x
    	);
        
-- raise notice '{"id": % , "title": % , "description": %  }', x, t, d;     

UPDATE datasetversions
SET title = t, description = d
WHERE id = x;
        
end loop;

-- ALTER TABLE DatasetVersions DROP Title;
-- ALTER TABLE DatasetVersions DROP Description;
-- Select * from datasetversions;
-- Select title,description from datasetversions;

-- update descriptions from different Tables
ALTER TABLE groups
ALTER COLUMN description TYPE character varying;

ALTER TABLE parties
ALTER COLUMN description TYPE character varying;

-- DisplayName  User And Group
ALTER TABLE public.users
    ALTER COLUMN token TYPE character varying(255) COLLATE pg_catalog."default";

ALTER TABLE public.users
    ADD COLUMN displayname character varying(255) COLLATE pg_catalog."default";

ALTER TABLE public.groups
    ADD COLUMN displayname character varying(255) COLLATE pg_catalog."default";

-- Update Displayname User
 
pu_user_ids := ARRAY(Select userid from partyusers as t);
 -- raise notice 'pu_user_ids : % ',pu_user_ids;
-- select id, name, displayname from users order by id;
-- select id,name from parties;
-- select * from partyusers;

-- update all displaynames with username

update 
	public.users
set 
	displayname = name; 
-- check if party exist and replace with party name

for y in 1..array_upper(pu_user_ids,1)
Loop
	x = pu_user_ids[y];
	-- raise notice 'x : % ',x;
	partyid:= (Select partyref from partyusers where userid = x);
	-- raise notice 'partyid : % ',partyid;
	partyname:= (Select name from parties where id = partyid);
	-- raise notice 'partyname : % ',partyname;
	Update
		users
	Set
		displayname = partyname
	Where
		id = x;
	
END LOOP;

-- update displayname groups

update 
	public.groups
set 
	displayname = name; 

-- UPDATE Dimensions
UPDATE dimensions SET specification='L(1,0)M(0,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 2;
UPDATE dimensions SET specification='L(0,0)M(1,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 3;
UPDATE dimensions SET specification='L(0,0)M(0,0)T(1,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 4;
UPDATE dimensions SET specification='L(0,0)M(0,0)T(0,0)I(1,0)?(0,0)N(0,0)J(0,0)' where id = 5;
UPDATE dimensions SET specification='L(0,0)M(0,0)T(0,0)I(0,0)?(1,0)N(0,0)J(0,0)' where id = 6;
UPDATE dimensions SET specification='L(0,0)M(0,0)T(0,0)I(0,0)?(0,0)N(1,0)J(0,0)' where id = 7;
UPDATE dimensions SET specification='L(0,0)M(0,0)T(0,0)I(0,0)?(0,0)N(0,0)J(1,0)' where id = 8;
UPDATE dimensions SET specification='L(2,0)M(1,0)T(0,3)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 9;
UPDATE dimensions SET specification='L(0,0)M(0,0)T(0,1)I(0,0)?(0,0)N(1,0)J(0,0)' where id = 10;
UPDATE dimensions SET specification='L(0,0)M(0,0)T(0,1)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 11;
UPDATE dimensions SET specification='L(0,2)M(0,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 12;
UPDATE dimensions SET specification='L(0,0)M(0,1)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 13;
UPDATE dimensions SET specification='L(1,0)M(0,0)T(0,1)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 14;
UPDATE dimensions SET specification='L(2,0)M(0,1)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 15;
UPDATE dimensions SET specification='L(0,2)M(1,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 16;
UPDATE dimensions SET specification='L(0,3)M(1,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 17;
UPDATE dimensions SET specification='L(3,0)M(0,1)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 18;
UPDATE dimensions SET specification='L(0,0)M(0,1)T(0,1)I(0,0)?(0,0)N(1,0)J(0,0)' where id = 19;
UPDATE dimensions SET specification='L(0,0)M(1,0)T(0,3)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 20;
UPDATE dimensions SET specification='L(1,0)M(0,1)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 21;
UPDATE dimensions SET specification='L(0,0)M(0,1)T(0,0)I(0,0)?(0,0)N(1,0)J(0,0)' where id = 22;
UPDATE dimensions SET specification='L(0,2)M(0,0)T(0,1)I(0,0)?(0,0)N(1,0)J(0,0)' where id = 23;
UPDATE dimensions SET specification='L(2,0)M(0,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 24;
UPDATE dimensions SET specification='L(3,0)M(0,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 25;
UPDATE dimensions SET specification='L(0,1)M(1,0)T(0,2)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 26;
UPDATE dimensions SET specification='L(2,0)M(1,0)T(0,3)I(0,1)?(0,0)N(0,0)J(0,0)' where id = 27;
UPDATE dimensions SET specification='L(2,0)M(1,0)T(0,2)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 28;
UPDATE dimensions SET specification='L(0,0)M(1,0)T(0,2)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 29;
UPDATE dimensions SET specification='L(0,0)M(0,0)T(1,0)I(1,0)?(0,0)N(0,0)J(0,0)' where id = 30;
UPDATE dimensions SET specification='L(0,1)M(0,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 31;
UPDATE dimensions SET specification='L(2,0)M(1,0)T(0,3)I(0,2)?(0,0)N(0,0)J(0,0)' where id = 32;
UPDATE dimensions SET specification='L(0,3)M(0,1)T(3,0)I(2,0)?(0,0)N(0,0)J(0,0)' where id = 33;
UPDATE dimensions SET specification='L(0,0)M(1,1)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 34;
UPDATE dimensions SET specification='L(3,3)M(0,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 35;
UPDATE dimensions SET specification='L(2,2)M(0,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 36;
UPDATE dimensions SET specification='L(3,2)M(0,0)T(0,0)I(0,0)?(0,0)N(0,0)J(0,0)' where id = 37;

-- Table: public.versions

-- DROP TABLE public.versions;
-- DROP SEQUENCE versions_id_seq;

CREATE SEQUENCE public.versions_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.versions_id_seq
    OWNER TO postgres;

CREATE TABLE public.versions
(
    id bigint NOT NULL DEFAULT nextval('versions_id_seq'::regclass),
    versionno integer NOT NULL,
    extra xml,
    module character varying(255) COLLATE pg_catalog."default",
    value character varying(255) COLLATE pg_catalog."default",
    date timestamp without time zone,
    CONSTRAINT versions_pkey PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.versions
    OWNER to postgres;

INSERT INTO public.versions(
	id, versionno, extra, module, value, date)
	VALUES (1, 1, null, 'Shell', '2.14.0',NOW());



-- change matching phrase data type

ALTER TABLE public.constraints
    ALTER COLUMN matchingphrase TYPE text ;

-- Drop Missing Value in Variable
ALTER TABLE public.variables DROP COLUMN missingvalue;


-- Create Index
CREATE INDEX "None"
    ON public.variables(unitref);
	
CREATE INDEX idx_dataattributeref_variables
    ON public.variables USING btree
    (dataattributeref ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE INDEX idx_datastructureref_variables
    ON public.variables USING btree
    (datastructureref ASC NULLS LAST)
    TABLESPACE pg_default;



END
$do$