import { Api } from '@bexis2/bexis2-core-ui';

export const loadResult = async (datasetId: number, versionId: number) => {
    try {
        const response = await Api.get(`http://localhost:44345/smm/species/ViewTailored?datasetId=${datasetId}&versionId=${versionId}`);
        return response.data;
    } catch (error) {
        console.error(error);
    }
}