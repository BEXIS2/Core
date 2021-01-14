BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

ALTER TABLE public.entityreferences
    ADD COLUMN creationdate timestamp without time zone;



-- INSERT Data --
INSERT INTO public.operations
(versionno, extra, "module", controller, "action", featureref)
VALUES(1, NULL, 'API', 'DataStatisticOut', '*', (
Select id from features where name = 'API' AND parentref = (
Select id from features where name = 'Data Dissemination')));

INSERT INTO public.operations
(versionno, extra, "module", controller, "action", featureref)
VALUES(1, NULL, 'API', 'DataQualityOut', '*', (
Select id from features where name = 'API' AND parentref = (
Select id from features where name = 'Data Dissemination')));

INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '2.14.3',NOW());

COMMIT;