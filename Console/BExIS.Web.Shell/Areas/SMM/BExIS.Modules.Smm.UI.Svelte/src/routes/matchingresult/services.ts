import { Api } from '@bexis2/bexis2-core-ui';
import type { AcceptMatchesRequest, ServiceResult } from '$lib/types/types';

export const loadMatchingResult = async (datasetId: number, versionId: number, stepId: number): Promise<ServiceResult<any>> => {
    try {
        const response = await Api.get(`/smm/species/ViewMatchingResult?datasetId=${datasetId}&versionId=${versionId}&stepId=${stepId}`);

        return { success: true, data: response.data };
    } catch (error: any) {
        return { success: false, error: error.data?.message };
    }
};

export const submitAcceptedIds = async (payload: AcceptMatchesRequest): Promise<ServiceResult<any>> => {
    try {
        const response = await Api.post('/smm/species/AcceptMatches', payload);

        return { success: true, data: response.data }
    } catch (error: any) {
        console.error(error);
        return { success: false, error: error.data?.message };
    }
}

export const loadMatchingFileStatus = async (datasetId: number, versionId: number, stepId: number): Promise<ServiceResult<any>> => {
    try {
        const response = await Api.get(`/smm/species/GetMatchingFileStatus?datasetId=${datasetId}&versionId=${versionId}&stepId=${stepId}`);

        return { success: true, data: response.data };
    } catch (error: any) {
        return { success: false, error: error.data?.message };
    }
}

export const requestResultFileDownload = async (datasetId: number, versionId: number, stepId: number): Promise<ServiceResult<any>> => {
    try {
        const response = await Api.get(`/smm/species/StartDownloadResultFile?datasetId=${datasetId}&versionId=${versionId}&stepId=${stepId}`);

        return { success: true, data: response.data };
    } catch (error: any) {
        return { success: false, error: error.data?.message };
    }
}