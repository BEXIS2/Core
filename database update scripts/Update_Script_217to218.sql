BEGIN TRANSACTION;  

-- ROLLBACK TRANSACTION;

-- update Tables
ALTER TABLE public.dim_transformationrules
    ADD COLUMN defaultvalue character varying(255) COLLATE pg_catalog."default";

ALTER TABLE public.mappingconcepts
    ADD COLUMN xsd character varying(255) COLLATE pg_catalog."default";

ALTER TABLE public.mappingkeys
    ADD COLUMN xpath character varying(255) COLLATE pg_catalog."default";


-- insert DATA
-- GBIF

INSERT INTO public.mappingconcepts(
	version, name, description, url, xsd) 
	SELECT 1,'GBIF', 'The concept is needed to create a darwin core archive for GBIF.', 'https://ipt.gbif.org/manual/en/ipt/latest/dwca-guide', 'Modules\DIM\concepts\gbif\eml.xsd'
	WHERE NOT EXISTS (SELECT * FROM public.mappingconcepts WHERE name='GBIF');

-- alternateIdentifier
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'alternateIdentifier', 
'It is a Universally Unique Identifier (UUID) for the EML document and not for the dataset. This term is optional. A list of different identifiers can be supplied. E.g., 619a4b95-1a82-4006-be6a-7dbe3c9b33c5.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#alternateIdentifier',
false,
false,
'eml/dataset/alternateIdentifier',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='alternateIdentifier' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

-- title
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'title', 
'A description of the resource that is being documented that is long enough to differentiate it from other similar resources. Multiple titles may be provided, particularly when trying to express the title in more than one language (use the \"xml: lang\" attribute to indicate the language if not English/en). E.g. Vernal pool amphibian density data, Isla Vista, 1990-1996.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#title',
false,
false,
'eml/dataset/title',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='title' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

--creator
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'creator', 
'The resource creator is the person or organization responsible for creating the resource itself. See section “People and Organizations” for more details.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#creator',
false,
true,
'eml/dataset/creator',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='creator' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

--creator/givenName
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'givenName', 
'Subfield of individualName field. The given name field can be used for the first name of the individual associated with the resource, or for any other names that are not intended to be alphabetized (as appropriate). E.g., Jonny',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#givenName',
true,
false,
'eml/dataset/creator/individualName/givenName',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='creator'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='givenName' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='creator'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--creator/surName
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'surName', 
'Subfield of individualName field. The surname field is used for the last name of the individual associated with the resource. This is typically the family name of an individual, for example, the name by which s/he is referred to in citations. E.g. Carson',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#surName',
false,
false,
'eml/dataset/creator/individualName/surName',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='creator'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='surName' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='creator'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

-- metadataProvider
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'metadataProvider', 
'The metadataProvider is the person or organization responsible for providing documentation for the resource. See section “People and Organizations” for more details.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#metadataProvider',
false,
true,
'eml/dataset/metadataProvider',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='metadataProvider' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

-- metadataProvider/givenName
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'givenName', 
'Subfield of individualName field. The given name field can be used for the first name of the individual associated with the resource, or for any other names that are not intended to be alphabetized (as appropriate). E.g., Jonny',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#givenName',
true,
false,
'eml/dataset/metadataProvider/individualName/givenName',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='metadataProvider'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='givenName' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='metadataProvider'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

-- metadataProvider/surName
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'surName', 
'Subfield of individualName field. The surname field is used for the last name of the individual associated with the resource. This is typically the family name of an individual, for example, the name by which s/he is referred to in citations. E.g. Carson',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#surName',
false,
false,
'eml/dataset/metadataProvider/individualName/surName',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='metadataProvider'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='surName' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='metadataProvider'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--pubDate
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'pubDate', 
'The date that the resource was published. The format should be represented as: CCYY, which represents a 4 digit year, or as CCYY-MM-DD, which denotes the full year, month, and day. Note that month and day are optional components. Formats must conform to ISO 8601. E.g. 2010-09-20.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#pubDate',
false,
false,
'eml/dataset/pubDate',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='pubDate' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

