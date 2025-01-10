import { Api } from '@bexis2/bexis2-core-ui';

export const GetMetadataScheema = async (id: number) => {
	try {
		const response = await Api.get('/api/MetadataStructure/'+id+'');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};