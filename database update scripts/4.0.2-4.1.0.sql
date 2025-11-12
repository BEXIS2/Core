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
	description = 'A special data type for very long sequences of character.'
	where id = (select id systemtype from datatypes where systemtype = 'String'  and name =	'text' 
		order by id
		limit 1);



-- select * from datatypes where name = 'Text'

update datatypes
	set 
	name = 'String',
	description = 'A sequence of characters, such as letters, numbers, or symbols (e.g., IDs).'
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
description = 'A data type for whole numbers. In this case, it corresponds to Int64, a 64-bit integer that can represent large values.'
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
description = 'A floating-point number with double precision (for decimal values, e.g., 3.14159).'
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
description = 'A data type for storing both a calendar date and a time.'
where id = (select id systemtype from datatypes where systemtype = 'DateTime' and name = 'datetime' 
order by id
limit 1);

update datatypes
set 
name = 'Date',
description = 'A data type for storing a calendar date without time.'
where id = (select id systemtype from datatypes where systemtype = 'DateTime' and name = 'date' 
order by id
limit 1);

update datatypes
set 
name = 'Time',
description = 'A data type for storing a time of day without a date.'
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
description = 'A logical data type with only two possible values: true or false; 0 or 1.'
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
SELECT 'Projects','Dataset projects of citation string.', '', true, true, (select id from dim_mappingconcepts where name='Citation'), 'data/projects'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/projects');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept,parentRef, xpath)
SELECT 'Project','Dataset project of citation string.', '', true, false, 
			(select id from dim_mappingconcepts where name='Citation'), 
			(SELECT id FROM public.dim_mappingkeys WHERE name='Projects'and concept = (SELECT id FROM public.dim_mappingconcepts WHERE name='Citation')),
			'data/projects/project'
WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='Citation') AND xpath='data/projects/project');

INSERT INTO public.dim_mappingkeys (name,description, url, optional, iscomplex, concept, xpath)
SELECT 'AuthorNames','Dataset author names of citation string.', '', false, true, (select id from dim_mappingconcepts where name='Citation'), 'data/authorNames'
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

-- DataCite
--- dim.repositories
DO $$
DECLARE
    cnt INT;
BEGIN
    IF EXISTS (
        SELECT 1 FROM public.dim_repositories
        WHERE LOWER(name) LIKE '%datacite%'
    ) THEN
        UPDATE public.dim_repositories
        SET name = 'DataCite'
        WHERE LOWER(name) LIKE '%datacite%';

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Updated % rows', cnt;
    ELSE
        INSERT INTO public.dim_repositories (name)
        VALUES ('DataCite');

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    END IF;
END $$;

--- dim.brokers
DO $$
DECLARE
    cnt INT;
