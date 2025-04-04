
// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { Api, host } from '@bexis2/bexis2-core-ui';

// get Model for Edit page
export const load = async () => {

	try {
		const response = await Api.get('/dim/gbif/load');
		//console.log(response);
		return response;
	} catch (error) {
		console.error('error', error);
		throw error;
	}
};

export const register = async (publicationId:string) => {

	try {
		const response = await Api.post('/dim/gbif/RegisterDataset',{publicationId});
		//console.log(response);
		return response;
	} catch (error) {
		console.error('error', error);
		throw error;
	}
};

// go to a internal action
export const goTo = async (url:string) => {
	window.open(host + url, '_blank').focus();
};
