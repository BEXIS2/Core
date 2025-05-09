import type { CurationEntryModel, CurationModel } from './types';
import { Api } from '@bexis2/bexis2-core-ui';

/**
 * Translate first letter of the keys to lowercase
 *
 * @param response - response object (could be an array or an object)
 * @returns - new object with first letter of the keys in lowercase
 */
const firstLetterToLowerCase = (response: any) => {
	const newEntry: any = {};
	for (const key in response) {
		newEntry[key.charAt(0).toLowerCase() + key.slice(1)] = response[key];
		if (response[key] && typeof response[key] === 'object') {
			if (Array.isArray(response[key])) {
				newEntry[key.charAt(0).toLowerCase() + key.slice(1)] = response[key].map(
					firstLetterToLowerCase as any
				);
			} else {
				newEntry[key.charAt(0).toLowerCase() + key.slice(1)] = firstLetterToLowerCase(
					response[key]
				);
			}
		}
	}
	return newEntry;
};

export const get = async () => {
	try {
		const response = await Api.get('/api/curationentries');

		console.log('🎈 ~ get ~ response:', response);

		response.data = response.data.map(firstLetterToLowerCase);

		return response.data as CurationEntryModel[];
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getCurationDataset = async (id: number) => {
	try {
		const response = await Api.get(`/api/datasets/${id}/curation`);

		console.log('🎈 ~ getByDataset ~ response:', response);

		response.data = firstLetterToLowerCase(response.data);

		return response.data as CurationModel;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const putCurationEntry = async (model: CurationEntryModel) => {
	try {
		const response = await Api.put('/api/curationentries', model);

		console.log('🎈 ~ PUT ~ Response:', response);

		response.data = firstLetterToLowerCase(response.data);

		return response.data as CurationEntryModel;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const postCurationEntry = async (model: CurationEntryModel) => {
	try {
		model.id = 0; // Set id to 0 to create a new entry

		const response = await Api.post('/api/curationentries', model);

		console.log('🎈 ~ POST ~ response:', response);

		response.data = firstLetterToLowerCase(response.data);

		return response.data as CurationEntryModel;
	} catch (error) {
		console.error(error);
		throw error;
	}
};
