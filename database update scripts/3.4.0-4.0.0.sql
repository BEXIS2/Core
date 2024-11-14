-- OPEN ISSUES
    -- DefaultValue within MetadataAttributeUsage: varchar(255) -> text (check MetadataAttributeUsage.hbm.xml)
    -- FixedValue within MetadataAttributeUsage: varchar(255) -> text (check MetadataAttributeUsage.hbm.xml)



-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.0.0',NOW());

commit;
