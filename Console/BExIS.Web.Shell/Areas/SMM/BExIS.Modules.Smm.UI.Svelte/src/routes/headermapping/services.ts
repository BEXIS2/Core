import { Api } from '@bexis2/bexis2-core-ui';
import type { HeaderMappings } from '$lib/types/types';
import type { ServiceResult } from '$lib/types/types';

/**
 * Submits selected column mappings to backend for storage. (used in later stages)
 * @param data HeaderMappings data including a MappingEntry[] with the selected mappings.
 * @returns response data
 */
export const submitHeaderMappings = async (data: HeaderMappings, datasetId: number, versionId: number): Promise<ServiceResult<any>> => {
    try {
        const payload = {
            data: data,
            datasetId: datasetId,
            versionId: versionId
        }

        const response = await Api.post('/smm/species/SubmitHeaderMappings', payload);
        return { success: true, data: response.data };
    } catch (error: any) {
        console.error(error);
        return { success: false, error: error.data?.message };
    }
}

/**
 * Loads datastructure information for display.
 * @param datastructureId .. datastructure id loaded from store
 * @returns DataStructureEditModel
 */
export const loadDataStructure = async (datastructureId: number): Promise<ServiceResult<any>> => {
    try {
        const response = await Api.get(`/rpm/DataStructure/get?id=${datastructureId}`);
        return { success: true, data: response.data };
    } catch (error: any) {
        console.error(error);
        return { success: false, error: error.data?.message };
    }
}
