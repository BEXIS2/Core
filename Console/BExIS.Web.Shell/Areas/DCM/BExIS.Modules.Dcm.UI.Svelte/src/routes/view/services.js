import { Api } from '@bexis2/bexis2-core-ui'; // get model for View page

export const getView = async (id) => {
	try {
		const response = await Api.get('/dcm/view/load?id=' + id);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getApiDataset = async (id, version ) => {
	try {
  if(version === undefined || version <= 0){
			const response = await Api.get('/api/dataset/'+id);
			return response.data;
		}
		else{
			const response = await Api.get('/api/dataset/'+id+'/version_number/'+version);
			return response.data;
		}

		
	} catch (error) {
		console.error(error);
	}
};

export const getCitation = async (id, version) => {
	try {
		const response = await Api.get('/dcm/view/citation?id=' + id+'&version=' + version);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getTags = async (id, version) => {
	try {
		const response = await Api.get('/dcm/view/tags?id=' + id+'&version=' + version);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getVersions = async (id, version) => {
	try {
		const response = await Api.get('/dcm/view/versions?id=' + id);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const downloadZip = async (id, format, version = -1, withFilter = false, withUnits = false) => {
	try {
			const config = { responseType: 'blob' }
				const header = { 'Accept': 'application/json' }

		if(format	=== undefined || format === null || format === ''){
				const response = await Api.get('/dcm/view/downloadZip?id=' + id+'&version=' + version,'',header,config);
				return response.data;
		}
		else
		{

				const response = await Api.get('/dcm/view/downloadZip?id=' + id+'&version=' + version+'&format=' + format+'&withFilter=' + withFilter+'&withUnits=' + withUnits, '', header, config);
				return response.data;
		}
	} catch (error) {
		console.error(error);
	}
};