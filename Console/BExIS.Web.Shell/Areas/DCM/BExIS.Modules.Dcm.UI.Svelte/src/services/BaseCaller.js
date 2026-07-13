// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { host, Api } from '@bexis2/bexis2-core-ui';

// go to a internal action
export const goTo = async (url, intern = true) => {
	if (intern == true) {
		// go to inside bexis2
		if (window != null && host != null && url != null) {
			window.open(host + url, '_self')?.focus();
		}
	} // go to a external page
	else {
		window.open(url, '_blank')?.focus();
	}
};

export const getToken = async () => {
	try {
		const response = await Api.get('/tokens/get');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const goToEntity = async (url, id, version, tag) => {

 if(version>=0 && tag==0){
		goTo(`${url}?id=${id}&version=${version}`);
	}

	if(version==0 && tag!=0){
		goTo(`${url}?${id}&tag=${tag}`);
	}
};

