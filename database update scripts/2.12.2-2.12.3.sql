BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

ALTER TABLE datacontainers
ALTER COLUMN description TYPE character varying;
   
--COMMIT;