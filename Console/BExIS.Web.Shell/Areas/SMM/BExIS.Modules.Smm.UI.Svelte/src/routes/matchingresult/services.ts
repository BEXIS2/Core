import { Api } from '@bexis2/bexis2-core-ui';
import type { ServiceResult } from '$lib/types/types';

export const loadMatchingResult = async (datasetId: number, stepId: number): Promise<ServiceResult<any>> => {
    try {
        const response = await Api.get(`/smm/species/ViewMatchingResult?datasetId=${datasetId}&stepId=${stepId}`);

        return { success: true, data: response.data };
    } catch (error: any) {
        return { success: false, error: error.data?.message };
    }
};