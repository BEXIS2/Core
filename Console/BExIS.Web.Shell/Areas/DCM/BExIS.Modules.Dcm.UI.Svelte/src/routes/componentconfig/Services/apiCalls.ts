import { Api } from '@bexis2/bexis2-core-ui';

export const GetMetadataSchema_old = async (id: number) => {
    try {
        // // API
        // const response = await Api.get('/api/MetadataStructure/' + id);
        // return response.data;
        
        // local load
        const response = await fetch(`/src/routes/componentconfig/Schema/metadataSchema_${id}.json`);
        if (!response.ok) {
            throw new Error(`Schema file not found: metadataSchema_${id}.json`);
        }
        const data = await response.json();
        return data;
    } catch (error) {
        throw error;
    }
};

export const GetMetadataSchema = async (id: number) => {
	try {
		const response = await Api.get('/api/MetadataStructure/' + id);
		console.log(" response.data in GetMetadataSchema:", response);
        return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

/*export const GetMetadata = async (id: number) => {
	try {
		const response = await Api.get('/api/Metadata/' + id + '?simplifiedJson=1');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};*/

export const GetSchemaIdByDatasetId = async (datasetId: number) => {
	try {
		const response = await Api.get('/api/dataset/' + datasetId);
		console.log(" response.data.MetadataStructureId:", response);
		return response.data.metadataStructureId;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const SaveConfig = async (value: any, id: number, type: string) => {
	try {
        console.log(" value:", value);
        const value_string = JSON.stringify(value);
		const data = {"id":id, "type":type, "content":value_string};
		const response = await Api.post('/dcm/ComponentConfig/SavConfige', data);
		// console.log('Dataset filled:', response);
        console.log('config saved:', response);
		return response.data;
    

	} catch (error) {
		console.error('error during saving config:', error);
		throw error;
	}
};

export const LoadConfig = async (id: any, type: string) => {
	try {
	
		const response = await Api.get('/dcm/ComponentConfig/LoadConfig/?id=' + id + "&type=" + type);
		// console.log('Dataset filled:', response);
		return response.data;
	} catch (error) {
		console.error('error during saving config:', error);
		throw error;
	}
};

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



