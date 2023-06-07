import { Api } from "@bexis2/bexis2-core-ui";

export const getUsers = async () => {
    try {
        const response = await Api.get('/api/users');
        console.log("response:", response);
        return response.data;
    } catch (error) {
        console.error(error);
    }
};

export const getUserById = async (id:number) => {
    try {
        const response = await Api.get(`/api/users/${id}`);
        return response.data;
    } catch (error) {
        console.error(error);
    }
};