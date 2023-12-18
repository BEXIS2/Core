import { Api } from '@bexis2/bexis2-core-ui';
import type { externalLinkType } from '$lib/components/meaning/types';

export const getLinks = async () => {
	try {
		const response = await Api.get('/rpm/ExternalLink/get');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getLinkTypes = async () => {
	try {
		const response = await Api.get('/rpm/ExternalLink/getLinkTypes');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getPrefixCategoriesAsList = async () => {
	try {
		const response = await Api.get('/rpm/ExternalLink/GetPrefixCategoriesAsList');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getPrefixListItems = async () => {
	try {
		const response = await Api.get('/rpm/ExternalLink/GetPrefixListItems');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const remove = async (id) => {
	try {
		const response = await Api.delete('/rpm/ExternalLink/delete?id='+id,null);
		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const create = async (data:externalLinkType) => {
	try {
		
		console.log("🚀 ~ file: services.ts:38 ~ create ~ data:", data)
		const response = await Api.post('/rpm/ExternalLink/create',data);
		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const update = async (data:externalLinkType) => {
	try {

		const response = await Api.post('/rpm/ExternalLink/update',data);
		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

