// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { Api } from '@bexis2/bexis2-core-ui';

/****************/
/* Create*/
/****************/
export const availableStructues = async (id) => {
	try {
		const response = await Api.get('/dcm/datadescription/availableStructues?id=' + id);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const setStructure = async (id, structureId) => {
	try {
		const response = await Api.put('/dcm/datadescription/set', { id, structureId });
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const deleteStructure = async (id, structureId) => {
	try {
		const response = await Api.delete('/dcm/datadescription/delete', { id, structureId });
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const removeStructure = async (id) => {
	try {
		const response = await Api.delete('/dcm/datadescription/remove', { id });
		return response.data;
	} catch (error) {
		console.error(error);
	}
};
