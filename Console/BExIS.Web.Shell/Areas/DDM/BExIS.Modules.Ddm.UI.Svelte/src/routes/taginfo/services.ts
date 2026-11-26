// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import type { TagInfoEditModel, TagType } from './types';
import { Api } from '@bexis2/bexis2-core-ui';

/****************/
/* Overview Variable Template*/
/****************/

export const get = async (id) => {
	try {
		const response = await Api.get('/api/datasets/' + id + '/tags');

		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getView = async (id) => {
	try {
		const response = await Api.get('/api/datasets/' + id + '/tags/simple');

		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const save = async (model: TagInfoEditModel) => {
	try {
		const response = await Api.post('/api/TagInfo/', model);
		console.log('🚀 ~ save ~ response:', response);

		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const updateSearch = async (id: number) => {
	try {
		const response = await Api.get('/ddm/taginfo/updatesearch?id=' + id);

		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const add = async (model: TagInfoEditModel, type: TagType) => {
	try {
		console.log('🚀 ~ add ~ model:', { model, type });
		const response = await Api.put('/api/taginfo?type=' + type, model);
		console.log('🚀 ~ add ~ response:', response);

		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const isCurator = async () => {
	try {
		const response = await Api.get('/ddm/taginfo/GetUserRole');

		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
}

export const isCuratorRequired = async () => {
	try {
		const response = await Api.get('/ddm/taginfo/GetCuratorRequired');

		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
}

export const sendRequestForTagApproval = async (data: { message: string }) => {
	try {
		console.log('🚀 ~ sendRequestForTagApproval ~ data:', data);
		const response = await Api.post('/ddm/taginfo/SendTagRequest', data);

		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
}
