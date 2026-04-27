import { Api } from '@bexis2/bexis2-core-ui';
import { MeaningModel } from '$lib/components/SearchConfig/SearchConfigModel';
// import type { EntityTemplateModel } from '../models/EntityTemplate';

export const getEntityTemplate = async (id: number) => {
    try {
        const response = await Api.get('/dcm/entitytemplates/Get?id=' + id);
        return response.data;
    } catch (error) {
        console.error(error);
        throw error;
    }
};

export const getEntityTemplateList = async () => {
    try {
        const response = await Api.get('/dcm/entitytemplates/Load');
        return response.data;
    } catch (error) {
        console.error(error);
        throw error;
    }
};

export const getDataTypes = async () => {
	try {
		const response = await Api.get('/rpm/dataType/GetDataTypes');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getMeanings = async () => {
	try {
		const response = await Api.get('/rpm/Meaning/get');
		console.log('🚀 ~ file: services.ts:8 ~ getMeanings ~ response.data:', response.data);

		const list: MeaningModel[] = [];

		for (let index = 0; index < response.data.length; index++) {
			list.push(new MeaningModel(response.data[index]));
		}

		return list;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getMetadataNodes = async () => {	
	try {
		const response = await Api.get('/ddm/searchconfig/GetMetadataNodes');
		return response.data;
	} catch (error) {
		console.error(error);
		return tempNodes;
	}
};

export const SaveConfig = async (value: any) => {
	try {
        console.log(" value:", value);
        const value_string = JSON.stringify(value);
		const data = {"content":value_string};
		const response = await Api.post('/ddm/SearchConfig/SaveConfig', data);
		// console.log('Dataset filled:', response);
        console.log('config saved:', response);
		return response.data;
    

	} catch (error) {
		console.error('error during saving config:', error);
		throw error;
	}
};

export const LoadConfig = async () => {
	try {
	
		const response = await Api.get('/ddm/SearchConfig/LoadConfig');
		// console.log('Dataset filled:', response);
		return response.data;
	} catch (error) {
		console.error('error during saving config:', error);
		throw error;
	}
};


let tempNodes = [
  {
    "displayName": "abstract/para",
    "displayNameLong": "(GBIF) abstract/para",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/abstract/abstractXmlSchemaComplexType/para/paraDatatype_string"
  },
  {
    "displayName": "additionalInfo/para",
    "displayNameLong": "(GBIF) additionalInfo/para",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/additionalInfo/additionalInfoXmlSchemaComplexType/para/paraDatatype_string"
  },
  {
    "displayName": "associatedParty/address/administrativeArea",
    "displayNameLong": "(GBIF) associatedParty/address/administrativeArea",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/address/addressXmlSchemaComplexType/administrativeArea/NonEmptyStringType"
  },
  {
    "displayName": "associatedParty/address/city",
    "displayNameLong": "(GBIF) associatedParty/address/city",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/address/addressXmlSchemaComplexType/city/NonEmptyStringType"
  },
  {
    "displayName": "associatedParty/address/country",
    "displayNameLong": "(GBIF) associatedParty/address/country",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/address/addressXmlSchemaComplexType/country/NonEmptyStringType"
  },
  {
    "displayName": "associatedParty/address/deliveryPoint",
    "displayNameLong": "(GBIF) associatedParty/address/deliveryPoint",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/address/addressXmlSchemaComplexType/deliveryPoint/NonEmptyStringType"
  },
  {
    "displayName": "associatedParty/address/postalCode",
    "displayNameLong": "(GBIF) associatedParty/address/postalCode",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/address/addressXmlSchemaComplexType/postalCode/NonEmptyStringType"
  },
  {
    "displayName": "associatedParty/electronicMailAddress",
    "displayNameLong": "(GBIF) associatedParty/electronicMailAddress",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/electronicMailAddress/NonEmptyStringType"
  },
  {
    "displayName": "associatedParty/individualName/givenName",
    "displayNameLong": "(GBIF) associatedParty/individualName/givenName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/individualName/individualNameXmlSchemaComplexType/givenName/NonEmptyStringType"
  },
  {
    "displayName": "associatedParty/individualName/surName",
    "displayNameLong": "(GBIF) associatedParty/individualName/surName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/individualName/individualNameXmlSchemaComplexType/surName/NonEmptyStringType"
  },
  {
    "displayName": "associatedParty/onlineUrl",
    "displayNameLong": "(GBIF) associatedParty/onlineUrl",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/onlineUrl/onlineUrlDatatype_anyURI"
  },
  {
    "displayName": "associatedParty/organizationName",
    "displayNameLong": "(GBIF) associatedParty/organizationName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/organizationName/NonEmptyStringType"
  },
  {
    "displayName": "associatedParty/phone",
    "displayNameLong": "(GBIF) associatedParty/phone",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/phone/phoneDatatype_string"
  },
  {
    "displayName": "associatedParty/positionName",
    "displayNameLong": "(GBIF) associatedParty/positionName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/positionName/NonEmptyStringType"
  },
  {
    "displayName": "associatedParty/role",
    "displayNameLong": "(GBIF) associatedParty/role",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/associatedParty/agentWithRoleType/role/roleDatatype_string"
  },
  {
    "displayName": "Basic/alternateIdentifier",
    "displayNameLong": "(GBIF) Basic/alternateIdentifier",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/Basic/BasicType/alternateIdentifier/alternateIdentifierDatatype_string"
  },
  {
    "displayName": "Basic/DatasetGUID",
    "displayNameLong": "(Basic ABCD) Basic/DatasetGUID",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Basic/BasicType/DatasetGUID/String"
  },
  {
    "displayName": "Basic/language",
    "displayNameLong": "(GBIF) Basic/language",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/Basic/BasicType/language/NonEmptyStringType"
  },
  {
    "displayName": "Basic/pubDate",
    "displayNameLong": "(GBIF) Basic/pubDate",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/Basic/BasicType/pubDate/yearDate"
  },
  {
    "displayName": "Basic/title",
    "displayNameLong": "(GBIF) Basic/title",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/Basic/BasicType/title/i18nString"
  },
  {
    "displayName": "contact/address/administrativeArea",
    "displayNameLong": "(GBIF) contact/address/administrativeArea",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/address/addressXmlSchemaComplexType/administrativeArea/NonEmptyStringType"
  },
  {
    "displayName": "contact/address/city",
    "displayNameLong": "(GBIF) contact/address/city",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/address/addressXmlSchemaComplexType/city/NonEmptyStringType"
  },
  {
    "displayName": "contact/address/country",
    "displayNameLong": "(GBIF) contact/address/country",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/address/addressXmlSchemaComplexType/country/NonEmptyStringType"
  },
  {
    "displayName": "contact/address/deliveryPoint",
    "displayNameLong": "(GBIF) contact/address/deliveryPoint",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/address/addressXmlSchemaComplexType/deliveryPoint/NonEmptyStringType"
  },
  {
    "displayName": "contact/address/postalCode",
    "displayNameLong": "(GBIF) contact/address/postalCode",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/address/addressXmlSchemaComplexType/postalCode/NonEmptyStringType"
  },
  {
    "displayName": "contact/electronicMailAddress",
    "displayNameLong": "(GBIF) contact/electronicMailAddress",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/electronicMailAddress/NonEmptyStringType"
  },
  {
    "displayName": "contact/individualName/givenName",
    "displayNameLong": "(GBIF) contact/individualName/givenName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/individualName/individualNameXmlSchemaComplexType/givenName/NonEmptyStringType"
  },
  {
    "displayName": "contact/individualName/surName",
    "displayNameLong": "(GBIF) contact/individualName/surName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/individualName/individualNameXmlSchemaComplexType/surName/NonEmptyStringType"
  },
  {
    "displayName": "contact/onlineUrl",
    "displayNameLong": "(GBIF) contact/onlineUrl",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/onlineUrl/onlineUrlDatatype_anyURI"
  },
  {
    "displayName": "contact/organizationName",
    "displayNameLong": "(GBIF) contact/organizationName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/organizationName/NonEmptyStringType"
  },
  {
    "displayName": "contact/phone",
    "displayNameLong": "(GBIF) contact/phone",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/phone/phoneDatatype_string"
  },
  {
    "displayName": "contact/positionName",
    "displayNameLong": "(GBIF) contact/positionName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/contact/agentType/positionName/NonEmptyStringType"
  },
  {
    "displayName": "ContentContacts/ContentContact/Address",
    "displayNameLong": "(Basic ABCD) ContentContacts/ContentContact/Address",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/ContentContacts/ContentContactsXmlSchemaComplexType/ContentContact/MicroAgentP/Address/String255"
  },
  {
    "displayName": "ContentContacts/ContentContact/Email",
    "displayNameLong": "(Basic ABCD) ContentContacts/ContentContact/Email",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/ContentContacts/ContentContactsXmlSchemaComplexType/ContentContact/MicroAgentP/Email/String255"
  },
  {
    "displayName": "ContentContacts/ContentContact/Name",
    "displayNameLong": "(Basic ABCD) ContentContacts/ContentContact/Name",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/ContentContacts/ContentContactsXmlSchemaComplexType/ContentContact/MicroAgentP/Name/String255"
  },
  {
    "displayName": "ContentContacts/ContentContact/Phone",
    "displayNameLong": "(Basic ABCD) ContentContacts/ContentContact/Phone",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/ContentContacts/ContentContactsXmlSchemaComplexType/ContentContact/MicroAgentP/Phone/String255"
  },
  {
    "displayName": "coverage/geographicCoverage/boundingCoordinates/eastBoundingCoordinate",
    "displayNameLong": "(GBIF) coverage/geographicCoverage/boundingCoordinates/eastBoundingCoordinate",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/coverage/coverageXmlSchemaComplexType/geographicCoverage/geographicCoverageXmlSchemaComplexType/boundingCoordinates/boundingCoordinatesXmlSchemaComplexType/eastBoundingCoordinate/eastBoundingCoordinateDatatype_decimal"
  },
  {
    "displayName": "coverage/geographicCoverage/boundingCoordinates/northBoundingCoordinate",
    "displayNameLong": "(GBIF) coverage/geographicCoverage/boundingCoordinates/northBoundingCoordinate",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/coverage/coverageXmlSchemaComplexType/geographicCoverage/geographicCoverageXmlSchemaComplexType/boundingCoordinates/boundingCoordinatesXmlSchemaComplexType/northBoundingCoordinate/northBoundingCoordinateDatatype_decimal"
  },
  {
    "displayName": "coverage/geographicCoverage/boundingCoordinates/southBoundingCoordinate",
    "displayNameLong": "(GBIF) coverage/geographicCoverage/boundingCoordinates/southBoundingCoordinate",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/coverage/coverageXmlSchemaComplexType/geographicCoverage/geographicCoverageXmlSchemaComplexType/boundingCoordinates/boundingCoordinatesXmlSchemaComplexType/southBoundingCoordinate/southBoundingCoordinateDatatype_decimal"
  },
  {
    "displayName": "coverage/geographicCoverage/boundingCoordinates/westBoundingCoordinate",
    "displayNameLong": "(GBIF) coverage/geographicCoverage/boundingCoordinates/westBoundingCoordinate",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/coverage/coverageXmlSchemaComplexType/geographicCoverage/geographicCoverageXmlSchemaComplexType/boundingCoordinates/boundingCoordinatesXmlSchemaComplexType/westBoundingCoordinate/westBoundingCoordinateDatatype_decimal"
  },
  {
    "displayName": "coverage/geographicCoverage/geographicDescription",
    "displayNameLong": "(GBIF) coverage/geographicCoverage/geographicDescription",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/coverage/coverageXmlSchemaComplexType/geographicCoverage/geographicCoverageXmlSchemaComplexType/geographicDescription/NonEmptyStringType"
  },
  {
    "displayName": "coverage/taxonomicCoverage/generalTaxonomicCoverage",
    "displayNameLong": "(GBIF) coverage/taxonomicCoverage/generalTaxonomicCoverage",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/coverage/coverageXmlSchemaComplexType/taxonomicCoverage/taxonomicCoverageXmlSchemaComplexType/generalTaxonomicCoverage/NonEmptyStringType"
  },
  {
    "displayName": "coverage/taxonomicCoverage/taxonomicClassification/commonName",
    "displayNameLong": "(GBIF) coverage/taxonomicCoverage/taxonomicClassification/commonName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/coverage/coverageXmlSchemaComplexType/taxonomicCoverage/taxonomicCoverageXmlSchemaComplexType/taxonomicClassification/taxonomicClassificationXmlSchemaComplexType/commonName/NonEmptyStringType"
  },
  {
    "displayName": "coverage/taxonomicCoverage/taxonomicClassification/taxonRankName",
    "displayNameLong": "(GBIF) coverage/taxonomicCoverage/taxonomicClassification/taxonRankName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/coverage/coverageXmlSchemaComplexType/taxonomicCoverage/taxonomicCoverageXmlSchemaComplexType/taxonomicClassification/taxonomicClassificationXmlSchemaComplexType/taxonRankName/NonEmptyStringType"
  },
  {
    "displayName": "coverage/taxonomicCoverage/taxonomicClassification/taxonRankValue",
    "displayNameLong": "(GBIF) coverage/taxonomicCoverage/taxonomicClassification/taxonRankValue",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/coverage/coverageXmlSchemaComplexType/taxonomicCoverage/taxonomicCoverageXmlSchemaComplexType/taxonomicClassification/taxonomicClassificationXmlSchemaComplexType/taxonRankValue/NonEmptyStringType"
  },
  {
    "displayName": "coverage/temporalCoverage",
    "displayNameLong": "(GBIF) coverage/temporalCoverage",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/coverage/coverageXmlSchemaComplexType/temporalCoverage/temporalCoverageXmlSchemaComplexType"
  },
  {
    "displayName": "creator/address/administrativeArea",
    "displayNameLong": "(GBIF) creator/address/administrativeArea",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/address/addressXmlSchemaComplexType/administrativeArea/NonEmptyStringType"
  },
  {
    "displayName": "creator/address/city",
    "displayNameLong": "(GBIF) creator/address/city",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/address/addressXmlSchemaComplexType/city/NonEmptyStringType"
  },
  {
    "displayName": "creator/address/country",
    "displayNameLong": "(GBIF) creator/address/country",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/address/addressXmlSchemaComplexType/country/NonEmptyStringType"
  },
  {
    "displayName": "creator/address/deliveryPoint",
    "displayNameLong": "(GBIF) creator/address/deliveryPoint",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/address/addressXmlSchemaComplexType/deliveryPoint/NonEmptyStringType"
  },
  {
    "displayName": "creator/address/postalCode",
    "displayNameLong": "(GBIF) creator/address/postalCode",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/address/addressXmlSchemaComplexType/postalCode/NonEmptyStringType"
  },
  {
    "displayName": "creator/electronicMailAddress",
    "displayNameLong": "(GBIF) creator/electronicMailAddress",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/electronicMailAddress/NonEmptyStringType"
  },
  {
    "displayName": "creator/individualName/givenName",
    "displayNameLong": "(GBIF) creator/individualName/givenName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/individualName/individualNameXmlSchemaComplexType/givenName/NonEmptyStringType"
  },
  {
    "displayName": "creator/individualName/surName",
    "displayNameLong": "(GBIF) creator/individualName/surName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/individualName/individualNameXmlSchemaComplexType/surName/NonEmptyStringType"
  },
  {
    "displayName": "creator/onlineUrl",
    "displayNameLong": "(GBIF) creator/onlineUrl",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/onlineUrl/onlineUrlDatatype_anyURI"
  },
  {
    "displayName": "creator/organizationName",
    "displayNameLong": "(GBIF) creator/organizationName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/organizationName/NonEmptyStringType"
  },
  {
    "displayName": "creator/phone",
    "displayNameLong": "(GBIF) creator/phone",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/phone/phoneDatatype_string"
  },
  {
    "displayName": "creator/positionName",
    "displayNameLong": "(GBIF) creator/positionName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/creator/agentType/positionName/NonEmptyStringType"
  },
  {
    "displayName": "distribution/online/url",
    "displayNameLong": "(GBIF) distribution/online/url",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/distribution/distributionXmlSchemaComplexType/online/onlineXmlSchemaComplexType/url/urlXmlSchemaComplexType"
  },
  {
    "displayName": "intellectualRights/para",
    "displayNameLong": "(GBIF) intellectualRights/para",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/intellectualRights/intellectualRightsXmlSchemaComplexType/para/paraDatatype_string"
  },
  {
    "displayName": "keywordSet/keyword",
    "displayNameLong": "(GBIF) keywordSet/keyword",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/keywordSet/keywordSetXmlSchemaComplexType/keyword/NonEmptyStringType"
  },
  {
    "displayName": "keywordSet/keywordThesaurus",
    "displayNameLong": "(GBIF) keywordSet/keywordThesaurus",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/keywordSet/keywordSetXmlSchemaComplexType/keywordThesaurus/NonEmptyStringType"
  },
  {
    "displayName": "Metadata/Description/Representation/Coverage",
    "displayNameLong": "(Basic ABCD) Metadata/Description/Representation/Coverage",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Description/DescriptionXmlSchemaComplexType/Representation/MetadataDescriptionRepr/Coverage/String"
  },
  {
    "displayName": "Metadata/Description/Representation/Details",
    "displayNameLong": "(Basic ABCD) Metadata/Description/Representation/Details",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Description/DescriptionXmlSchemaComplexType/Representation/MetadataDescriptionRepr/Details/String"
  },
  {
    "displayName": "Metadata/Description/Representation/Title",
    "displayNameLong": "(Basic ABCD) Metadata/Description/Representation/Title",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Description/DescriptionXmlSchemaComplexType/Representation/MetadataDescriptionRepr/Title/String255"
  },
  {
    "displayName": "Metadata/Description/Representation/URI",
    "displayNameLong": "(Basic ABCD) Metadata/Description/Representation/URI",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Description/DescriptionXmlSchemaComplexType/Representation/MetadataDescriptionRepr/URI/URIDatatype_anyURI"
  },
  {
    "displayName": "Metadata/IconURI",
    "displayNameLong": "(Basic ABCD) Metadata/IconURI",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IconURI/IconURIDatatype_anyURI"
  },
  {
    "displayName": "Metadata/IPRStatements/Acknowledgements/Acknowledgement/Details",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Acknowledgements/Acknowledgement/Details",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Acknowledgements/AcknowledgementsXmlSchemaComplexType/Acknowledgement/Statement/Details/String"
  },
  {
    "displayName": "Metadata/IPRStatements/Acknowledgements/Acknowledgement/Text",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Acknowledgements/Acknowledgement/Text",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Acknowledgements/AcknowledgementsXmlSchemaComplexType/Acknowledgement/Statement/Text/String"
  },
  {
    "displayName": "Metadata/IPRStatements/Acknowledgements/Acknowledgement/URI",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Acknowledgements/Acknowledgement/URI",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Acknowledgements/AcknowledgementsXmlSchemaComplexType/Acknowledgement/Statement/URI/URIDatatype_anyURI"
  },
  {
    "displayName": "Metadata/IPRStatements/Citations/Citation/Details",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Citations/Citation/Details",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Citations/CitationsXmlSchemaComplexType/Citation/Statement/Details/String"
  },
  {
    "displayName": "Metadata/IPRStatements/Citations/Citation/Text",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Citations/Citation/Text",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Citations/CitationsXmlSchemaComplexType/Citation/Statement/Text/String"
  },
  {
    "displayName": "Metadata/IPRStatements/Citations/Citation/URI",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Citations/Citation/URI",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Citations/CitationsXmlSchemaComplexType/Citation/Statement/URI/URIDatatype_anyURI"
  },
  {
    "displayName": "Metadata/IPRStatements/Copyrights/Copyright/Details",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Copyrights/Copyright/Details",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Copyrights/CopyrightsXmlSchemaComplexType/Copyright/Statement/Details/String"
  },
  {
    "displayName": "Metadata/IPRStatements/Copyrights/Copyright/Text",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Copyrights/Copyright/Text",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Copyrights/CopyrightsXmlSchemaComplexType/Copyright/Statement/Text/String"
  },
  {
    "displayName": "Metadata/IPRStatements/Copyrights/Copyright/URI",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Copyrights/Copyright/URI",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Copyrights/CopyrightsXmlSchemaComplexType/Copyright/Statement/URI/URIDatatype_anyURI"
  },
  {
    "displayName": "Metadata/IPRStatements/Disclaimers/Disclaimer/Details",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Disclaimers/Disclaimer/Details",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Disclaimers/DisclaimersXmlSchemaComplexType/Disclaimer/Statement/Details/String"
  },
  {
    "displayName": "Metadata/IPRStatements/Disclaimers/Disclaimer/Text",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Disclaimers/Disclaimer/Text",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Disclaimers/DisclaimersXmlSchemaComplexType/Disclaimer/Statement/Text/String"
  },
  {
    "displayName": "Metadata/IPRStatements/Disclaimers/Disclaimer/URI",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Disclaimers/Disclaimer/URI",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Disclaimers/DisclaimersXmlSchemaComplexType/Disclaimer/Statement/URI/URIDatatype_anyURI"
  },
  {
    "displayName": "Metadata/IPRStatements/IPRDeclarations/IPRDeclaration/Details",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/IPRDeclarations/IPRDeclaration/Details",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/IPRDeclarations/IPRDeclarationsXmlSchemaComplexType/IPRDeclaration/Statement/Details/String"
  },
  {
    "displayName": "Metadata/IPRStatements/IPRDeclarations/IPRDeclaration/Text",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/IPRDeclarations/IPRDeclaration/Text",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/IPRDeclarations/IPRDeclarationsXmlSchemaComplexType/IPRDeclaration/Statement/Text/String"
  },
  {
    "displayName": "Metadata/IPRStatements/IPRDeclarations/IPRDeclaration/URI",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/IPRDeclarations/IPRDeclaration/URI",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/IPRDeclarations/IPRDeclarationsXmlSchemaComplexType/IPRDeclaration/Statement/URI/URIDatatype_anyURI"
  },
  {
    "displayName": "Metadata/IPRStatements/Licenses/License/Details",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Licenses/License/Details",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Licenses/LicensesXmlSchemaComplexType/License/Statement/Details/String"
  },
  {
    "displayName": "Metadata/IPRStatements/Licenses/License/Text",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Licenses/License/Text",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Licenses/LicensesXmlSchemaComplexType/License/Statement/Text/String"
  },
  {
    "displayName": "Metadata/IPRStatements/Licenses/License/URI",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/Licenses/License/URI",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/Licenses/LicensesXmlSchemaComplexType/License/Statement/URI/URIDatatype_anyURI"
  },
  {
    "displayName": "Metadata/IPRStatements/TermsOfUseStatements/TermsOfUse/Details",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/TermsOfUseStatements/TermsOfUse/Details",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/TermsOfUseStatements/TermsOfUseStatementsXmlSchemaComplexType/TermsOfUse/Statement/Details/String"
  },
  {
    "displayName": "Metadata/IPRStatements/TermsOfUseStatements/TermsOfUse/Text",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/TermsOfUseStatements/TermsOfUse/Text",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/TermsOfUseStatements/TermsOfUseStatementsXmlSchemaComplexType/TermsOfUse/Statement/Text/String"
  },
  {
    "displayName": "Metadata/IPRStatements/TermsOfUseStatements/TermsOfUse/URI",
    "displayNameLong": "(Basic ABCD) Metadata/IPRStatements/TermsOfUseStatements/TermsOfUse/URI",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/IPRStatements/IPRStatements/TermsOfUseStatements/TermsOfUseStatementsXmlSchemaComplexType/TermsOfUse/Statement/URI/URIDatatype_anyURI"
  },
  {
    "displayName": "Metadata/Owners/Owner/Addresses/Address",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Addresses/Address",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Addresses/AddressesXmlSchemaComplexType/Address/StringLP"
  },
  {
    "displayName": "Metadata/Owners/Owner/EmailAddresses/EmailAddress",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/EmailAddresses/EmailAddress",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/EmailAddresses/EmailAddressesXmlSchemaComplexType/EmailAddress/StringP255"
  },
  {
    "displayName": "Metadata/Owners/Owner/LogoURI",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/LogoURI",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/LogoURI/LogoURIDatatype_anyURI"
  },
  {
    "displayName": "Metadata/Owners/Owner/Organisation/Name/Representation/Abbreviation",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Organisation/Name/Representation/Abbreviation",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Organisation/Organisation/Name/Label/Representation/RepresentationXmlSchemaComplexType/Abbreviation/String50"
  },
  {
    "displayName": "Metadata/Owners/Owner/Organisation/Name/Representation/Text",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Organisation/Name/Representation/Text",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Organisation/Organisation/Name/Label/Representation/RepresentationXmlSchemaComplexType/Text/String255"
  },
  {
    "displayName": "Metadata/Owners/Owner/Organisation/OrgUnits/OrgUnit",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Organisation/OrgUnits/OrgUnit",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Organisation/Organisation/OrgUnits/OrgUnitsXmlSchemaComplexType/OrgUnit/StringL"
  },
  {
    "displayName": "Metadata/Owners/Owner/Person/AtomisedName/GivenNames",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Person/AtomisedName/GivenNames",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Person/PersonName/AtomisedName/AtomisedNameXmlSchemaComplexType/GivenNames/String255"
  },
  {
    "displayName": "Metadata/Owners/Owner/Person/AtomisedName/InheritedName",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Person/AtomisedName/InheritedName",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Person/PersonName/AtomisedName/AtomisedNameXmlSchemaComplexType/InheritedName/String255"
  },
  {
    "displayName": "Metadata/Owners/Owner/Person/AtomisedName/PreferredName",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Person/AtomisedName/PreferredName",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Person/PersonName/AtomisedName/AtomisedNameXmlSchemaComplexType/PreferredName/String255"
  },
  {
    "displayName": "Metadata/Owners/Owner/Person/AtomisedName/Prefix",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Person/AtomisedName/Prefix",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Person/PersonName/AtomisedName/AtomisedNameXmlSchemaComplexType/Prefix/String255"
  },
  {
    "displayName": "Metadata/Owners/Owner/Person/AtomisedName/Suffix",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Person/AtomisedName/Suffix",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Person/PersonName/AtomisedName/AtomisedNameXmlSchemaComplexType/Suffix/String255"
  },
  {
    "displayName": "Metadata/Owners/Owner/Person/FullName",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Person/FullName",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Person/PersonName/FullName/String255"
  },
  {
    "displayName": "Metadata/Owners/Owner/Person/SortingName",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Person/SortingName",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Person/PersonName/SortingName/String255"
  },
  {
    "displayName": "Metadata/Owners/Owner/Roles/Role",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/Roles/Role",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/Roles/RolesXmlSchemaComplexType/Role/StringL"
  },
  {
    "displayName": "Metadata/Owners/Owner/TelephoneNumbers/TelephoneNumber/Device",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/TelephoneNumbers/TelephoneNumber/Device",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/TelephoneNumbers/TelephoneNumbersXmlSchemaComplexType/TelephoneNumber/TelephoneNumber/Device/TelephoneDevice"
  },
  {
    "displayName": "Metadata/Owners/Owner/TelephoneNumbers/TelephoneNumber/Number",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/TelephoneNumbers/TelephoneNumber/Number",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/TelephoneNumbers/TelephoneNumbersXmlSchemaComplexType/TelephoneNumber/TelephoneNumber/Number/String255"
  },
  {
    "displayName": "Metadata/Owners/Owner/TelephoneNumbers/TelephoneNumber/UsageNotes",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/TelephoneNumbers/TelephoneNumber/UsageNotes",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/TelephoneNumbers/TelephoneNumbersXmlSchemaComplexType/TelephoneNumber/TelephoneNumber/UsageNotes/StringL"
  },
  {
    "displayName": "Metadata/Owners/Owner/URIs/URL",
    "displayNameLong": "(Basic ABCD) Metadata/Owners/Owner/URIs/URL",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Owners/OwnersXmlSchemaComplexType/Owner/Contact/URIs/URIsXmlSchemaComplexType/URL/anyUriP"
  },
  {
    "displayName": "Metadata/RevisionData/Contributors",
    "displayNameLong": "(Basic ABCD) Metadata/RevisionData/Contributors",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/RevisionData/RevisionData/Contributors/String"
  },
  {
    "displayName": "Metadata/RevisionData/Creators",
    "displayNameLong": "(Basic ABCD) Metadata/RevisionData/Creators",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/RevisionData/RevisionData/Creators/String"
  },
  {
    "displayName": "Metadata/RevisionData/DateCreated",
    "displayNameLong": "(Basic ABCD) Metadata/RevisionData/DateCreated",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/RevisionData/RevisionData/DateCreated/DateCreatedDatatype_dateTime"
  },
  {
    "displayName": "Metadata/RevisionData/DateModified",
    "displayNameLong": "(Basic ABCD) Metadata/RevisionData/DateModified",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/RevisionData/RevisionData/DateModified/DateModifiedDatatype_dateTime"
  },
  {
    "displayName": "Metadata/Scope/GeoecologicalTerms/GeoEcologicalTerm",
    "displayNameLong": "(Basic ABCD) Metadata/Scope/GeoecologicalTerms/GeoEcologicalTerm",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Scope/ScopeXmlSchemaComplexType/GeoecologicalTerms/GeoecologicalTermsXmlSchemaComplexType/GeoEcologicalTerm/StringL255"
  },
  {
    "displayName": "Metadata/Scope/TaxonomicTerms/TaxonomicTerm",
    "displayNameLong": "(Basic ABCD) Metadata/Scope/TaxonomicTerms/TaxonomicTerm",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Scope/ScopeXmlSchemaComplexType/TaxonomicTerms/TaxonomicTermsXmlSchemaComplexType/TaxonomicTerm/StringL255"
  },
  {
    "displayName": "Metadata/Version/DateIssued",
    "displayNameLong": "(Basic ABCD) Metadata/Version/DateIssued",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Version/VersionXmlSchemaComplexType/DateIssued/DateIssuedDatatype_date"
  },
  {
    "displayName": "Metadata/Version/Major",
    "displayNameLong": "(Basic ABCD) Metadata/Version/Major",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Version/VersionXmlSchemaComplexType/Major/MajorDatatype_nonNegativeInteger"
  },
  {
    "displayName": "Metadata/Version/Minor",
    "displayNameLong": "(Basic ABCD) Metadata/Version/Minor",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Version/VersionXmlSchemaComplexType/Minor/MinorDatatype_nonNegativeInteger"
  },
  {
    "displayName": "Metadata/Version/Modifier",
    "displayNameLong": "(Basic ABCD) Metadata/Version/Modifier",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/Metadata/ContentMetadata/Version/VersionXmlSchemaComplexType/Modifier/String255"
  },
  {
    "displayName": "metadataProvider/address/administrativeArea",
    "displayNameLong": "(GBIF) metadataProvider/address/administrativeArea",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/address/addressXmlSchemaComplexType/administrativeArea/NonEmptyStringType"
  },
  {
    "displayName": "metadataProvider/address/city",
    "displayNameLong": "(GBIF) metadataProvider/address/city",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/address/addressXmlSchemaComplexType/city/NonEmptyStringType"
  },
  {
    "displayName": "metadataProvider/address/country",
    "displayNameLong": "(GBIF) metadataProvider/address/country",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/address/addressXmlSchemaComplexType/country/NonEmptyStringType"
  },
  {
    "displayName": "metadataProvider/address/deliveryPoint",
    "displayNameLong": "(GBIF) metadataProvider/address/deliveryPoint",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/address/addressXmlSchemaComplexType/deliveryPoint/NonEmptyStringType"
  },
  {
    "displayName": "metadataProvider/address/postalCode",
    "displayNameLong": "(GBIF) metadataProvider/address/postalCode",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/address/addressXmlSchemaComplexType/postalCode/NonEmptyStringType"
  },
  {
    "displayName": "metadataProvider/electronicMailAddress",
    "displayNameLong": "(GBIF) metadataProvider/electronicMailAddress",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/electronicMailAddress/NonEmptyStringType"
  },
  {
    "displayName": "metadataProvider/individualName/givenName",
    "displayNameLong": "(GBIF) metadataProvider/individualName/givenName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/individualName/individualNameXmlSchemaComplexType/givenName/NonEmptyStringType"
  },
  {
    "displayName": "metadataProvider/individualName/surName",
    "displayNameLong": "(GBIF) metadataProvider/individualName/surName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/individualName/individualNameXmlSchemaComplexType/surName/NonEmptyStringType"
  },
  {
    "displayName": "metadataProvider/onlineUrl",
    "displayNameLong": "(GBIF) metadataProvider/onlineUrl",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/onlineUrl/onlineUrlDatatype_anyURI"
  },
  {
    "displayName": "metadataProvider/organizationName",
    "displayNameLong": "(GBIF) metadataProvider/organizationName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/organizationName/NonEmptyStringType"
  },
  {
    "displayName": "metadataProvider/phone",
    "displayNameLong": "(GBIF) metadataProvider/phone",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/phone/phoneDatatype_string"
  },
  {
    "displayName": "metadataProvider/positionName",
    "displayNameLong": "(GBIF) metadataProvider/positionName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/metadataProvider/agentType/positionName/NonEmptyStringType"
  },
  {
    "displayName": "methods/methodStep/description",
    "displayNameLong": "(GBIF) methods/methodStep/description",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/methods/methodsXmlSchemaComplexType/methodStep/description/description/descriptionXmlSchemaComplexType"
  },
  {
    "displayName": "methods/qualityControl/description",
    "displayNameLong": "(GBIF) methods/qualityControl/description",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/methods/methodsXmlSchemaComplexType/qualityControl/description/description/descriptionXmlSchemaComplexType"
  },
  {
    "displayName": "methods/sampling/samplingDescription/para",
    "displayNameLong": "(GBIF) methods/sampling/samplingDescription/para",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/methods/methodsXmlSchemaComplexType/sampling/samplingXmlSchemaComplexType/samplingDescription/samplingDescriptionXmlSchemaComplexType/para/paraDatatype_string"
  },
  {
    "displayName": "methods/sampling/studyExtent/description",
    "displayNameLong": "(GBIF) methods/sampling/studyExtent/description",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/methods/methodsXmlSchemaComplexType/sampling/samplingXmlSchemaComplexType/studyExtent/description/description/descriptionXmlSchemaComplexType"
  },
  {
    "displayName": "OtherProviders/OtherProvider",
    "displayNameLong": "(Basic ABCD) OtherProviders/OtherProvider",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/OtherProviders/OtherProvidersXmlSchemaComplexType/OtherProvider/String"
  },
  {
    "displayName": "project/designDescription/description",
    "displayNameLong": "(GBIF) project/designDescription/description",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/designDescription/description/description/descriptionXmlSchemaComplexType"
  },
  {
    "displayName": "project/funding/para",
    "displayNameLong": "(GBIF) project/funding/para",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/funding/fundingXmlSchemaComplexType/para/paraDatatype_string"
  },
  {
    "displayName": "project/personnel/address/administrativeArea",
    "displayNameLong": "(GBIF) project/personnel/address/administrativeArea",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/address/addressXmlSchemaComplexType/administrativeArea/NonEmptyStringType"
  },
  {
    "displayName": "project/personnel/address/city",
    "displayNameLong": "(GBIF) project/personnel/address/city",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/address/addressXmlSchemaComplexType/city/NonEmptyStringType"
  },
  {
    "displayName": "project/personnel/address/country",
    "displayNameLong": "(GBIF) project/personnel/address/country",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/address/addressXmlSchemaComplexType/country/NonEmptyStringType"
  },
  {
    "displayName": "project/personnel/address/deliveryPoint",
    "displayNameLong": "(GBIF) project/personnel/address/deliveryPoint",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/address/addressXmlSchemaComplexType/deliveryPoint/NonEmptyStringType"
  },
  {
    "displayName": "project/personnel/address/postalCode",
    "displayNameLong": "(GBIF) project/personnel/address/postalCode",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/address/addressXmlSchemaComplexType/postalCode/NonEmptyStringType"
  },
  {
    "displayName": "project/personnel/electronicMailAddress",
    "displayNameLong": "(GBIF) project/personnel/electronicMailAddress",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/electronicMailAddress/NonEmptyStringType"
  },
  {
    "displayName": "project/personnel/individualName/givenName",
    "displayNameLong": "(GBIF) project/personnel/individualName/givenName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/individualName/individualNameXmlSchemaComplexType/givenName/NonEmptyStringType"
  },
  {
    "displayName": "project/personnel/individualName/surName",
    "displayNameLong": "(GBIF) project/personnel/individualName/surName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/individualName/individualNameXmlSchemaComplexType/surName/NonEmptyStringType"
  },
  {
    "displayName": "project/personnel/onlineUrl",
    "displayNameLong": "(GBIF) project/personnel/onlineUrl",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/onlineUrl/onlineUrlDatatype_anyURI"
  },
  {
    "displayName": "project/personnel/organizationName",
    "displayNameLong": "(GBIF) project/personnel/organizationName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/organizationName/NonEmptyStringType"
  },
  {
    "displayName": "project/personnel/phone",
    "displayNameLong": "(GBIF) project/personnel/phone",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/phone/phoneDatatype_string"
  },
  {
    "displayName": "project/personnel/positionName",
    "displayNameLong": "(GBIF) project/personnel/positionName",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/positionName/NonEmptyStringType"
  },
  {
    "displayName": "project/personnel/role",
    "displayNameLong": "(GBIF) project/personnel/role",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/personnel/agentWithRoleType/role/roleDatatype_string"
  },
  {
    "displayName": "project/studyAreaDescription/descriptor/descriptorValue",
    "displayNameLong": "(GBIF) project/studyAreaDescription/descriptor/descriptorValue",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/studyAreaDescription/studyAreaDescriptionXmlSchemaComplexType/descriptor/descriptorXmlSchemaComplexType/descriptorValue/descriptorValueDatatype_string"
  },
  {
    "displayName": "project/title",
    "displayNameLong": "(GBIF) project/title",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/project/projectXmlSchemaComplexType/title/i18nString"
  },
  {
    "displayName": "publication/Abstract",
    "displayNameLong": "(Publication) publication/Abstract",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Abstract/AbstractXmlSchemaSimpleType"
  },
  {
    "displayName": "publication/Affiliations",
    "displayNameLong": "(Publication) publication/Affiliations",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Affiliations/AffiliationsDatatype_string"
  },
  {
    "displayName": "publication/Author",
    "displayNameLong": "(Publication) publication/Author",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Author/AuthorDatatype_string"
  },
  {
    "displayName": "publication/comment",
    "displayNameLong": "(Publication) publication/comment",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/comment/commentDatatype_string"
  },
  {
    "displayName": "publication/Curation/access_date",
    "displayNameLong": "(Publication) publication/Curation/access_date",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Curation/curation/access_date/access_dateDatatype_date"
  },
  {
    "displayName": "publication/Curation/curated",
    "displayNameLong": "(Publication) publication/Curation/curated",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Curation/curation/curated/curatedDatatype_boolean"
  },
  {
    "displayName": "publication/Curation/date_added",
    "displayNameLong": "(Publication) publication/Curation/date_added",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Curation/curation/date_added/date_addedDatatype_date"
  },
  {
    "displayName": "publication/Curation/date_modified",
    "displayNameLong": "(Publication) publication/Curation/date_modified",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Curation/curation/date_modified/date_modifiedDatatype_date"
  },
  {
    "displayName": "publication/Curation/rated_by",
    "displayNameLong": "(Publication) publication/Curation/rated_by",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Curation/curation/rated_by/rated_byDatatype_string"
  },
  {
    "displayName": "publication/Identifiers/alt_uri",
    "displayNameLong": "(Publication) publication/Identifiers/alt_uri",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Identifiers/identifiers/alt_uri/alt_uriDatatype_string"
  },
  {
    "displayName": "publication/Identifiers/doi",
    "displayNameLong": "(Publication) publication/Identifiers/doi",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Identifiers/identifiers/doi/doiDatatype_string"
  },
  {
    "displayName": "publication/Identifiers/isbn",
    "displayNameLong": "(Publication) publication/Identifiers/isbn",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Identifiers/identifiers/isbn/isbnDatatype_string"
  },
  {
    "displayName": "publication/Identifiers/issn",
    "displayNameLong": "(Publication) publication/Identifiers/issn",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Identifiers/identifiers/issn/issnDatatype_string"
  },
  {
    "displayName": "publication/Identifiers/uri",
    "displayNameLong": "(Publication) publication/Identifiers/uri",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Identifiers/identifiers/uri/uriDatatype_string"
  },
  {
    "displayName": "publication/issue",
    "displayNameLong": "(Publication) publication/issue",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/issue/issueDatatype_string"
  },
  {
    "displayName": "publication/Journal",
    "displayNameLong": "(Publication) publication/Journal",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Journal/JournalDatatype_string"
  },
  {
    "displayName": "publication/keywords",
    "displayNameLong": "(Publication) publication/keywords",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/keywords/keywordsDatatype_string"
  },
  {
    "displayName": "publication/license",
    "displayNameLong": "(Publication) publication/license",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/license/licenseDatatype_string"
  },
  {
    "displayName": "publication/pages",
    "displayNameLong": "(Publication) publication/pages",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/pages/pagesDatatype_string"
  },
  {
    "displayName": "publication/preprint",
    "displayNameLong": "(Publication) publication/preprint",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/preprint/preprintDatatype_boolean"
  },
  {
    "displayName": "publication/publication_date",
    "displayNameLong": "(Publication) publication/publication_date",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/publication_date/publication_dateDatatype_date"
  },
  {
    "displayName": "publication/Recourses/code_availability",
    "displayNameLong": "(Publication) publication/Recourses/code_availability",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Recourses/recourses/code_availability/code_availabilityDatatype_boolean"
  },
  {
    "displayName": "publication/Recourses/data_availability",
    "displayNameLong": "(Publication) publication/Recourses/data_availability",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Recourses/recourses/data_availability/data_availabilityDatatype_boolean"
  },
  {
    "displayName": "publication/Recourses/Data_code_availiablity_statement",
    "displayNameLong": "(Publication) publication/Recourses/Data_code_availiablity_statement",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Recourses/recourses/Data_code_availiablity_statement/Data_code_availiablity_statementDatatype_string"
  },
  {
    "displayName": "publication/Recourses/Data_code_availiablity_title",
    "displayNameLong": "(Publication) publication/Recourses/Data_code_availiablity_title",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Recourses/recourses/Data_code_availiablity_title/Data_code_availiablity_titleDatatype_string"
  },
  {
    "displayName": "publication/supplementary_info",
    "displayNameLong": "(Publication) publication/supplementary_info",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/supplementary_info/supplementary_infoDatatype_boolean"
  },
  {
    "displayName": "publication/Title",
    "displayNameLong": "(Publication) publication/Title",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Title/TitleDatatype_string"
  },
  {
    "displayName": "publication/Type",
    "displayNameLong": "(Publication) publication/Type",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/Type/TypeDatatype_string"
  },
  {
    "displayName": "publication/volume",
    "displayNameLong": "(Publication) publication/volume",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/publication/publication/volume/volumeDatatype_string"
  },
  {
    "displayName": "purpose/para",
    "displayNameLong": "(GBIF) purpose/para",
    "metadataStructureName": "GBIF",
    "xPath": "Metadata/purpose/purposeXmlSchemaComplexType/para/paraDatatype_string"
  },
  {
    "displayName": "Resource/Access/DOI",
    "displayNameLong": "(Publication) Resource/Access/DOI",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Access/accessType/DOI/DOIDatatype_string"
  },
  {
    "displayName": "Resource/Access/DOI_Health",
    "displayNameLong": "(Publication) Resource/Access/DOI_Health",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Access/accessType/DOI_Health/DOI_HealthDatatype_string"
  },
  {
    "displayName": "Resource/Access/Licence",
    "displayNameLong": "(Publication) Resource/Access/Licence",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Access/accessType/Licence/LicenceDatatype_string"
  },
  {
    "displayName": "Resource/Access/Repository_Name",
    "displayNameLong": "(Publication) Resource/Access/Repository_Name",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Access/accessType/Repository_Name/Repository_NameDatatype_string"
  },
  {
    "displayName": "Resource/Access/URI",
    "displayNameLong": "(Publication) Resource/Access/URI",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Access/accessType/URI/URIDatatype_anyURI"
  },
  {
    "displayName": "Resource/Access/URI_Health",
    "displayNameLong": "(Publication) Resource/Access/URI_Health",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Access/accessType/URI_Health/URI_HealthDatatype_string"
  },
  {
    "displayName": "Resource/Code/Code_Type",
    "displayNameLong": "(Publication) Resource/Code/Code_Type",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Code/code/Code_Type/Code_TypeDatatype_string"
  },
  {
    "displayName": "Resource/Code/Programing_Language",
    "displayNameLong": "(Publication) Resource/Code/Programing_Language",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Code/code/Programing_Language/Programing_LanguageDatatype_string"
  },
  {
    "displayName": "Resource/Embargo",
    "displayNameLong": "(Publication) Resource/Embargo",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Embargo/EmbargoDatatype_boolean"
  },
  {
    "displayName": "Resource/Embargo_End",
    "displayNameLong": "(Publication) Resource/Embargo_End",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Embargo_End/Embargo_EndDatatype_date"
  },
  {
    "displayName": "Resource/File/Data_Type",
    "displayNameLong": "(Publication) Resource/File/Data_Type",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/File/file/Data_Type/Data_TypeDatatype_string"
  },
  {
    "displayName": "Resource/File/File_Format",
    "displayNameLong": "(Publication) Resource/File/File_Format",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/File/file/File_Format/File_FormatDatatype_string"
  },
  {
    "displayName": "Resource/File/SizeIn_MB",
    "displayNameLong": "(Publication) Resource/File/SizeIn_MB",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/File/file/SizeIn_MB/SizeIn_MBDatatype_int"
  },
  {
    "displayName": "Resource/Language",
    "displayNameLong": "(Publication) Resource/Language",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Language/LanguageDatatype_string"
  },
  {
    "displayName": "Resource/Name",
    "displayNameLong": "(Publication) Resource/Name",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Name/NameDatatype_string"
  },
  {
    "displayName": "Resource/Resources_Type",
    "displayNameLong": "(Publication) Resource/Resources_Type",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Resources_Type/resourcesType"
  },
  {
    "displayName": "Resource/Submission_Date",
    "displayNameLong": "(Publication) Resource/Submission_Date",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Submission_Date/Submission_DateDatatype_date"
  },
  {
    "displayName": "Resource/Submitted_By",
    "displayNameLong": "(Publication) Resource/Submitted_By",
    "metadataStructureName": "Publication",
    "xPath": "Metadata/Resource/Resource/Submitted_By/Submitted_ByDatatype_string"
  },
  {
    "displayName": "TechnicalContacts/TechnicalContact/Address",
    "displayNameLong": "(Basic ABCD) TechnicalContacts/TechnicalContact/Address",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/TechnicalContacts/TechnicalContactsXmlSchemaComplexType/TechnicalContact/MicroAgentP/Address/String255"
  },
  {
    "displayName": "TechnicalContacts/TechnicalContact/Email",
    "displayNameLong": "(Basic ABCD) TechnicalContacts/TechnicalContact/Email",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/TechnicalContacts/TechnicalContactsXmlSchemaComplexType/TechnicalContact/MicroAgentP/Email/String255"
  },
  {
    "displayName": "TechnicalContacts/TechnicalContact/Name",
    "displayNameLong": "(Basic ABCD) TechnicalContacts/TechnicalContact/Name",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/TechnicalContacts/TechnicalContactsXmlSchemaComplexType/TechnicalContact/MicroAgentP/Name/String255"
  },
  {
    "displayName": "TechnicalContacts/TechnicalContact/Phone",
    "displayNameLong": "(Basic ABCD) TechnicalContacts/TechnicalContact/Phone",
    "metadataStructureName": "Basic ABCD",
    "xPath": "Metadata/TechnicalContacts/TechnicalContactsXmlSchemaComplexType/TechnicalContact/MicroAgentP/Phone/String255"
  }
]