import { Api } from '@bexis2/bexis2-core-ui';
import type { Dataset } from '../models.js';

export const getEntityTemplateList = async () => {
	try {
		const response = await Api.get('/dcm/entitytemplates/Load');
		console.log('response', response.data);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const createDataset = async (dataset: Dataset) => {
	try {
		const response = await Api.post('/api/Dataset', dataset);
		// console.log('Dataset created:', response);
		return response.data;
	} catch (error) {
		console.error('Fehler beim Erstellen des Datasets:', error);
		throw error;
	}
};

export const putMetadata = async (id: number, value: any) => {
	try {
		// console.log(" value:", value);

		const response = await Api.put('/api/Metadata/' + id, value);
		// console.log('Dataset filled:', response);
		return response.data;
	} catch (error) {
		console.error('Fehler beim füllen vom dataset:', error);
		throw error;
	}
};

export const GetMetadata = async (id: number) => {
	try {
		const response = await Api.get('/api/Metadata/' + id + '?simplifiedJson=1');
		// console.log('response', response.data);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetMetadataScheema = async (id: number) => {
	try {
		const response = await Api.get('/api/MetadataStructure/' + id);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};
