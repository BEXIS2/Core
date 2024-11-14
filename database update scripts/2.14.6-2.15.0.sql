BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

-- Add Entry for api/MetadataStructureOut
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'MetadataStructureOut', '*', null
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='MetadataStructureOut');


-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '2.15.0',NOW());

COMMIT;