import { Api } from '@bexis2/bexis2-core-ui';
import type { ServiceResult } from '$lib/types/types';

export const loadDatasetProgress = async (datasetId: number, versionId: number): Promise<ServiceResult<any>> => {
    try {
        const response = await Api.get(`/smm/species/ViewProgress?datasetId=${datasetId}&versionId=${versionId}`);

        return { success: true, data: response.data };
    } catch (error: any) {
        return { success: false, error: error.data?.message };
    }
};

export const tailorDataset = async (datasetId: number, versionId: number): Promise<ServiceResult<any>> => {
    try {
        const response = await Api.post('/smm/species/Tailor', { datasetId, versionId });

        return { success: true, data: response.data };
    } catch (error: any) {
        console.log(error);
        return { success: false, error: error.data?.message };
    }
}

export const genNewMatchFile = async (datasetId: number, versionId: number): Promise<ServiceResult<any>> => {
    try {
        const response = await Api.post('/smm/species/GenNewMatchInputFile', { datasetId, versionId });

        return { success: true, data: response.data };
    } catch (error: any) {
        return { success: false, error: error.data?.message };
    }
}

export const matchNextFile = async (datasetId: number, versionId: number): Promise<ServiceResult<any>> => {
    try {
        const response = await Api.post('/smm/species/MatchNextFile', { datasetId, versionId });

        return { success: true, data: response.data };
    } catch (error: any) {
        return { success: false, error: error.data?.message };
    }
}