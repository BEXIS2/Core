import { Api } from '@bexis2/bexis2-core-ui';

import type { UnitListItem } from '../models';

export const GetUnits = async () => {
	try {
		const response = await Api.get('/rpm/unit/GetUnits');
		// console.log('get units', response.data);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetMeasurementSystems = async () => {
	try {
		const response = await Api.get('/rpm/unit/GetMeasurementSystems');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetDataTypes = async () => {
	try {
		const response = await Api.get('/rpm/unit/GetDataTypes');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetDimensions = async () => {
	try {
		const response = await Api.get('/rpm/unit/GetDimensions');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const EditUnit = async (unitListItem: UnitListItem) => {
	try {
		const response = await Api.post('/rpm/unit/EditUnit', unitListItem);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const DeleteUnit = async (id: number) => {
	try {
		const response = await Api.post('/rpm/unit/DeleteUnit', { id });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};
