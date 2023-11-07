import { Api } from '@bexis2/bexis2-core-ui';
import type { ConstraintListItem } from '../models';

export const GetDimensions = async () => {
	try {
		const response = await Api.get('/rpm/constraints/GetConstraints');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetConstraintTypes = async () => {
	try {
		const response = await Api.get('/rpm/constraints/GetConstraintTypes');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};