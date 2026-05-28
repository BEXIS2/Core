import { Api } from '@bexis2/bexis2-core-ui';

export const GetMetadataSchema = async (id: number) => {
	try {
		const response = await Api.get('/api/MetadataStructure/' + id);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetMetadata = async (id: number) => {
	try {
		const response = await Api.get('/api/Metadata/' + id + '?simplifiedJson=1');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};


export const GetComponentConfig = async (id: any, type: string) => {
	try {
	
		const response = await Api.get('/dcm/ComponentConfig/LoadConfig/?id=' + id + "&type=" + type);
		// console.log('Dataset filled:', response);
		console.log('config loaded:', response);
		return response;
	} catch (error) {
		console.error('error during saving config:', error);
		throw error;
	}
};

export const GetDatasetInfoById = async (datasetId: number) => {
	try {
		const response = await Api.get('/api/dataset/' + datasetId);
		// console.log(" response.data.MetadataStructureId:", response);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const SaveMetadata = async (id: number, value: any, comment: string) => {
	try {
		console.log(" value:", value);
		value["@comment"] = comment;
		const response = await Api.put('/api/Metadata/' + id, value);
		// console.log('Dataset filled:', response);
		return response.data;
	} catch (error) {
		console.error('error during saving metadata:', error);
		throw error;
	}
};

export const GetSystemMappings = async (metadataStructureId: number) => {
	try {
		const response = await Api.get('/dcm/m/LoadSystemMappings?id=' + metadataStructureId);
		// console.log(" response.data.MetadataStructureId:", response);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetPartyValue = async (partyid: number, linkid) => {
	try {
		const response = await Api.get('/dcm/m/GetPartyValue?partyId=' + partyid + '&linkId=' + linkid);
		// console.log(" response.data.MetadataStructureId:", response);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};


export const GetMetadataAsJson = async (id: number, version) => {
	try {

		const header = { 'Accept': 'application/json' }
		const config = { responseType: 'blob' }

		const response = await Api.get('/api/metadata/' + id + "?version=" + version + "&simplifiedJson=2", '', header, config);
		// console.log(" response.data.MetadataStructureId:", response);

		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetMetadataAsXml = async (id: number, version) => {
	try {

		const header = { 'Accept': 'application/xml' }
		const config = { responseType: 'blob' }

		const response = await Api.get('/api/metadata/' + id + '/version_number/' + version + '?format=1', '', header, config);
		// console.log(" response.data.MetadataStructureId:", response);

		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetMetadataAsFlattened = async (id: number, version) => {
	try {

		const header = { 'Accept': 'text/plain' }
		const config = { responseType: 'blob' }

		//http://localhost:44345/api/metadata/1/version_number/1/?format=3
		const response = await Api.get('/api/metadata/' + id + '/version_number/' + version + '?format=3', '', header, config);
		// console.log(" response.data.MetadataStructureId:", response);

		return response.data;
	} catch (error) {
		console.error(error);
		alert("An error occurred while downloading the metadata. Please try again later.");
		throw error;
	}
};



