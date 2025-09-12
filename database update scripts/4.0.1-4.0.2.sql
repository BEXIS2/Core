BEGIN TRANSACTION;

-- -- xpath changes for all older linkelements
-- update dim_linkelements
-- set xpath =
--     CASE
--         WHEN xpath LIKE '%/%' and xpath like '%Type'
--         THEN reverse(substring(reverse(xpath) FROM position('/' in reverse(xpath)) + 1))
--         ELSE xpath
--     END
-- where complexity = 1 and type in (5,6);

--- RPM  TeFilest --
INSERT INTO public.operations (versionno, extra, module, controller, action, featureref)
SELECT 1, NULL, 'RPM', 'File', '*', null 
WHERE NOT EXISTS (SELECT * FROM public.operations WHERE module='RPM' AND controller='File');

-- set all meanings to variable_templates
INSERT INTO rpm_meanings_variables (variableref, meaningref)
SELECT v.id, m.id
FROM variables v, RPM_MEANINGS M
WHERE v.label = m.name and 
v.variablestype = 'VAR_TEMPL' and
NOT EXISTS (
    SELECT 1
    FROM rpm_meanings_variables mv
    WHERE mv.variableref = v.id
      AND mv.meaningref = m.id
  );

-- set all meanings from template to instance variables
INSERT INTO rpm_meanings_variables (variableref, meaningref)
SELECT v.id, mv.meaningref
FROM variables v, rpm_meanings_variables mv
WHERE v.vartemplateref = mv.variableref and 
v.variablestype = 'VAR_INST' and
NOT EXISTS (
    SELECT 1
    FROM rpm_meanings_variables mv
    WHERE mv.variableref = v.id
      AND mv.meaningref = mv.meaningref
  );

-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.0.2',NOW());

commit;