BEGIN TRANSACTION;


-- api taginfo
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'API', 'TagInfo', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='API' AND controller='TagInfo');


-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '3.4.0',NOW());

commit;