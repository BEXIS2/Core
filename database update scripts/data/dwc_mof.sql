DO
$do$
DECLARE

dwclinks text[] := ARRAY[
'measurementAccuracy'
'measurementDeterminedBy'
'measurementDeterminedDate'
'measurementID'
'measurementMethod'
'measurementRemarks'
'measurementType'
'measurementUnit'
'measurementValue'
'parentMeasurementID'      
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
SELECT dwclinks[i],dwclinks[i], 5, (SELECT b1.id FROM public.rpm_externallink b1 WHERE name='dwc'), null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name=dwclinks[i]);


--SQL output at:1/10/2025 9:03:02 AM--> INSERT INTO rpm_meanings (VersionNo, Name, ShortName, Description, Approved, Selectable) VALUES (?, ?, ?, ?, ?, ?) returning Id
-- SQL output at:1/10/2025 9:03:02 AM--> INSERT INTO rpm_meaningentry (MappingRelation) VALUES (?) returning Id
-- SQL output at:1/10/2025 9:03:02 AM--> UPDATE rpm_meanings SET VersionNo = ? WHERE Id = ? AND VersionNo = ?
-- SQL output at:1/10/2025 9:03:02 AM--> INSERT INTO rpm_MeaningEntry_MappedLinks (MeaningEntryRef, ExternalLink_mapped_linkRef) VALUES (?, ?)
-- SQL output at:1/10/2025 9:03:02 AM--> INSERT INTO rpm_Meanings_MeaningEntry (MeaningRef, MeaningEntryRef) VALUES (?, ?)

end loop;

END
$do$;
