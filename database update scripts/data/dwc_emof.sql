DO
$do$
DECLARE

dwclinks text[] := ARRAY[
'measurementAccuracy',
'measurementDeterminedBy',
'measurementDeterminedDate',
'measurementID',
'measurementMethod',
'measurementRemarks',
'measurementType',
'measurementUnit',
'measurementValue',
'parentMeasurementID'    
];

obislinks text[] := ARRAY[
'measurementTypeID',
'measurementValueID',
'measurementUnitID'
];

begin

-- dwc
INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT 'dwc','http://rs.tdwg.org/dwc/terms/',1, null, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name='dwc');

INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT 'obis','http://rs.iobis.org/obis/terms/',1, null, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name='obis');


INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT 'hasDwcTerm','na', 6, null, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name='hasDwcTerm');

for i in array_lower(dwclinks, 1)..array_upper(dwclinks, 1) loop

RAISE NOTICE 'name: %',dwclinks[i];

INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT dwclinks[i],dwclinks[i], 5, (SELECT b1.id FROM public.rpm_externallink b1 WHERE name='dwc'), null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name=dwclinks[i]);

end loop;

for i in array_lower(dwclinks, 1)..array_upper(dwclinks, 1) loop

RAISE NOTICE 'name: %',obislinks[i];

INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT dwclinks[i],dwclinks[i], 5, (SELECT b1.id FROM public.rpm_externallink b1 WHERE name='obis'), null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name=dwclinks[i]);

end loop;


END
$do$;
