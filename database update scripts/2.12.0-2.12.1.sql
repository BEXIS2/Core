BEGIN TRANSACTION;  
-- ROLLBACK TRANSACTION;

-- update system type of a party type
UPDATE public.partytypes SET systemtype = 'N';
--Select * from public.partytypes; 

-- update new added terms and condition & privacy policy default values
UPDATE public.users SET hastermsandconditionsaccepted = 'N';
UPDATE public.users SET hasprivacypolicyaccepted = 'N';
--Select * from public.users;

-- ADD GRANT to partycustomgridcolumns - postgres user 
GRANT SELECT, UPDATE, INSERT ON public.partycustomgridcolumns TO postgres;


-- ADD SEEDDATA from shell
INSERT INTO public.operations(
            versionno, extra, module, controller, action, featureref)
    VALUES 
    (1, ' ', 'Shell', 'TermsAndConditions', '*', null),
    (1, ' ', 'Shell', 'PrivacyPolicy', '*', null);

--Select * from public.operations;

-- add token to users table
ALTER TABLE public.users ADD token character varying;

--COMMIT;