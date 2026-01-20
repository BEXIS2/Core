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
