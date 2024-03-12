BEGIN TRANSACTION;

-- add missing data types to none - unit
DELETE FROM public.units_datatypes
WHERE unitref=1;

INSERT INTO public.units_datatypes (datatyperef, unitref)
SELECT id, 1
FROM datatypes;


ALTER TABLE rpm_meanings
ALTER COLUMN description 
TYPE TEXT;

-- create meanings from all var temps
INSERT INTO rpm_meanings (name, description, versionno, approved, selectable)
SELECT label, description, 1, true,true
FROM variables
WHERE variablestype = 'VAR_TEMPL';

-- add intentions
ALTER TABLE public.requests
    ALTER COLUMN intention TYPE text COLLATE pg_catalog."default";

-- 
ALTER TABLE IF EXISTS public.variables
	 ALTER COLUMN approved DROP DEFAULT;

-- add default & fixed value
ALTER TABLE IF EXISTS public.metadataattributeusages
    ADD COLUMN defaultvalue character varying(255) COLLATE pg_catalog."default";

ALTER TABLE IF EXISTS public.metadataattributeusages
    ADD COLUMN fixedvalue character varying(255) COLLATE pg_catalog."default";

				ALTER TABLE IF EXISTS public.metadatanestedattributeusages
    ADD COLUMN defaultvalue character varying(255) COLLATE pg_catalog."default";

ALTER TABLE IF EXISTS public.metadatanestedattributeusages
    ADD COLUMN fixedvalue character varying(255) COLLATE pg_catalog."default";

ALTER TABLE IF EXISTS public.metadataparameterusages
    ADD COLUMN defaultvalue character varying(255) COLLATE pg_catalog."default";

ALTER TABLE IF EXISTS public.metadataparameterusages
    ADD COLUMN fixedvalue character varying(255) COLLATE pg_catalog."default";


-- add seeddata
DO
$do$
DECLARE

dwclinks text[] := ARRAY[
'occurrenceID',
'basisOfRecord',
'scientificName',
'eventDate',
'countryCode',
'taxonRank',
'kingdom',
'decimalLatitude',
'decimalLongitude',
'geodeticDatum',
'coordinateUncertaintyInMeters',
'individualCount',
'organismQuantity',
'organismQuantityType',
'informationWithheld',
'dataGeneralizations',
'eventTime',
'country',
'eventID',
'eventDate',
'samplingProtocol',
'samplingSizeUnit',
'samplingSizeValue',
'parentEventID',
'samplingEffort',
'locationID',
'footprintWKT',
'occurrenceStatus'         
];

releationshipTypes text[] := ARRAY[
'hasContextObject',
'hasObjectOfInterest',
'hasMatrix',
'hasProperty'   
];

begin

-- dwc
INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT 'dwc','http://rs.tdwg.org/dwc/terms/',1, null, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name='dwc');

INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT 'hasDwcTerm','na', 6, null, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name='hasDwcTerm');

for i in array_lower(dwclinks, 1)..array_upper(dwclinks, 1) loop

RAISE NOTICE 'name: %',dwclinks[i];

INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT dwclinks[i],dwclinks[i], 5, 1, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name=dwclinks[i]);

end loop;


-- releationships
INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT 'i-adopt','https://i-adopt.github.io/#/',1, null, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name='i-adopt');

for i in array_lower(releationshipTypes, 1)..array_upper(releationshipTypes, 1) loop

INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT releationshipTypes[i],releationshipTypes[i], 6, 30, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name=releationshipTypes[i]);

end loop;

END
$do$;


-- list of display pattern
-- xpath('Extra/DisplayPattern/Name:text)
DO
$do$
DECLARE

displaypattern text[] := ARRAY[
'DateTimeIso',       
'DateIso',           
'DateUs',            
'DateUs yy',         
'DateUs M/d/yyyy',   
'DateTimeUs h:m',    
'DateUk',            
'DateUk yy',         
'DateEu',            
'DateEu yy',         
'Time',              
'Time mm:ss',        
'Time hh:mm',        
'Time 12h',          
'Time 12h hh:mm',    
'yyyy-M-d',          
'yyyy-d-M',          
'yyyy-MM-dd',        
'yyyy-dd-MM',        
'd/M/yyyy hh:mm:ss tt',
'Year',              
'Month'          
];

dp_name text := 'DateUs';
i int := 0;

BEGIN

i = array_position(displaypattern, dp_name);

update variables as v
set displaypatternid = array_position(displaypattern,(select unnest(xpath('Extra/DisplayPattern/Name/text()',extra))::text from datatypes where id = v.datatyperef))
where v.datatyperef in (Select id from datatypes where systemtype = 'DateTime' and extra is not null);

update variables as v
set displaypatternid = 0
where v.datatyperef in (Select id from datatypes where systemtype = 'DateTime' and extra is null);

-- Select id, datatyperef, displaypatternid from variables where datatyperef in (3,4,13);

END
$do$;

-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.1.0',NOW());

commit;