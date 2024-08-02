import { Api } from '@bexis2/bexis2-core-ui';

export const getVersions = async () => {
	try {
		const response = await Api.get('/api/versions');
		console.log('response:', response);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getVersion = async (name: string) => {
	try {
		const response = await Api.get(`/api/versions/${name}`);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};
