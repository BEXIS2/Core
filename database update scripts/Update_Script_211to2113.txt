BEGIN TRANSACTION;  
-- ROLLBACK TRANSACTION;

ALTER TABLE public.partyrelationships ADD COLUMN IF NOT EXISTS partyrelationshiptypepairref int8 NULL;
ALTER TABLE public.partyrelationships ADD CONSTRAINT IF NOT EXISTS partyrelationships_partytypepairs_fk FOREIGN KEY (id) REFERENCES public.partytypepairs(id);

COMMIT;