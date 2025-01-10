-- OPEN ISSUES
 -- add dim/gbif controller in operations

-- DefaultValue within MetadataAttributeUsage: varchar(255) -> text (check MetadataAttributeUsage.hbm.xml)
ALTER TABLE public.metadataattributeusages
ALTER COLUMN DefaultValue TYPE TEXT;

-- FixedValue within MetadataAttributeUsage: varchar(255) -> text (check MetadataAttributeUsage.hbm.xml)
ALTER TABLE public.metadataattributeusages
ALTER COLUMN FixedValue TYPE TEXT;

-- DefaultValue within MetadataNestedAttributeUsages: varchar(255) -> text (check MetadataAttributeUsage.hbm.xml)
ALTER TABLE public.MetadataNestedAttributeUsages
ALTER COLUMN DefaultValue TYPE TEXT;

-- FixedValue within MetadataNestedAttributeUsages: varchar(255) -> text (check MetadataAttributeUsage.hbm.xml)
ALTER TABLE public.MetadataNestedAttributeUsages
ALTER COLUMN FixedValue TYPE TEXT;



-- DOI Mapping(s) Update
INSERT INTO public.dim_mappingkeys (name, url, optional, iscomplex, concept, xpath)
    SELECT 'Types', 'https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/resourcetype/#a-resourcetypegeneral', false, true, (select id from dim_mappingconcepts where name='datacite'), 'data/attributes/types'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='datacite') AND xpath='data/attributes/types');

INSERT INTO public.dim_mappingkeys (name, optional, iscomplex, concept, parentref, xpath)
    SELECT 'ResourceTypeGeneral', false, false, (select id from dim_mappingconcepts where name='datacite'), (select id from dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='datacite') AND xpath='data/attributes/types'), 'data/attributes/types/resourceTypeGeneral'
    WHERE NOT EXISTS (select * from public.dim_mappingkeys where concept=(select id from dim_mappingconcepts where name='datacite') AND xpath='data/attributes/types/resourceTypeGeneral');

-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.0.0',NOW());

commit;