--language
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'language', 
'The language in which the resource (not the metadata document) is written. This can be a well-known language name, or one of the ISO language codes to be more precise. GBIF recommendation is to use the ISO language code (https://api.gbif.org/v1/enumeration/language). E.g., English.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#language',
true,
false,
'eml/dataset/language',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='language' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

--abstract
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'abstract', 
'A brief overview of the resource that is being documented.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#abstract',
false,
false,
'eml/dataset/abstract/para',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='abstract' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

--intellectualRights
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'intellectualRights', 
'A rights management statement for the resource, or reference a service providing such information. Rights information encompasses Intellectual Property Rights (IPR), Copyright, and various Property Rights. In the case of a data set, rights might include requirements for use, requirements for attribution, or other requirements the owner would like to impose. E.g., © 2001 Regents of the University of California Santa Barbara. Free for use by all individuals provided that the owners are acknowledged in any use or publication.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#intellectualRights',
false,
false,
'eml/dataset/intellectualRights/para/ulink/citetitle',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='intellectualRights' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

-- keyword
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'keyword', 
'This field names a keyword or key phrase that concisely describes the resource or is related to the resource. Each keyword field should contain one and only one keyword (i.e., keywords should not be separated by commas or other delimiters).',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#keyword',
true,
false,
'eml/dataset/keywordSet/keyword',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='keyword' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));


--geographicCoverage
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'geographicCoverage', 
'A container for spatial information about a resource; allows a bounding box for the overall coverage (in lat long), and also allows description of arbitrary polygons with exclusions.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#geographicCoverage',
true,
true,
'eml/dataset/coverage/geographicCoverage',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='geographicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

-- geographicCoverage/geographicDescription

INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'geographicDescription', 
'A short text description of a dataset’s geographic areal domain.A text description is especially important to provide a geographic setting when the extent of the dataset cannot be well described by the \"boundingCoordinates\".E.g., \"Manistee River watershed\", \"extent of 7 1/2 minute quads containing any property belonging to Yellowstone National Park\"',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#geographicDescription',
false,
false,
'eml/dataset/coverage/geographicCoverage/geographicDescription',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='geographicCoverage'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='geographicDescription' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='geographicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--geographicCoverage/boundingCoordinates/westBoundingCoordinate
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'westBoundingCoordinate', 
'Subfield of boundingCoordinates field covering the W margin of a bounding box. The longitude in decimal degrees of the western-most point of the bounding box that is being described. E.g., -18.25, +25, 45.24755.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#westBoundingCoordinate',
false,
false,
'eml/dataset/coverage/geographicCoverage/boundingCoordinates/westBoundingCoordinate',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='geographicCoverage'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='westBoundingCoordinate' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='geographicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--geographicCoverage/boundingCoordinates/eastBoundingCoordinate
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'eastBoundingCoordinate', 
'Subfield of boundingCoordinates field covering the E margin of a bounding box. The longitude in decimal degrees of the eastern-most point of the bounding box that is being described. E.g., -18.25, +25, 45.24755.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#eastBoundingCoordinate',
false,
false,
'eml/dataset/coverage/geographicCoverage/boundingCoordinates/eastBoundingCoordinate',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='geographicCoverage'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='eastBoundingCoordinate' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='geographicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--geographicCoverage/boundingCoordinates/northBoundingCoordinate
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'northBoundingCoordinate', 
'Subfield of boundingCoordinates field covering the N margin of a bounding box. The longitude in decimal degrees of the northern-most point of the bounding box that is being described. E.g., -18.25, +25, 65.24755.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#northBoundingCoordinate',
false,
false,
'eml/dataset/coverage/geographicCoverage/boundingCoordinates/northBoundingCoordinate',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='geographicCoverage'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='northBoundingCoordinate' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='geographicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--geographicCoverage/boundingCoordinates/southBoundingCoordinate
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'southBoundingCoordinate', 
'Subfield of boundingCoordinates field covering the S margin of a bounding box. The longitude in decimal degrees of the southern-most point of the bounding box that is being described. E.g., -118.25, +25, 84.24755.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#southBoundingCoordinate',
false,
false,
'eml/dataset/coverage/geographicCoverage/boundingCoordinates/southBoundingCoordinate',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='geographicCoverage'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='southBoundingCoordinate' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='geographicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--taxonomicCoverage

INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'taxonomicCoverage', 
'A container for taxonomic information about a resource. It includes a list of species names (or higher level ranks) from one or more classification systems. Please note the taxonomic classifications should not be nested, just listed one after the other.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#TaxonomicCoverage',
true,
true,
'eml/dataset/coverage/taxonomicCoverage',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='taxonomicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

--taxonomicCoverage/generalTaxonomicCoverage
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'generalTaxonomicCoverage', 
'Taxonomic Coverage is a container for taxonomic information about a resource. It includes a list of species names (or higher level ranks) from one or more classification systems. A description of the range of taxa addressed in the data set or collection. Use a simple comma separated list of taxa. E.g., \"All vascular plants were identified to family or species, mosses and lichens were identified as moss or lichen.\"',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#generalTaxonomicCoverage',
true,
false,
'eml/dataset/coverage/taxonomicCoverage/generalTaxonomicCoverage',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='taxonomicCoverage'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='generalTaxonomicCoverage' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='taxonomicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--taxonomicCoverage/taxonomicClassification/taxonRankName
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'taxonRankName', 
'The name of the taxonomic rank for which the Taxon rank value is provided. E.g., phylum, class, genus, species.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#taxonRankName',
false,
false,
'eml/dataset/coverage/taxonomicCoverage/taxonomicClassification/taxonRankName',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='taxonomicCoverage'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='taxonRankName' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='taxonomicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--taxonomicCoverage/taxonomicClassification/taxonRankValue
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'taxonRankValue', 
'The name representing the taxonomic rank of the taxon being described. E.g. Acer would be an example of a genus rank value, and rubrum would be an example of a species rank value, together indicating the common name of red maple. It is recommended to start with Kingdom and include ranks down to the most detailed level possible.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#taxonRankValue',
false,
false,
'eml/dataset/coverage/taxonomicCoverage/taxonomicClassification/taxonRankValue',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='taxonomicCoverage'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='taxonRankValue' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='taxonomicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--taxonomicCoverage/taxonomicClassification/commonName
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'commonName', 
'Applicable common names; these common names may be general descriptions of a group of organisms if appropriate. E.g., invertebrates, waterfowl.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#commonName',
false,
false,
'eml/dataset/coverage/taxonomicCoverage/taxonomicClassification/commonName',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='taxonomicCoverage'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='commonName' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='taxonomicCoverage' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

-- contact
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'contact', 
'The contact field contains contact information for this dataset. This is the person or institution to contact with questions about the use, interpretation of a data set. See section “People and Organizations” for more details.',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#contact',
false,
true,
'eml/dataset/contact',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
null
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='contact' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'));

--contact/givenName
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'givenName', 
'Subfield of individualName field. The given name field can be used for the first name of the individual associated with the resource, or for any other names that are not intended to be alphabetized (as appropriate). E.g., Jonny',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#givenName',
true,
false,
'eml/dataset/contact/individualName/givenName',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='contact'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='givenName' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='contact' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

--contact/surName
INSERT INTO public.mappingkeys(
	name, description, url, optional, iscomplex, xpath, concept, parentref)
SELECT 'surName', 
'Subfield of individualName field. The surname field is used for the last name of the individual associated with the resource. This is typically the family name of an individual, for example, the name by which s/he is referred to in citations. E.g. Carson',
'https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#surName',
true,
false,
'eml/dataset/contact/individualName/surName',
(SELECT id FROM public.mappingconcepts WHERE name='GBIF'),
(SELECT id FROM public.mappingkeys WHERE name='contact'and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF'))
WHERE NOT EXISTS (SELECT * FROM public.mappingkeys WHERE name='surName' and parentref =  (SELECT id FROM public.mappingkeys WHERE name='contact' and concept = (SELECT id FROM public.mappingconcepts WHERE name='GBIF')));

-- END GBIF


-- END insert DATA


-- Insert version
INSERT INTO public.versions(
	versionno, extra, module, value, date)
	VALUES (1, null, 'Shell', '2.18',NOW());
  
  COMMIT;
