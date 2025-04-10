// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { Api } from '@bexis2/bexis2-core-ui';

export const revertFile = async (id, file) => {
	try {
		const response = await Api.get('/dcm/data/revertFile?file=' + file + '&&id=' + id);

		return response;
	} catch (error) {
		console.error('ERROR : LOAD', error);
		throw error;
	}
};
