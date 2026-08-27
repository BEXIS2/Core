import { Api } from '@bexis2/bexis2-core-ui'; // get model for View page

export const getView = async (id, version, tag) => {
	try {
		const response = await Api.get('/dcm/view/load?id=' + id + '&version=' + version + '&tag=' + tag);
		console.log("🚀 ~ getView ~ response:", response)
		return response;
	} catch (error) {
		console.error(error);
		throw error;
	}
};

export const getApiDataset = async (id, version, tag ) => {
	try {
 
 let url =	'/api/dataset/'+id;
	if(version >= 0 && (tag === undefined || tag <= 0)){ //load	by version
		url = '/api/dataset/'+id+'/version_number/'+version
	}
	else if(tag >= 0){ //load by tag
		url = '/api/dataset/'+id+'/tag/'+tag
	}

	const response = await Api.get(url);
	return response.data;
		
	} catch (error) {
		console.error(error);
	}
};

export const getCitation = async (id, version, tag) => {
	try {
		const response = await Api.get('/dcm/view/citation?id=' + id+'&version=' + version+'&tag=' + tag);
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

export const getVersions = async (id) => {
	try {
		const response = await Api.get('/dcm/view/versions?id=' + id);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getDeleted = async (id) => {
	try {
		const response = await Api.get('/dcm/view/loaddeleted?id=' + id);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getCitationText = async (id, version, tag, format, useTags) => {
	try {
  
		let url =	'/api/datasets/'+id;
		
		if(tag >= 0 && (useTags)){ //load by tag
			url = '/api/datasets/'+id+'/tag/'+tag
		}else
			if(version >= 0){ //load	by version
			url = '/api/datasets/'+id+'/version_number/'+version
		}

		const response = await Api.get(`${url}/citations?format=${format}`);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getCitationOptions = async (id, version, tag ) => {
	try {
		const response = await Api.get(`/dcm/view/getcitationoptions?id=${id}&version=${version}&tag=${tag}`);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};

export const getDataDescription = async (id, version) => {
	try {
		const response = await Api.get(`/dcm/datadescription/Load?id=${id}&version=${version}`);
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
			
				return response;
		}
		else
		{

				const response = await Api.get('/dcm/view/downloadZip?id=' + id+'&version=' + version+'&format=' + format+'&withFilter=' + withFilter+'&withUnits=' + withUnits, '', header, config);
				return response;
		}
	} catch (error) {
		console.error(error);
	}
};

export const sendRequest = async (id, intention) => {
	try {
		const response = await Api.get('/ddm/RequestsSend/send?id=' + id + '&intention=' + intention);
		return response.data;
	} catch (error) {
		console.error(error);
	}
};



