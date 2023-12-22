BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;


-- Add Controllers to operation
-- shell/uitest
-- shell/menu

-- dcm/Metadata - DatasetCreationFeature
-- dcm/Edit - DatasetCreationFeature
-- dcm/StructureSuggestion - DatasetCreationFeature


-- dcm/Validation - DatasetUploadFeature
-- dcm/FileUpload - DatasetUploadFeature
-- dcm/AttachmentUpload - DatasetUploadFeature
-- dcm/messages - DatasetUploadFeature



-- ddm/view
-- dim/publish 
-- sam/userpermissions

-- EntityTemplates / create Table, Link to Dataset, Is required

-- Variables
    -- add values from DataAttribute to Variable Instance (datatype, unit, constraints)
    -- Convert DataAttrributes to Variable Templates
    -- Convert Variables to Variable Instances
    -- link Variable Template to Variable Instance

CREATE TABLE public.variable_constraints
(
    constraintref bigint NOT NULL,
    variableref bigint NOT NULL,
    CONSTRAINT variable_constraints_pkey PRIMARY KEY (variableref, constraintref),
    CONSTRAINT fk_17300cc1 FOREIGN KEY (variableref)
        REFERENCES public.variables (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT fk_8ba3b117 FOREIGN KEY (constraintref)
        REFERENCES public.constraints (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE public.variable_constraints
    OWNER to postgres;
	
	
ALter table variables add column vartemplateref bigint;
ALter table variables add column datepattern character varying(255);


ALter table variables ADD CONSTRAINT fk_2abad2e6 FOREIGN KEY (vartemplateref)
        REFERENCES public.variables (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION;
		
ALter table variables ADD CONSTRAINT fk_990c0cd5 FOREIGN KEY (datatyperef)
        REFERENCES public.datatypes (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION;		



COMMIT;