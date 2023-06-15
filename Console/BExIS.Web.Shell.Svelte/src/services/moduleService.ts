import { Api } from "@bexis2/bexis2-core-ui";

export const getModules = async () => {
    try {
        const response = await Api.get('/api/modules');
        console.log("response:", response);
        return response.data;
    } catch (error) {
        console.error(error);
    }
};

export const getModuleByName = async (name:string) => {
    try {
        const response = await Api.get(`/api/modules/${name}`);
        return response.data;
    } catch (error) {
        console.error(error);
    }
};