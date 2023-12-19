// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { Api } from '@bexis2/bexis2-core-ui';

/****************/
/* Overview Data structures*/
/****************/

export const getDataStructures = async () => {
	try {
		const response = await Api.get('/rpm/DataStructure/DataStructures');
		console.log("responce",response.data ,Date.now()/1000);
		return response.data;

	} catch (error) {
		console.error(error);
	}
};

export const get = async (id) => {
	try {
		const response = await Api.get('/rpm/DataStructure/get?id='+id);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

/****************/
/* Create*/
/****************/
export const load = async (file,entityId,version) => {
	console.log(file,entityId,version);
	try {
		const response = await Api.get(
			'/rpm/DataStructure/load?file=' + file + '&&entityId=' + entityId + '&&version=' + version
		);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getStructures = async () => {
	try {
		const response = await Api.get('/rpm/DataStructure/GetStructures');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getDisplayPattern = async () => {
	try {
		const response = await Api.get('/rpm/DataStructure/GetDisplayPattern');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getDelimeters = async () => {
	try {
		const response = await Api.get('/rpm/DataStructure/GetDelimters');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const generate = async (data) => {
	try {
		const response = await Api.post('/rpm/DataStructure/generate', data);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};


export const empty = async () => {
	try {
		const response = await Api.get('/rpm/DataStructure/empty');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const copy = async (id) => {
	try {
		const response = await Api.get('/rpm/DataStructure/copy?id='+id);

		console.log("🚀 ~ file: services.js:95 ~ copy ~ response.data:", response.data)
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const remove = async (id) => {
	try {
		const response = await Api.post('/rpm/DataStructure/delete',{id});
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const store = async (data) => {
	try {
		const response = await Api.post('/rpm/DataStructure/store', data);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const create = async (data) => {
	try {
		const response = await Api.post('/rpm/DataStructure/create', data);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const save = async (data) => {
	try {
		const response = await Api.post('/rpm/DataStructure/save', data);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getDataTypes = async () => {
	try {
		const response = await Api.get('/rpm/DataStructure/getDataTypes');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getUnits = async () => {
	try {
		const response = await Api.get('/rpm/DataStructure/getUnits');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getUnitsWithDataTypes = async () => {
	try {
		const response = await Api.get('/rpm/variableTemplate/getUnits');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getVariableTemplates = async () => {
	try {
		const response = await Api.get('/rpm/DataStructure/getVariableTemplates');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};


export const getMeanings = async () => {
	try {
		const response = await Api.get('/rpm/DataStructure/getMeanings');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	
	}
};

export const getConstraints = async () => {
	try {
		const response = await Api.get('/rpm/DataStructure/GetConstraints');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

