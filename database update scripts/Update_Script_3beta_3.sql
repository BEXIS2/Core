BEGIN TRANSACTION;

-- add missing data types to none - unit
DELETE FROM public.units_datatypes
WHERE unitref=1;

INSERT INTO public.units_datatypes (datatyperef, unitref)
SELECT id, 1
FROM datatypes;


-- create meanings from all var temps
INSERT INTO rpm_meanings (name, description, versionno, approved, selectable)
SELECT label, description, 1, true,true
FROM variables
WHERE variablestype = 'VAR_TEMPL';

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
$do$




-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.0.0',NOW());





commit;