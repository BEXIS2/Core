import { Api } from '@bexis2/bexis2-core-ui';

export const loadBasicDatasetInfo = async () => {
    try {
        const response = await Api.get('/smm/species/GetMyDatasetsJson');
        return response.data;
    } catch (error) {
        console.error(error);
    }
};
