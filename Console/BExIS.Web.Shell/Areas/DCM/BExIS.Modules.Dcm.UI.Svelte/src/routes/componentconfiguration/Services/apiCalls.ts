import { Api } from '@bexis2/bexis2-core-ui';

export const GetMetadataSchema = async (id: number) => {
    try {
        // // API
        // const response = await Api.get('/api/MetadataStructure/' + id);
        // return response.data;
        
        // local load
        const response = await fetch(`/src/routes/componentconfiguration/Schema/metadataSchema_${id}.json`);
        if (!response.ok) {
            throw new Error(`Schema file not found: metadataSchema_${id}.json`);
        }
        const data = await response.json();
        return data;
    } catch (error) {
        throw error;
    }
};