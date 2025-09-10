BEGIN TRANSACTION;

-- entities - extension
INSERT INTO public.entities (
	versionno, 
	extra, 
	entitystoretype, 
	entitytype, 
	name, 
	securable, 
	usemetadata,
	parentref
	)
SELECT 
	1,
	'<extra><modules><module name="name" value="ddm" type="parameter" /></modules></extra>',
	'BExIS.Xml.Helpers.ExtensionStore, BExIS.Xml.Helpers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null',
	'BExIS.Dlm.Entities.Data.Dataset, BExIS.Dlm.Entities, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null',
	'Extension',
	true,
	true,
	null
	
WHERE NOT EXISTS (SELECT * FROM public.entities WHERE name='Extension');



-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.2.0',NOW());

commit;
