// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { Api } from '@bexis2/bexis2-core-ui';

// get Model for Edit page
export const getEdit = async (id) => {
	//console.log("edit",id);

	try {
		const response = await Api.get('/dcm/edit/load?id=' + id);
		//console.log(response);

		return response.data;
	} catch (error) {
		console.error('error', error);
		throw error;
	}
};

// get Model for Edit page
export const getHooks = async (id) => {
	//console.log("edit",id);

	try {
		const response = await Api.get('/dcm/edit/hooks?id=' + id);
		//console.log(response);

		return response.data;
	} catch (error) {
		console.error('error', error);
		throw error;
	}
};

// get Model for Edit page
export const submit = async (id) => {
	//console.log("edit",id);

	try {
		const response = await Api.post('/dcm/submit/submit', { id });
		//console.log(response);

		return response.data;
	} catch (error) {
		console.error('error', error);
		throw error;
	}
};

// load message from a dataset edit situation
export const loadMessages = async (id) => {
	try {
		//console.log("test load messages")
		const response = await Api.get('/dcm/messages/load?id=' + id);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

// save file description
export const saveFileDescription = async (action, id, file, description) => {
	try {
		const response = await Api.post(action, { id, file, description });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

// remove file from server
export const removeFile = async (action, id, file) => {
	try {
		const response = await Api.post(action, { id, file });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};
