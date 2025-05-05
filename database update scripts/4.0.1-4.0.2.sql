BEGIN TRANSACTION;

-- xpath changes for all older linkelements
update dim_linkelements
set xpath =
    CASE
        WHEN xpath LIKE '%/%' and xpath like '%Type'
        THEN reverse(substring(reverse(xpath) FROM position('/' in reverse(xpath)) + 1))
        ELSE xpath
    END
where complexity = 1 and type in (5,6);

--- RPM  TeFilest --
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'File', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='File');

-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.0.2',NOW());

commit;