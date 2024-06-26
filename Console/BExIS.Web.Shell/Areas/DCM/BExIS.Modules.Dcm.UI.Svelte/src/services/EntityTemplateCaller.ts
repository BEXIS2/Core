import { Api } from '@bexis2/bexis2-core-ui';
import type { EntityTemplateModel } from '../models/EntityTemplate';

export const getEntityTemplate = async (id: number) => {
	try {
		const response = await Api.get('/dcm/entitytemplates/Get?id=' + id);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getEntityTemplateList = async () => {
	try {
		const response = await Api.get('/dcm/entitytemplates/Load');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getEntities = async () => {
	try {
		const response = await Api.get('/dcm/entitytemplates/Entities');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getMetadataStructures = async () => {
	try {
		const response = await Api.get('/dcm/entitytemplates/MetadataStructures');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getSystemKeys = async (id) => {
	try {
		const response = await Api.get('/dcm/entitytemplates/SystemKeys?metadataStructureId=' + id);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getDataStructures = async () => {
	try {
		const response = await Api.get('/dcm/entitytemplates/DataStructures');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getHooks = async () => {
	try {
		const response = await Api.get('/dcm/entitytemplates/Hooks');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getGroups = async () => {
	try {
		const response = await Api.get('/dcm/entitytemplates/Groups');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getFileTypes = async () => {
	try {
		const response = await Api.get('/dcm/entitytemplates/FileTypes');
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const saveEntityTemplate = async (entityTemplate: EntityTemplateModel) => {
	try {
		const response = await Api.post('/dcm/entitytemplates/Update', entityTemplate);
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const deleteEntityTemplate = async (id: bigint) => {
	try {
		const response = await Api.delete('/dcm/entitytemplates/Delete', { id });
		return response.data;
	} catch (error) {
		console.error(error);
		throw error;
	}
};
