BEGIN TRANSACTION;

-- add operations
--- DCM Test --
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'Test', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='Test');

-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.2.0',NOW());

commit;