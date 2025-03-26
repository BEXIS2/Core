import { Api } from '@bexis2/bexis2-core-ui';
import type {Dataset} from '../models.ts';

export const getEntityTemplateList = async () => {
    try {
        const response = await Api.get('/dcm/entitytemplates/Load');
        console.log("response", response.data)
        return response.data;
    } catch (error) {
        console.error(error);
        throw error;
    }
};

export const createDataset = async (dataset: Dataset) => {
    try {
        const response = await Api.post('/api/Dataset', dataset);
        console.log("Dataset created:", response.data);
        return response.data;
    } catch (error) {
        console.error("Fehler beim Erstellen des Datasets:", error);
        throw error;
    }
};


// export const GetMetadataScheema = async (id: number) => {
// 	try {
// 		const response = await Api.get('/api/MetadataStructure/' + id);
// 		return response.data;
// 	} catch (error) {
// 		console.error(error);
// 		throw error;
// 	}
// };
