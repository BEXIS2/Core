// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import {Api} from "@bexis2/bexis2-core-ui";

/****************/ 
/* Create*/
/****************/ 
export const deleteStructure = async (id, structureId) => {
 try {
  

   const response = await Api.delete('/dcm/datadescription/delete',"{id,structureId}");
   return response.data;
 } catch (error) {
   console.error(error);
 }
};
