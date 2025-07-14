import { Api } from '@bexis2/bexis2-core-ui';

export const GetMetadataSchema = async (id: number) => {
	try {
		const response = await Api.get('/api/MetadataStructure/'+ id);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetMetadata = async (id: number) => {
	try {
		const response = await Api.get('/api/Metadata/'+ id + '?simplifiedJson=1');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};