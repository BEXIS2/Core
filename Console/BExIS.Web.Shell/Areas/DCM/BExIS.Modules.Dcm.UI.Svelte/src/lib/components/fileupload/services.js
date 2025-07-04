// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { Api } from '@bexis2/bexis2-core-ui';

export const load = async (file, entityId, version) => {
	try {
		const response = await Api.get(
			'/rpm/File/load?file=' + file + '&&entityId=' + entityId + '&&version=' + version
		);
		console.log('🚀 ~ load ~ response:', response);

		return response.data;
	} catch (error) {
		console.error('ERROR : LOAD', error);
		throw error;
	}
};
