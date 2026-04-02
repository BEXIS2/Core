import { Api } from '@bexis2/bexis2-core-ui';
import type { HeaderMappings } from '$lib/types/types';


/**
 * Submits selected column mappings to backend for storage. (used in later stages)
 * @param data HeaderMappings data including a MappingEntry[] with the selected mappings.
 * @returns response data
 */
export const submitHeaderMappings = async (data: HeaderMappings) => {
    try {
        const response = await Api.post('/smm/species/SubmitHeaderMappings', data);
        return response.data;
    } catch (error) {
        console.error(error);
    }
}

/**
 * Loads datastructure information for display.
 * @param datastructureId .. datastructure id loaded from store
 * @returns DataStructureEditModel
 */
export const loadDataStructure = async (datastructureId: number) => {
    try {
        const response = await Api.get(`/rpm/DataStructure/get?id=${datastructureId}`);
        return response.data;
    } catch (error) {
        console.error(error);
    }
}
