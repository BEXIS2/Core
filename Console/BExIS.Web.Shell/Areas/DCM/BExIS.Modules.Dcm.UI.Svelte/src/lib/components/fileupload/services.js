// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { Api } from '@bexis2/bexis2-core-ui';

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
