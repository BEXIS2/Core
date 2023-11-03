import { Api } from '@bexis2/bexis2-core-ui';
import type { MeaningModel, externalLinkType } from '$lib/components/meaning/types';

export const getMeanings = async () => {
	try {
		const response = await Api.get('/rpm/Meaning/get');
		console.log("🚀 ~ file: services.ts:8 ~ getMeanings ~ response.data:", response.data)
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};


export const getLinks = async () => {
	try {
		const response = await Api.get('/rpm/Meaning/getlinks');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const remove = async (id) => {
	try {
		const response = await Api.delete('/rpm/Meaning/delete?id='+id,null);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const create = async (data:MeaningModel) => {
	try {

		const response = await Api.post('/rpm/Meaning/create',data);
		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const update = async (data:MeaningModel) => {
	try {

		const response = await Api.post('/rpm/Meaning/update',data);
		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const createLink = async (data:externalLinkType) => {
	try {
		const response = await Api.post('/rpm/meaning/createLink',data);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};