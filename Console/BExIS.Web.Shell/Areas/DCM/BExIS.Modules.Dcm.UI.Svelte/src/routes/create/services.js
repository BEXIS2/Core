// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { Api } from '@bexis2/bexis2-core-ui';

/****************/
/* Create*/
/****************/

export const getEntityTemplateList = async () => {
	try {
		const response = await Api.get('/dcm/create/GetEntityTemplateList');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};
