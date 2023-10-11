import { Api } from '@bexis2/bexis2-core-ui';

export const getMeanings = async () => {
	try {
		const response = await Api.get('/api/Meanings/Index');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const remove = async (id) => {
	try {
		const response = await Api.delete('/api/MeaningsAdmin/delete?id='+id,null);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};