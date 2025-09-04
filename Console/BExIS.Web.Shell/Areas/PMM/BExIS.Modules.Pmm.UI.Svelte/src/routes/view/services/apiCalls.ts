import { Api } from '@bexis2/bexis2-core-ui';

export const GetMetadata = async (id: number) => {
	try {
		const response = await Api.get('/api/Metadata/' + id + '?simplifiedJson=1');
		// console.log('response', response.data);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};
