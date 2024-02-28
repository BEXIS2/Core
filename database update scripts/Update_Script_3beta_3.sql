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




-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.0.0',NOW());


commit;