import { Api } from '@bexis2/bexis2-core-ui';
import type { MeaningModel, externalLinkType } from './types';

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

export const update = async (data:MeaningModel) => {
	try {
		const response = await Api.post('/api/MeaningsAdmin/create',data);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const createLink = async (data:externalLinkType) => {
	try {
		const response = await Api.post('/api/MeaningsAdmin/createExternalLink',data);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};