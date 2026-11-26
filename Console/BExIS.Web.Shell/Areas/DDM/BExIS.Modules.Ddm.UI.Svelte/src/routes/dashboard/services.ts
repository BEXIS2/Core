import { Api } from '@bexis2/bexis2-core-ui';

export interface MyDatasetModel {
	id: number;
	title: string;
	description: string;
	isOwn: boolean;
	isValid: string;
	type: string;
	hasTag: boolean;
	tagNr: number;
	hasData: boolean;
}

export interface RequestModel {
	id: number;
	instanceId: number;
	title: string;
	intention: string;
	rights: string;
	requestStatus: string;
	requestDate: string;
}

export interface DecisionModel {
	id: number;
	requestId: number;
	instanceId: number;
	title: string;
	applicant: string;
	intention: string;
	rights: string;
	status: number;
	statusAsText: string;
	requestDate: string;
}

export interface EntityModel {
	id: number;
	name: string;
}

export const getEntities = async (): Promise<EntityModel[]> => {
	try {
		const response = await Api.get('/ddm/Dashboard/GetEntities');
		return response.data;
	} catch (error) {
		console.error(error);
		return [];
	}
};

export const getMyDatasets = async (rightType: string = 'grant', entityName: string = 'Dataset'): Promise<MyDatasetModel[]> => {
	try {
		const response = await Api.get(`/ddm/Dashboard/GetMyDatasets?rightType=${rightType}&entityName=${entityName}`);
		return response.data;
	} catch (error) {
		console.error(error);
		return [];
	}
};

export const getMyRequests = async (): Promise<RequestModel[]> => {
	try {
		const response = await Api.get('/ddm/Dashboard/GetMyRequests');
		return response.data;
	} catch (error) {
		console.error(error);
		return [];
	}
};

export const getDecisions = async (): Promise<DecisionModel[]> => {
	try {
		const response = await Api.get('/ddm/Dashboard/GetDecisions');
		return response.data;
	} catch (error) {
		console.error(error);
		return [];
	}
};

export const getUseTags = async (): Promise<boolean> => {
	try {
		const response = await Api.get('/ddm/Dashboard/GetUseTags');
		return response.data;
	} catch (error) {
		console.error(error);
		return false;
	}
};

export const withdrawRequest = async (requestId: number): Promise<boolean> => {
	try {
		const response = await Api.post('/ddm/Dashboard/WithdrawRequest', { requestId });
		return response.data;
	} catch (error) {
		console.error(error);
		return false;
	}
};

export const acceptDecision = async (decisionId: number): Promise<boolean> => {
	try {
		const response = await Api.post('/ddm/Dashboard/AcceptDecision', { decisionId });
		return response.data;
	} catch (error) {
		console.error(error);
		return false;
	}
};

export const rejectDecision = async (requestId: number): Promise<boolean> => {
	try {
		const response = await Api.post('/ddm/Dashboard/RejectDecision', { requestId });
		return response.data;
	} catch (error) {
		console.error(error);
		return false;
	}
};
