import { Api } from '@bexis2/bexis2-core-ui';
import type { DimensionListItem } from '../models';

export const GetDimensions = async () => {
	try {
		const response = await Api.get('/rpm/dimension/GetDimensions');
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const EditDimension = async (dimensionListItem: DimensionListItem) => {
	try {
		const response = await Api.post('/rpm/dimension/EditDimension', dimensionListItem);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const DeleteDimension = async (id: number) => {
	try {
		const response = await Api.post('/rpm/dimension/DeleteDimension', { id });
		return response.data;
	} catch (error) {
		console.error(error);
	}
};
