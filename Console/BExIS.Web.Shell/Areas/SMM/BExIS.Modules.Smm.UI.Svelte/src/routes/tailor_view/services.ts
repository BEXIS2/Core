import { Api } from '@bexis2/bexis2-core-ui';
import type { TailorEdit } from './types';
import type { ServiceResult } from '$lib/types/types';

export const loadResult = async (datasetId: number, versionId: number) => {
    try {
        const response = await Api.get(`http://localhost:44345/smm/species/ViewTailored?datasetId=${datasetId}&versionId=${versionId}`);
        return response.data;
    } catch (error) {
        console.error(error);
    }
}

export const submitTailorEdits = async (datasetId: number, versionId: number, payload: TailorEdit[]): Promise<ServiceResult<any>> => {
    try {
        console.log("Applying tailor Edits...");
        console.log("Payload: \n", payload);
        const response = await Api.post(`/smm/species/ApplyTailorEdits?datasetId=${datasetId}&versionId=${versionId}`, payload);

        return { success: true, data: response.data }
    } catch (error: any) {
        console.error(error);
        return { success: false, error: error.data?.message };
    }

}