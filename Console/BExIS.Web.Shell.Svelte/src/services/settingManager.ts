import { Api } from '@bexis2/bexis2-core-ui';
import { ReadSettingModel, UpdateSettingModel } from '../models/settingModels';

export const get = async () => {
	try {
		const response = await Api.get('/Settings/GetSettings');
		return response;
	} catch (error) {
		console.error(error);
	}
};

export const getByModuleId = async (id: string) => {
	try {
		const response = await Api.get(`/Settings/GetSettingsByModuleId?moduleId=${id}`);
		return response;
	} catch (error) {
		console.error(error);
	}
};

export const putByModuleId = async (id: string, model: UpdateSettingModel) => {
	try {
		const response = await Api.put(`/Settings/PutSettingsByModuleId?moduleId=${id}`, model);
		return response;
	} catch (error) {
		console.error(error);
	}
};
