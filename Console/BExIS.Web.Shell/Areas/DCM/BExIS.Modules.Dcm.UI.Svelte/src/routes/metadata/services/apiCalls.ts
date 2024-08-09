import { Api } from '@bexis2/bexis2-core-ui';

export const GetConstraints = async () => {
	try {
		const response = await Api.get('/');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};