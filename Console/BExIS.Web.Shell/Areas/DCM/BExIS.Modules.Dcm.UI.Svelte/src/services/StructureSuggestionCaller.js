// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import {Api} from "@bexis2/svelte-bexis2-core-ui";

/****************/ 
/* Create*/
/****************/ 
export const getStructureSuggestion = async (id, file, version) => {
 try {
   const response = await Api.get('/dcm/StructureSuggestion/load?id='+ id +'&&file='+file+'&&version='+version );
   return response.data;
 } catch (error) {
   console.error(error);
 }
};

export const getDelimeters = async (id, file, version) => {
  try {
    const response = await Api.get('/dcm/StructureSuggestion/GetDelimters' );
    return response.data;
  } catch (error) {
    console.error(error);
  }
};
 
export const generate = async (data) => {
  try {
    const response = await Api.post('/dcm/StructureSuggestion/generate',data);
    return response.data;
  } catch (error) {
    console.error(error);
  }
};

export const getDataTypes = async () => {
  try {
    const response = await Api.get('/dcm/StructureSuggestion/getDataTypes');
    return response.data;
  } catch (error) {
    console.error(error);
  }
};

export const getUnits = async () => {
  try {
    const response = await Api.get('/dcm/StructureSuggestion/getUnits');
    return response.data;
  } catch (error) {
    console.error(error);
  }
};