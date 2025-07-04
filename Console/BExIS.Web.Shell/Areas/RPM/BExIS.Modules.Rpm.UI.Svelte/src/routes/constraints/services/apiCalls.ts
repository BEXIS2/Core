import { Api } from '@bexis2/bexis2-core-ui';
import type {
	ConstraintListItem,
	DomainConstraintListItem,
	PatternConstraintListItem,
	RangeConstraintListItem
} from '../models';

export const GetConstraints = async () => {
	try {
		const response = await Api.get('/rpm/constraints/GetConstraints');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const DeleteConstraint = async (id: number) => {
	try {
		const response = await Api.post('/rpm/constraints/DeleteConstraint', { id });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetDomainConstraint = async (id: number) => {
	try {
		const response = await Api.post('/rpm/constraints/GetDomainConstraint', { id });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetRangeConstraint = async (id: number) => {
	try {
		const response = await Api.post('/rpm/constraints/GetRangeConstraint', { id });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetPatternConstraint = async (id: number) => {
	try {
		const response = await Api.post('/rpm/constraints/GetPatternConstraint', { id });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const EditConstraint = async (constraint: ConstraintListItem) => {
	try {
		const response = await Api.post('/rpm/constraints/EditConstraint', constraint);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const EditDomainConstraint = async (constraint: DomainConstraintListItem) => {
	try {
		const response = await Api.post('/rpm/constraints/EditDomainConstraint', constraint);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const EditRangeConstraint = async (constraint: RangeConstraintListItem) => {
	try {
		const response = await Api.post('/rpm/constraints/EditRangeConstraint', constraint);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const EditPatternConstraint = async (constraint: PatternConstraintListItem) => {
	try {
		const response = await Api.post('/rpm/constraints/EditPatternConstraint', constraint);
		console.log('PatternConstraint', response.data);
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

export const GetDatasetsByConstraint = async (id: number) => {
	try {
		const response = await Api.post('/rpm/constraints/GetDatasetsByConstraint', { id });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetMeaningsByConstraint = async (id: number) => {
	try {
		const response = await Api.post('/rpm/constraints/GetMeaningsByConstraint', { id });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetStruturedDatasetsByUserPermission = async () => {
	try {
		const response = await Api.get('/rpm/constraints/GetStruturedDatasetsByUserPermission');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetDatastructure = async (id: number) => {
	try {
		const response = await Api.post('/rpm/constraints/GetDatastructure', { id });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetData = async (id: number, pagesize: number = 10, variableid: number = 0) => {
	try {
		const response = await Api.post('/rpm/constraints/GetData', { id, pagesize, variableid });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const GetProvider = async () => {
	try {
		const response = await Api.get('/rpm/constraints/GetProvider');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};
