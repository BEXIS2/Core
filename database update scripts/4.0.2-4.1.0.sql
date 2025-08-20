BEGIN TRANSACTION;

-- update names of existing datatypes
-- Name;Description;System Type;Display Pattern
-- Text;A unicode string;String;
-- Floating Point Number;A precise real number with a restricted range;Double;
-- Integer;An 64 bit integer number;Int64;
-- Boolean;A boolean value;Boolean;
-- Date and Time;A date with / or time;DateTime;
-- Date;A date;DateTime;DateIso
-- Time;A time;DateTime;Time


-- 
/*
Update dim_linkelements
Set
	xpath = xpath ||'/' || SUBSTRING(xpath FROM LENGTH(xpath) - STRPOS(REVERSE(xpath), '/') + 2)|| 'Type'
 where type  in (5,6) and complexity = 1 and xpath !='';
	*/

BEGIN TRANSACTION;

-- TEXT - string
update datatypes
	set 
	name = 'Text',
	description = 'A unicode string'
	where id = (select id systemtype from datatypes where systemtype = 'String'  and name =	'text' 
		order by id
		limit 1);



-- select * from datatypes where name = 'Text'

update datatypes
	set 
	name = 'String',
	description = 'A unicode string'
	where id = (select id systemtype from datatypes where systemtype = 'String'  and name =	'string' 
		order by id
		limit 1);



select id from datatypes where systemtype = 'String';

-- update all variables with string
update variables 
set datatyperef = (select id from datatypes where name = 'String') 
where datatyperef in (select id from datatypes where systemtype = 'String' and name=	'String');

-- update datacontainers with string
update datacontainers 
set datatyperef = (select id from datatypes where name = 'Text') 
where datatyperef in (select id from datatypes where systemtype = 'String' and name=	'Text');


-- Integer - Int64
select * from datatypes where name = 'Integer';

select id from datatypes where SystemType like 'Int%';

update datatypes
set 
name = 'Integer',
description = 'An 64 bit integer number'
where id = (select id systemtype from datatypes where systemtype = 'Int64' 
order by id
limit 1);

-- update all variables with Integer
update variables 
set datatyperef = (select id from datatypes where name = 'Integer') 
where datatyperef in (select id from datatypes where SystemType like '%Int%');

-- update datacontainers with Integer
update datacontainers 
set datatyperef = (select id from datatypes where name = 'Integer') 
where datatyperef in (select id from datatypes where SystemType like '%Int%');

-- Floating Point Number - double
select * from datatypes where name = 'Floating Point Number';

select id from datatypes where SystemType = 'Decimal' OR SystemType = 'Double';
select id from datatypes where SystemType = 'Double';

update datatypes
set 
name = 'Floating Point Number',
description = 'A precise real number with a restricted range'
where id = (select id systemtype from datatypes where systemtype = 'Double' 
order by id
limit 1);

-- update all variables with Floating Point Number
update variables 
set datatyperef = (select id from datatypes where name = 'Floating Point Number') 
where datatyperef in (select id from datatypes where SystemType = 'Decimal' OR SystemType = 'Double');

-- update datacontainers with Floating Point Number
update datacontainers 
set datatyperef = (select id from datatypes where name = 'Floating Point Number') 
where datatyperef in (select id from datatypes where SystemType = 'Decimal' OR SystemType = 'Double');


-- Date and Time - DateTime
	
select * from datatypes where systemtype = 'DateTime';

update datatypes
set 
name = 'Date and Time',
description = 'A date with / or time'
where id = (select id systemtype from datatypes where systemtype = 'DateTime' and name = 'datetime' 
order by id
limit 1);

update datatypes
set 
name = 'Date',
description = 'A date'
where id = (select id systemtype from datatypes where systemtype = 'DateTime' and name = 'date' 
order by id
limit 1);

update datatypes
set 
name = 'Time',
description = 'A time'
where id = (select id systemtype from datatypes where systemtype = 'DateTime' and name = 'time' 
order by id
limit 1);


-- update all variables with Floating Point Number
update variables 
set datatyperef = (select id from datatypes where name = 'Date and Time') 
where datatyperef in (select id from datatypes where SystemType = 'DateTime');

-- update datacontainers with Floating Point Number
update datacontainers 
set datatyperef = (select id from datatypes where name = 'Date and Time') 
where datatyperef in (select id from datatypes where SystemType = 'DateTime');



-- Boolean
update datatypes
set 
name = 'Boolean',
description = 'A boolean value'
where id = (select id systemtype from datatypes where systemtype = 'Boolean' 
order by id
limit 1);

-- update all variables with Floating Point Number
update variables 
set datatyperef = (select id from datatypes where name = 'Boolean') 
where datatyperef in (select id from datatypes where SystemType = 'Boolean');