BEGIN
    IF EXISTS (
        SELECT 1 FROM public.dim_brokers
        WHERE LOWER(name) LIKE '%datacite%'
    ) THEN
        UPDATE public.dim_brokers
        SET name = 'DataCite', type = '', repositoryref = (select id from public.dim_repositories where name = 'DataCite') 
        WHERE LOWER(name) LIKE '%datacite%';

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Updated % rows', cnt;
    ELSE
        INSERT INTO public.dim_brokers (name, type, repositoryref)
        VALUES ('DataCite', '', (select id from public.dim_repositories where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    END IF;
END $$;

--- dim.mappingconcepts
DO $$
DECLARE
    cnt INT;
BEGIN
    IF EXISTS (
        SELECT 1 FROM public.dim_mappingconcepts
        WHERE LOWER(name) LIKE '%datacite%'
    ) THEN
        UPDATE public.dim_mappingconcepts
        SET name = 'DataCite' 
        WHERE LOWER(name) LIKE '%datacite%';

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Updated % rows', cnt;
    ELSE
        INSERT INTO public.dim_mappingconcepts (name)
        VALUES ('DataCite');

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    END IF;
END $$;

--- dim.mappingkeys
---- type
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/type' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Type', '', '', false, false, 'data/type', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- event
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/event' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Event', '', '', false, false, 'data/attributes/event', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- creators
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Creators', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/creator/#id1', false, true, 'data/attributes/creators', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.name
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/name' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Name', '', '', false, false, 'data/attributes/creators/name', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.givenName
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/givenName' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('GivenName', '', '', true, false, 'data/attributes/creators/givenName', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.familyName
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/familyName' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('FamilyName', '', '', true, false, 'data/attributes/creators/familyName', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.nameType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/nameType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('NameType', '', '', true, false, 'data/attributes/creators/nameType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.nameIdentifiers
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/nameIdentifiers' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('NameIdentifiers', '', '', true, true, 'data/attributes/creators/nameIdentifiers', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ creators.nameIdentifiers.nameIdentifier
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/nameIdentifiers/nameIdentifier' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('NameIdentifier', '', '', false, false, 'data/attributes/creators/nameIdentifiers/nameIdentifier', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ creators.nameIdentifiers.nameIdentifierScheme
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/nameIdentifiers/nameIdentifierScheme' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('NameIdentifierScheme', '', '', false, false, 'data/attributes/creators/nameIdentifiers/nameIdentifierScheme', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ creators.nameIdentifiers.schemeUri
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/nameIdentifiers/nameIdentifierScheme' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SchemeUri', '', '', false, false, 'data/attributes/creators/nameIdentifiers/schemeUri', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.affiliation
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/affiliation' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Affiliation', '', '', true, true, 'data/attributes/creators/affiliation', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.affiliation.affiliationIdentifier
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/affiliation/affiliationIdentifier' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('AffiliationIdentifier', '', '', true, false, 'data/attributes/creators/affiliation/affiliationIdentifier', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.affiliation.affiliationIdentifierScheme
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/affiliation/affiliationIdentifierScheme' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('AffiliationIdentifierScheme', '', '', true, false, 'data/attributes/creators/affiliation/affiliationIdentifierScheme', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.affiliation.name
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/affiliation/name' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Name', '', '', false, false, 'data/attributes/creators/affiliation/name', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.affiliation.schemeUri
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/affiliation/schemeUri' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SchemeUri', '', '', true, false, 'data/attributes/creators/affiliation/schemeUri', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- creators.lang
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/creators/lang' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Language', '', '', true, false, 'data/attributes/creators/lang', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/creators' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- titles
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/titles' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Titles', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/title/#id1', false, true, 'data/attributes/titles', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- titles.title
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/titles/title' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/titles' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Title', '', '', false, false, 'data/attributes/titles/title', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/titles' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- titles.lang
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/titles/lang' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/titles' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Language', '', '', true, false, 'data/attributes/titles/lang', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/titles' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- titles.titleType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/titles/titleType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/titles' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('TitleType', '', '', true, false, 'data/attributes/titles/titleType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/titles' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- publisher
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Publisher', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/publisher/#id1', false, true, 'data/attributes/publisher', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- publisher.name
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/publisher/name' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Name', '', '', false, false, 'data/attributes/publisher/name', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- publisher.publisherIdentifier
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/publisher/publisherIdentifier' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('PublisherIdentifier', '', '', true, false, 'data/attributes/publisher/publisherIdentifier', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- publisher.publisherIdentifierScheme
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/publisher/publisherIdentifierScheme' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('PublisherIdentifierScheme', '', '', false, false, 'data/attributes/publisher/publisherIdentifierScheme', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- publisher.schemeUri
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/publisher/schemeUri' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SchemeUri', '', '', true, false, 'data/attributes/publisher/schemeUri', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- publisher.lang
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/publisher/lang' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Language', '', '', true, false, 'data/attributes/publisher/lang', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/publisher' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- publicationYear
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/publicationYear' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('PublicationYear', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/publicationyear/#id1', true, false, 'data/attributes/publicationYear', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- subjects
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Subjects', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.6/properties/subject/', true, true, 'data/attributes/subjects', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- subjects.subject
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/subjects/subject' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Subject', '', '', false, false, 'data/attributes/subjects/subject', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- subjects.subjectScheme
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/subjects/subjectScheme' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SubjectScheme', '', '', true, false, 'data/attributes/subjects/subjectScheme', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- subjects.schemeUri
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/subjects/schemeUri' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SchemeUri', '', '', true, false, 'data/attributes/subjects/schemeUri', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- subjects.valueUri
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/subjects/valueUri' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('ValueUri', '', '', true, false, 'data/attributes/subjects/valueUri', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- subjects.lang
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/subjects/lang' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Language', '', '', true, false, 'data/attributes/subjects/lang', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- subjects.classificationCode
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/subjects/classificationCode' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('ClassificationCode', '', '', true, false, 'data/attributes/subjects/classificationCode', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/subjects' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- contributors
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Contributors', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/contributor/#id1', true, true, 'data/attributes/contributors', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.name
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/name' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Name', '', '', false, false, 'data/attributes/contributors/name', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.givenName
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/givenName' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('GivenName', '', '', true, false, 'data/attributes/contributors/givenName', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.familyName
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/familyName' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('FamilyName', '', '', true, false, 'data/attributes/contributors/familyName', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.nameType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/nameType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('NameType', '', '', true, false, 'data/attributes/contributors/nameType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.contributorType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/contributorType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('ContributorType', '', '', true, false, 'data/attributes/contributors/contributorType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.nameIdentifiers
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/nameIdentifiers' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('NameIdentifiers', '', '', true, true, 'data/attributes/contributors/nameIdentifiers', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ contributors.nameIdentifiers.nameIdentifier
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/nameIdentifiers/nameIdentifier' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('NameIdentifier', '', '', false, false, 'data/attributes/contributors/nameIdentifiers/nameIdentifier', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ contributors.nameIdentifiers.nameIdentifierScheme
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/nameIdentifiers/nameIdentifierScheme' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('NameIdentifierScheme', '', '', false, false, 'data/attributes/contributors/nameIdentifiers/nameIdentifierScheme', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ contributors.nameIdentifiers.schemeUri
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/nameIdentifiers/nameIdentifierScheme' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SchemeUri', '', '', false, false, 'data/attributes/contributors/nameIdentifiers/schemeUri', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/nameIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.affiliation
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/affiliation' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Affiliation', '', '', true, true, 'data/attributes/contributors/affiliation', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.affiliation.affiliationIdentifier
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/affiliation/affiliationIdentifier' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('AffiliationIdentifier', '', '', true, false, 'data/attributes/contributors/affiliation/affiliationIdentifier', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.affiliation.affiliationIdentifierScheme
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/affiliation/affiliationIdentifierScheme' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('AffiliationIdentifierScheme', '', '', true, false, 'data/attributes/contributors/affiliation/affiliationIdentifierScheme', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.affiliation.name
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/affiliation/name' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Name', '', '', false, false, 'data/attributes/contributors/affiliation/name', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.affiliation.schemeUri
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/affiliation/schemeUri' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SchemeUri', '', '', true, false, 'data/attributes/contributors/affiliation/schemeUri', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors/affiliation' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- contributors.lang
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/contributors/lang' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Language', '', '', true, false, 'data/attributes/contributors/lang', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/contributors' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- dates
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/dates' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Dates', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/date/#id1', true, true, 'data/attributes/dates', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- dates.date
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/dates/date' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/dates' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Date', '', '', false, false, 'data/attributes/dates/date', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/dates' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- dates.dateType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/dates/dateType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/dates' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('DateType', '', '', false, false, 'data/attributes/dates/dateType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/dates' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- dates.dateInformation
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/dates/dateInformation' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/dates' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('DateInformation', '', '', true, false, 'data/attributes/dates/dateInformation', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/dates' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- language
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/language' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Language', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/language/#id1', true, false, 'data/attributes/language', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- types
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/types' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Types', '', '', false, true, 'data/attributes/types', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- types.resourceTypeGeneral
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/types/resourceTypeGeneral' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/types' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('ResourceTypeGeneral', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/resourcetype/#a-resourcetypegeneral', false, false, 'data/attributes/types/resourceTypeGeneral', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/types' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- types.resourceTypeGeneral
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/types/resourceType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/types' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('ResourceType', '', '', false, false, 'data/attributes/types/resourceType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/types' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- relatedIdentifiers
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('RelatedIdentifiers', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/relatedidentifier/#id1', true, true, 'data/attributes/relatedIdentifiers', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- relatedIdentifiers.relatedIdentifier
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/relatedIdentifiers/relatedIdentifier' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('RelatedIdentifier', '', '', false, false, 'data/attributes/relatedIdentifiers/relatedIdentifier', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- relatedIdentifiers.relatedIdentifierType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/relatedIdentifiers/relatedIdentifierType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('RelatedIdentifierType', '', '', false, false, 'data/attributes/relatedIdentifiers/relatedIdentifierType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- relatedIdentifiers.relationType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/relatedIdentifiers/relationType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('RelationType', '', '', false, false, 'data/attributes/relatedIdentifiers/relationType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- relatedIdentifiers.relatedMetadataScheme
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/relatedIdentifiers/relatedMetadataScheme' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('RelatedMetadataScheme', '', '', true, false, 'data/attributes/relatedIdentifiers/relatedMetadataScheme', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- relatedIdentifiers.schemeURI
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/relatedIdentifiers/schemeURI' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SchemeURI', '', '', true, false, 'data/attributes/relatedIdentifiers/schemeURI', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- relatedIdentifiers.schemeType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/relatedIdentifiers/schemeType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SchemeType', '', '', true, false, 'data/attributes/relatedIdentifiers/schemeType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- relatedIdentifiers.resourceTypeGeneral
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/relatedIdentifiers/resourceTypeGeneral' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('ResourceTypeGeneral', '', '', true, false, 'data/attributes/relatedIdentifiers/resourceTypeGeneral', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/relatedIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- sizes
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/sizes' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Sizes', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/size/', true, false, 'data/attributes/sizes', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- formats
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/formats' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Formats', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/format/', true, false, 'data/attributes/formats', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- rightsList
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('RightsList', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/rights/#id1', true, true, 'data/attributes/rightsList', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- rightsList.rights
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/rightsList/rights' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Rights', '', '', false, false, 'data/attributes/rightsList/rights', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- rightsList.rightsUri
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/rightsList/rightsUri' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('RightsUri', '', '', true, false, 'data/attributes/rightsList/rightsUri', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- rightsList.schemeUri
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/rightsList/schemeUri' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SchemeUri', '', '', true, false, 'data/attributes/rightsList/schemeUri', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- rightsList.rightsIdentifier
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/rightsList/rightsIdentifier' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('RightsIdentifier', '', '', true, false, 'data/attributes/rightsList/rightsIdentifier', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- rightsList.rightsIdentifierScheme
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/rightsList/rightsIdentifierScheme' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('RightsIdentifierScheme', '', '', true, false, 'data/attributes/rightsList/rightsIdentifierScheme', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- rightsList.lang
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/rightsList/lang' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Lang', '', '', true, false, 'data/attributes/rightsList/lang', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/rightsList' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- descriptions
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/descriptions' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Descriptions', '', '', true, true, 'data/attributes/descriptions', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- descriptions.lang
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/descriptions/lang' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/descriptions' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Lang', '', '', true, false, 'data/attributes/descriptions/lang', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/descriptions' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- descriptions.description
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/descriptions/description' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/descriptions' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('Description', '', '', false, false, 'data/attributes/descriptions/description', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/descriptions' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- descriptions.descriptionType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/descriptions/descriptionType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/descriptions' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('DescriptionType', '', '', false, false, 'data/attributes/descriptions/descriptionType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/descriptions' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- geoLocations
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/geoLocations' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('GeoLocations', '', '', true, true, 'data/attributes/geoLocations', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- geoLocations.geoLocationPlace
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/geoLocations/geoLocationPlace' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('GeoLocationPlace', '', '', true, false, 'data/attributes/geoLocations/geoLocationPlace', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- geoLocations.geoLocationPoint
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/geoLocations/geoLocationPoint' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('GeoLocationPoint', '', '', true, true, 'data/attributes/geoLocations/geoLocationPoint', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ geoLocations.geoLocationPoint.pointLongitude
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/geoLocations/geoLocationPoint/pointLongitude' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationPoint' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('PointLongitude', '', '', true, false, 'data/attributes/geoLocations/geoLocationPoint/pointLongitude', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationPoint' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ geoLocations.geoLocationPoint.pointLatitude
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/geoLocations/geoLocationPoint/pointLatitude' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationPoint' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('PointLatitude', '', '', true, false, 'data/attributes/geoLocations/geoLocationPoint/pointLatitude', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationPoint' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- geoLocations.geoLocationBox
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/geoLocations/geoLocationBox' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('GeoLocationBox', '', '', true, true, 'data/attributes/geoLocations/geoLocationBox', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ geoLocations.geoLocationBox.westBoundLongitude
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/geoLocations/geoLocationBox/westBoundLongitude' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationBox' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('WestBoundLongitude', '', '', true, false, 'data/attributes/geoLocations/geoLocationBox/westBoundLongitude', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationBox' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ geoLocations.geoLocationBox.eastBoundLongitude
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/geoLocations/geoLocationBox/eastBoundLongitude' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationBox' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('EastBoundLongitude', '', '', true, false, 'data/attributes/geoLocations/geoLocationBox/eastBoundLongitude', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationBox' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ geoLocations.geoLocationBox.southBoundLatitude
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/geoLocations/geoLocationBox/southBoundLatitude' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationBox' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('SouthBoundLatitude', '', '', true, false, 'data/attributes/geoLocations/geoLocationBox/southBoundLatitude', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationBox' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

------ geoLocations.geoLocationBox.northBoundLatitude
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/geoLocations/geoLocationBox/northBoundLatitude' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationBox' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('NorthBoundLatitude', '', '', true, false, 'data/attributes/geoLocations/geoLocationBox/northBoundLatitude', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/geoLocations/geoLocationBox' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- fundingReferences
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('fundingReferences', '', '', true, true, 'data/attributes/fundingReferences', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- fundingReferences.funderName
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/fundingReferences/funderName' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('FunderName', '', '', false, false, 'data/attributes/fundingReferences/funderName', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- fundingReferences.funderIdentifier
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/fundingReferences/funderIdentifier' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('FunderIdentifier', '', '', false, false, 'data/attributes/fundingReferences/funderIdentifier', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- fundingReferences.funderIdentifierType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/fundingReferences/funderIdentifierType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('FunderIdentifierType', '', '', false, false, 'data/attributes/fundingReferences/funderIdentifierType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- fundingReferences.awardNumber
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/fundingReferences/awardNumber' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('AwardNumber', '', '', true, false, 'data/attributes/fundingReferences/awardNumber', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- fundingReferences.awardURI
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/fundingReferences/awardURI' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('AwardURI', '', '', true, false, 'data/attributes/fundingReferences/awardURI', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- fundingReferences.awardTitle
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/fundingReferences/awardTitle' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('AwardTitle', '', '', true, false, 'data/attributes/fundingReferences/awardTitle', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/fundingReferences' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- alternateIdentifiers
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/alternateIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('AlternateIdentifiers', '', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/alternateidentifier/#id1', true, true, 'data/attributes/alternateIdentifiers', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- alternateIdentifiers.alternateIdentifierType
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/alternateIdentifiers/alternateIdentifierType' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/alternateIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('AlternateIdentifierType', '', '', false, false, 'data/attributes/alternateIdentifiers/alternateIdentifierType', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/alternateIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

----- alternateIdentifiers.alternateIdentifier
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/alternateIdentifiers/alternateIdentifier' AND parentref = (select id from public.dim_mappingkeys where xpath = 'data/attributes/alternateIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite'))
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept, parentref)
        VALUES ('AlternateIdentifier', '', '', false, false, 'data/attributes/alternateIdentifiers/alternateIdentifier', (select id from public.dim_mappingconcepts where name = 'DataCite'), (select id from public.dim_mappingkeys where xpath = 'data/attributes/alternateIdentifiers' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- xml
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/xml' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Xml', '', '', true, false, 'data/attributes/xml', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- url
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/url' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('Url', '', '', true, false, 'data/attributes/url', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

---- schemaVersion
DO $$
DECLARE
    cnt INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public.dim_mappingkeys
        WHERE xpath = 'data/attributes/schemaVersion' AND concept = (select id from public.dim_mappingconcepts where name = 'DataCite')
    ) THEN

        INSERT INTO public.dim_mappingkeys (name, description, url, optional, iscomplex, xpath, concept)
        VALUES ('SchemaVersion', '', '', true, false, 'data/attributes/schemaVersion', (select id from public.dim_mappingconcepts where name = 'DataCite'));

        GET DIAGNOSTICS cnt = ROW_COUNT;
        RAISE NOTICE 'Inserted % rows', cnt;
    ELSE
        RAISE NOTICE 'Nothing to insert!';
    END IF;
END $$;

-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.1.0',NOW());

commit;
