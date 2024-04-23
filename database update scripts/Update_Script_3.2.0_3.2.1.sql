BEGIN TRANSACTION;

-- add operations
--- DCM Test --
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'Test', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='Test');

-- api datatable
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'DataTable', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='DataTable');

-- update displaypattern
UPDATE public.variables
	SET displaypatternid=-1
	WHERE displaypatternid is NULL and datatyperef not in (3,4,10);

-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.2.0',NOW());

commit;