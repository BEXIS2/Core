CREATE INDEX idx_DataAttributeRef_Variables
    ON public.variables USING btree
    (dataattributeref)
    TABLESPACE pg_default;
    
CREATE INDEX idx_DataStructureRef_Variables
    ON public.variables USING btree
    (datastructureref)
    TABLESPACE pg_default;
