import { Api } from '@bexis2/bexis2-core-ui';
import type { DataTypeListItem } from '../models';

export const GetDataTypes = async () => {
	try {
		const response = await Api.get('/rpm/dataType/GetDataTypes');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetSystemTypes = async () => {
	try {
		const response = await Api.get('/rpm/dataType/GetSystemTypes');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const EditDataType = async (dataTypeListItem: DataTypeListItem) => {
	try {
		const response = await Api.post('/rpm/dataType/EditDataType', dataTypeListItem);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const DeleteDataType = async (id: number) => {
	try {
		const response = await Api.post('/rpm/dataType/DeleteDataType', {id});
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};
