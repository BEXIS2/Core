BEGIN TRANSACTION;

-- update names of existing datatypes
-- Name;Description;System Type;Display Pattern
-- Text;A unicode string;String;
-- Floating Point Number;A precise real number with a restricted range;Double;
-- Integer;An 64 bit integer number;Int64;
-- Boolean;A boolean value;Boolean;
-- Date and Time;A date with / or time;DateTime;
-- Date;A date;DateTime;DateIso
-- Time;A time;DateTime;Time


-- 
/*
Update dim_linkelements
Set
	xpath = xpath ||'/' || SUBSTRING(xpath FROM LENGTH(xpath) - STRPOS(REVERSE(xpath), '/') + 2)|| 'Type'
 where type  in (5,6) and complexity = 1 and xpath !='';
	*/


-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.1.0',NOW());

commit;