import {Api} from "@bexis2/bexis2-core-ui/src/lib/index";// get model for View page

export const getView = async (id) => {
 try {
   const response = await Api.get('/dcm/view/load?id='+id);
   return response.data;
 } catch (error) {
   console.error(error);
 }
};
