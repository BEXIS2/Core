BEGIN TRANSACTION;

-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.3.0',NOW());


--- operations	for new features
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DDM', 'metadiff', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DDM' AND controller='metadiff');

INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'PUM', 'importcsv', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='PUM' AND controller='importcsv');

INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'PUM', 'importjson', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='PUM' AND controller='importjson');

INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'PUM', 'view', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='PUM' AND controller='view');

INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DDM', 'curation', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DDM' AND controller='curation');

INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'DCM', 'componentconfig', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='DCM' AND controller='componentconfig');

-- add order column in entity template table
ALTER TABLE entitytemplates 
ADD COLUMN "ordernr" INTEGER;

commit;
