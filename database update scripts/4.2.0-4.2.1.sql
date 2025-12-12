BEGIN TRANSACTION;

-- fix spelling mistake in dim_broker
Update dim_brokers
set type = 'Observations'
where name = 'GFBIO' and type like 'Ob%'

-- BEXIS2 Version Update
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '4.2.1',NOW());

commit;