-- update datacontainers with Floating Point Number
update datacontainers 
set datatyperef = (select id from datatypes where name = 'Boolean') 
where datatyperef in (select id from datatypes where SystemType = 'Boolean');

DO $$
DECLARE
    -- Integer
    intId integer := (select id from datatypes where name = 'Integer');
    intArray integer[]; -- Correctly declared as an array
    -- Floating Point Number
    floatId integer := (select id from datatypes where name = 'Floating Point Number');
    floatArray integer[]; -- Correctly declared as an array
    -- Text
    textId integer := (select id from datatypes where name = 'Text');
    textArray integer[]; -- Correctly declared as an array
    -- Date and Time
    dateTimeId integer := (select id from datatypes where name = 'Date and Time');
    dateArray integer[]; -- Correctly declared as an array#
    -- Boolean
    booleanId integer := (select id from datatypes where name = 'Boolean');
    booleanArray integer[]; -- Correctly declared as an array

    unitIds integer[]; -- To hold all unit IDs as an array
    unitId integer;    -- Scalar variable for the loop
BEGIN

    -- set arrays using ARRAY_AGG to collect multiple IDs into an array
    -- Adding ORDER BY is good practice for consistent results, though not strictly necessary for ANY/EXISTS checks
	--  select * from datatypes WHERE SystemType LIKE '%Int%';
    SELECT array_agg(id ORDER BY id) INTO intArray FROM datatypes WHERE SystemType LIKE '%Int%';
    SELECT array_agg(id ORDER BY id) INTO floatArray FROM datatypes WHERE SystemType = 'Decimal' OR SystemType = 'Double';
    SELECT array_agg(id ORDER BY id) INTO textArray FROM datatypes WHERE SystemType = 'String';
    SELECT array_agg(id ORDER BY id) INTO dateArray FROM datatypes WHERE SystemType = 'DateTime';
    Select array_agg(id ORDER BY id) INTO booleanArray FROM datatypes WHERE SystemType = 'Boolean';

    -- Populate unitIds array
    SELECT array_agg(id ORDER BY id) INTO unitIds FROM units;
    
    -- Add logging to verify array contents (for debugging)
    RAISE NOTICE 'intArray: %', intArray;
    RAISE NOTICE 'floatArray: %', floatArray;
    RAISE NOTICE 'textArray: %', textArray;
    RAISE NOTICE 'dateArray: %', dateArray;
    RAISE NOTICE 'unitIds: %', unitIds;
    RAISE NOTICE 'booleanArray: %', booleanArray;

    -- Check if any of the arrays are NULL (e.g., if no matching datatypes were found)
    -- This helps prevent errors if intArray, etc., are NULL.
    -- The ANY operator handles NULL arrays gracefully, but it's good to be aware.

    -- für jede unit in unitIds
    FOR unitId IN SELECT unnest(unitIds) LOOP

        RAISE NOTICE 'Processing unitId: %', unitId;

        -- Integer
        IF intId IS NOT NULL AND intArray IS NOT NULL THEN -- Check if intArray is not NULL
            IF EXISTS (SELECT 1 FROM units_datatypes WHERE datatyperef = ANY(intArray) AND unitref = unitId) THEN
                -- If the 'Integer' type is already directly associated, delete others of its system type.
                DELETE FROM units_datatypes
                WHERE datatyperef = ANY(intArray) -- Corrected: Use ANY
                AND datatyperef <> intId
                AND unitref = unitId;
            -- ELSE
            --     -- If 'Integer' type is NOT directly associated, delete ALL Int* types, then insert intId.
            --     DELETE FROM units_datatypes WHERE datatyperef = ANY(intArray) AND unitref = unitId; -- Corrected: Use ANY
            --     INSERT INTO units_datatypes (datatyperef, unitref) VALUES (intId, unitId);
            END IF;
        END IF;

        -- Floating Point Number
        IF floatId IS NOT NULL AND floatArray IS NOT NULL THEN -- Check if floatArray is not NULL
            IF EXISTS (SELECT 1 FROM units_datatypes WHERE datatyperef = ANY(floatArray) AND unitref = unitId) THEN
                DELETE FROM units_datatypes
                WHERE datatyperef = ANY(floatArray) -- Corrected: Use ANY
                AND datatyperef <> floatId
                AND unitref = unitId;
            -- ELSE
            --     DELETE FROM units_datatypes WHERE datatyperef = ANY(floatArray) AND unitref = unitId; -- Corrected: Use ANY
            --     INSERT INTO units_datatypes (datatyperef, unitref) VALUES (floatId, unitId);
            END IF;
        END IF;

        -- Text
        IF textId IS NOT NULL AND textArray IS NOT NULL THEN -- Check if textArray is not NULL
            IF EXISTS (SELECT 1 FROM units_datatypes WHERE datatyperef = ANY(textArray) AND unitref = unitId) THEN
                DELETE FROM units_datatypes
                WHERE datatyperef = ANY(textArray) -- Corrected: Use ANY
                AND datatyperef <> textId
                AND unitref = unitId;
            -- ELSE
            --     DELETE FROM units_datatypes WHERE datatyperef = ANY(textArray) AND unitref = unitId; -- Corrected: Use ANY
            --     INSERT INTO units_datatypes (datatyperef, unitref) VALUES (textId, unitId);
            END IF;
        END IF;

        -- Date and Time
        -- The logic is aligned with other blocks now, assuming the intent is consistent:
        -- if dateTimeId exists for the unit, keep it and remove others from dateArray
        -- if dateTimeId doesn't exist, remove all from dateArray and add dateTimeId
        IF dateTimeId IS NOT NULL AND dateArray IS NOT NULL THEN -- Check if dateArray is not NULL
            IF EXISTS (SELECT 1 FROM units_datatypes WHERE datatyperef = ANY(dateArray) AND unitref = unitId) THEN
                DELETE FROM units_datatypes
                WHERE datatyperef = ANY(dateArray) -- Corrected: Use ANY
                AND datatyperef <> dateTimeId
                AND unitref = unitId;
            -- ELSE
            --     DELETE FROM units_datatypes WHERE datatyperef = ANY(dateArray) AND unitref = unitId; -- Corrected: Use ANY
            --     INSERT INTO units_datatypes (datatyperef, unitref) VALUES (dateTimeId, unitId);
            END IF;
        END IF;

        -- Boolean
        IF booleanId IS NOT NULL AND booleanArray IS NOT NULL THEN -- Check if booleanArray is not NULL
			
            IF EXISTS (SELECT 1 FROM units_datatypes WHERE datatyperef = ANY(booleanArray) AND unitref = unitId) THEN
				RAISE NOTICE 'Processing booleanId: %', booleanId;
				RAISE NOTICE 'Processing booleanArray: %', booleanArray;
			
                DELETE FROM units_datatypes
                WHERE datatyperef = ANY(booleanArray) -- Corrected: Use ANY
                AND datatyperef <> booleanId
                AND unitref = unitId;
            -- ELSE
            --     DELETE FROM units_datatypes WHERE datatyperef = ANY(booleanArray) AND unitref = unitId; -- Corrected: Use ANY
            --     INSERT INTO units_datatypes (datatyperef, unitref) VALUES (booleanId, unitId);
            END IF;
        END IF;

    END LOOP;

