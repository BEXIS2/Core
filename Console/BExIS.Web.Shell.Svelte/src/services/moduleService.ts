import { Api } from "@bexis2/bexis2-core-ui";

export const get = async () => {
    try {
        const response = await Api.get('/api/settings');
        return response;
    } catch (error) {
        console.error(error);
    }
};

export const put = async (model:any) => {
    try {
        const response = await Api.put('/api/settings', model);
        return response;
    } catch (error) {
        console.error(error);
    }
};

export const getByModuleId = async (id:string) => {
    try {
        const response = await Api.get(`/api/settings/${id}`);
        return response;
    } catch (error) {
        console.error(error);
    }
};

export const putByModuleId = async (id:string, model:any) => {
    try {
        const response = await Api.put(`/api/settings/${id}`, model);
        return response;
    } catch (error) {
        console.error(error);
    }
};