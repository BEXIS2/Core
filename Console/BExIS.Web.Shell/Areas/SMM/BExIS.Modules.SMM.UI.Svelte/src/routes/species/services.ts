// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import { Api } from '@bexis2/bexis2-core-ui';

/****************/
/* Create*/
/****************/
export const load = async () => {
    try {
        const response = await Api.get('/smm/species/load');
        return response.data;
    } catch (error) {
        console.error(error);
    }
};

