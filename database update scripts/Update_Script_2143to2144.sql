BEGIN TRANSACTION;  

ALTER TABLE REQUESTS ALTER COLUMN intention TYPE text;
ALTER TABLE Decisions ALTER COLUMN reason TYPE text;
alter table datasetversions alter column changedescription TYPE text;

COMMIT;