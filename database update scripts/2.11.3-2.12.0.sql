BEGIN TRANSACTION;  
-- ROLLBACK TRANSACTION;

ALTER TABLE public.partyrelationships RENAME COLUMN firstpartyref TO sourcepartyref;
ALTER TABLE public.partyrelationships RENAME COLUMN secondpartyref TO targetpartyref;
ALTER TABLE public.partyrelationships ADD permission int4 NULL;

ALTER TABLE public.partytypepairs ADD permissiontemplate int4 NULL;

ALTER TABLE public.partytypes ADD systemtype char(1) NULL;

ALTER TABLE public.users ADD registrationdate timestamp NULL;
ALTER TABLE public.users ADD hastermsandconditionsaccepted char(1) NULL;
ALTER TABLE public.users ADD hasprivacypolicyaccepted char(1) NULL;

CREATE TABLE public.partycustomgridcolumns (
	id bigserial NOT NULL,
	versionno int4 NOT NULL,
	extra xml NULL,
	userid int8 NULL,
	"enable" bpchar(1) NULL,
	customattributeref int8 NULL,
	typepairref int8 NULL,
	CONSTRAINT partycustomgridcolumns_pkey PRIMARY KEY (id),
	CONSTRAINT fkb9edbdce69a94133 FOREIGN KEY (customattributeref) REFERENCES partycustomattributes(id),
	CONSTRAINT fkb9edbdcec3d67554 FOREIGN KEY (typepairref) REFERENCES partytypepairs(id)
);

COMMIT;