END $$;


Delete
from datatypes
where name not in (
    'String',
    'Text',
    'Integer',
    'Floating Point Number',
    'Date and Time',
    'Date',
    'Time',
    'Boolean'
);




-- missing author and description update to empty strings
update datasetversions
set 
m_performer = ''
where m_performer is null;

update datasetversions
set 
m_comment = ''
where m_comment is null;

-- reset all datatypes to units
delete from units_datatypes where unitref = (select id from units where name like 'none');
INSERT INTO units_datatypes (datatyperef, unitref)
SELECT id, (select id from units where name like 'none')
from datatypes;

-- Citation Concept Mapping Updates

INSERT INTO public.dim_mappingconcepts(
version, name, description, url, xsd) 
SELECT 1,'Citation', 'The concept is needed to create a citation string', '', ''
WHERE NOT EXISTS (SELECT * FROM public.dim_mappingconcepts WHERE name='Citation');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Title','Title of citation string.', '', false, false, (select id from dim_mappingconcepts where name='Citation'), 'data/title'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/title');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Version','Dataset version of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation'), 'data/version'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/version');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Year','Dataset publish year of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation'), 'data/year'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/year');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Doi','Dataset DOI of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation'), 'data/doi'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/doi');


INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'EntityType','Entity Type of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation'), 'data/entityType'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/entityType');


INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'Projects','Dataset projects of citation string.', '', true, false, (select id from dim_mappingconcepts where name='Citation'), 'data/projects'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/projects');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'Project','Dataset project of citation string.', '', true, false, 
			(select id from dim_mappingconcepts where name='Citation'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='Projects'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation')),
			'data/projects/project'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/projects/project');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'AuthorNames','Dataset author names of citation string.', '', false, false, (select id from dim_mappingconcepts where name='Citation'), 'data/authorNames'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/authorNames');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'AuthorName','Dataset author name of citation string.', '', false, false, 
			(select id from dim_mappingconcepts where name='Citation'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='AuthorNames'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation')),
			'data/authorNames/authorName'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/authorNames/authorName');


--Create feature and operation for citation api
INSERT INTO public.features(
versionno, extra, description, name, parentref)
SELECT 1, null, 'Citation Api', 'Citation Api', 13
WHERE NOT EXISTS (SELECT * FROM public.features WHERE name='Citation Api');

INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'Citation', '*', (Select id from features where name = 'Citation Api')
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='Citation');

-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.1.0',NOW());

commit;