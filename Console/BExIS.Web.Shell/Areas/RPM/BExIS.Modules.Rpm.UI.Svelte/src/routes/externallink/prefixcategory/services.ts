import { Api } from '@bexis2/bexis2-core-ui';
import type { prefixCategoryType } from '$lib/components/meaning/types';

export const getPrefixCategories = async () => {
	try {
		const response = await Api.get('/rpm/ExternalLink/GetPrefixCategories');
		console.log("🚀 ~ file: services.ts:8 ~ getLinks ~ response.data:", response.data)
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const remove = async (id) => {
	try {
		const response = await Api.delete('/rpm/ExternalLink/DeletePrefixCategory?id='+id,null);
		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const create = async (data:prefixCategoryType) => {
	try {

		const response = await Api.post('/rpm/ExternalLink/CreatePrefixCategories',data);
		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const update = async (data:prefixCategoryType) => {
	try {

		const response = await Api.post('/rpm/ExternalLink/updatePrefixCategories',data);
		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

