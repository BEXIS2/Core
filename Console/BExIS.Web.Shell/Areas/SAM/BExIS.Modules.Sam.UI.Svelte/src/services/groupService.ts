import { Api } from "@bexis2/bexis2-core-ui";
import { ReadGroupModel } from "../models/groupModels";

export const getGroups = async () => {
    try {
        const response = await Api.get('/api/groups');
        let groups: ReadGroupModel[] = [];
        Array.from(response.data).forEach(item => {
            groups.push(new ReadGroupModel(item))
        });
        return (groups);
    } catch (error) {
        console.error(error);
    }
};

export const getGroupById = async (id: number) => {
    try {
        const response = await Api.get(`/api/groups/${id}`);
        return new ReadGroupModel(response.data);
    } catch (error) {
        console.error(error);
    }
};