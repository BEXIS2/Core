// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import {Api} from "@bexis2/bexis2-core-ui/src/lib/index";

/****************/ 
/* Create*/
/****************/ 
export const deleteStructure = async (id, structureId) => {
 try {
   const response = await Api.delete('/dcm/datadescription/delete',{id,structureId});
   return response.json();
 } catch (error) {
   console.error(error);
 }
};
