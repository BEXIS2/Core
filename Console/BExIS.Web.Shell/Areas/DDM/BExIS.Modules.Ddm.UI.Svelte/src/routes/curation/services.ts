import type { CurationEntryModel, CurationModel, CurationsOverviewModel } from './types';
import { Api } from '@bexis2/bexis2-core-ui';

/**
 * Translate first letter of the keys to lowercase.
 * This may not be needed if the backend returns the keys as lowerCase
 *
 * @param response - response object (could be an array or an object)
 * @returns - new object with first letter of the keys in lowercase
 */
function firstLetterToLowerCase(response: any): any {
	if (Array.isArray(response)) {
		// Only process array elements if they are objects (not strings, numbers, etc.)
		return response.map((item) =>
			typeof item === 'object' && item !== null ? firstLetterToLowerCase(item) : item
		);
	} else if (response && typeof response === 'object') {
		const newEntry: any = {};
		for (const key in response) {
			const newKey = key.charAt(0).toLowerCase() + key.slice(1);
			newEntry[newKey] = firstLetterToLowerCase(response[key]);
		}
		return newEntry;
	}
	return response;
}

function fixModel(model: CurationEntryModel): CurationEntryModel {
	if (model.name.trim() === '') model.name = 'None';
	if (model.description.trim() === '') model.description = 'None';
	if (model.topic.trim() === '') model.topic = 'None';
	if (model.solution.trim() === '') model.solution = 'None';
	if (model.source.trim() === '') model.source = 'None';
	return model;
}

export const getCurationEntries = async () => {
	const response = await Api.get('/api/curationentries');

	console.log('🎈 ~ GET ~ response:', response);

	response.data = firstLetterToLowerCase(response.data);

	return response.data as CurationsOverviewModel;
};

export const getCurationDataset = async (id: number) => {
	const response = await Api.get(`/api/datasets/${id}/curation`);

	console.log('🎈 ~ GET by dataset ~ response:', response);

	response.data = firstLetterToLowerCase(response.data);

	return response.data as CurationModel;
};

export const putCurationEntry = async (model: CurationEntryModel) => {
	model = fixModel(model);

	const response = await Api.put('/api/curationentries', model);

	console.log('🎈 ~ PUT ~ Response:', response);

	response.data = firstLetterToLowerCase(response.data);

	return response.data as CurationEntryModel;
};

export const postCurationEntry = async (model: CurationEntryModel) => {
	model = fixModel(model);

	model.id = 0; // Set id to 0 to create a new entry

	const response = await Api.post('/api/curationentries', model);

	console.log('🎈 ~ POST ~ response:', response);

	response.data = firstLetterToLowerCase(response.data);

	return response.data as CurationEntryModel;
};
