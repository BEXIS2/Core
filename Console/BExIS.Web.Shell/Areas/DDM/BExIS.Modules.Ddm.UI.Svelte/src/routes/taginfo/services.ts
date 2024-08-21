// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import type { TagInfoModel } from './types';
import { Api } from '@bexis2/bexis2-core-ui';

/****************/
/* Overview Variable Template*/
/****************/

export const getTagInfos = async (id) => {
	try {
		const response = await Api.get('/api/TagInfo/'+id);
		console.log("🚀 ~ getTagInfos ~ response:", response)

		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const save = async (model:TagInfoModel) => {
	try {
		const response = await Api.post('/api/TagInfo/',model);
		console.log("🚀 ~ save ~ response:", response)


		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};
