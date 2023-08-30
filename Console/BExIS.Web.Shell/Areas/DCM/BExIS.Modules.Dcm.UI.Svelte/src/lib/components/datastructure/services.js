// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { Api } from '@bexis2/bexis2-core-ui';

/****************/
/* Create*/
/****************/
export const load = async (entityId, file, version) => {
	try {
		const response = await Api.get(
			'/dcm/DataStructure/load?entityId=' + entityId + '&&file=' + file + '&&version=' + version
		);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getStructures = async () => {
	try {
		const response = await Api.get('/dcm/DataStructure/GetStructures');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getDisplayPattern = async () => {
	try {
		const response = await Api.get('/dcm/DataStructure/GetDisplayPattern');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getDelimeters = async () => {
	try {
		const response = await Api.get('/dcm/DataStructure/GetDelimters');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const generate = async (data) => {
	try {
		const response = await Api.post('/dcm/DataStructure/generate', data);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const store = async (data) => {
	try {
		const response = await Api.post('/dcm/DataStructure/store', data);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const save = async (data) => {
	try {
		const response = await Api.post('/dcm/DataStructure/save', data);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getDataTypes = async () => {
	try {
		const response = await Api.get('/dcm/DataStructure/getDataTypes');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getUnits = async () => {
	try {
		const response = await Api.get('/dcm/DataStructure/getUnits');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};
