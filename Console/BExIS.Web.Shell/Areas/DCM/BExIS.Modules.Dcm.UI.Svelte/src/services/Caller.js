// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import {Api} from "@bexis2/svelte-bexis2-core-ui";

// get Model for Edit page
export const getEdit = async (id) => {
    console.log("edit");
    try {
      const response = await Api.get('/dcm/edit/load?id='+id);
      return response.data;
      
    } catch (error) {
      console.error(error);
    }
};

// get model for View page
export const getView = async (id) => {
  try {
    const response = await Api.get('/dcm/view/load?id='+id);
    return response.data;
  } catch (error) {
    console.error(error);
  }
};

// get start page from hook
export const getHookStart = async (action, id, version) => {
  try {
    const url = action+'?id='+id+'&version='+version
    const response = await Api.get(url);
    return response.data;
  } catch (error) {
    console.error(error);
  }
};

export const getViewStart = async (action, id, version) => {
  try {
    const url = action+'?id='+id+'&version='+version
    const response = await Api.get(url);
    return response;
  } catch (error) {
    console.error(error);
  }
};

// load message from a dataset edit situation
export const loadMessages = async (id) => {
  try {

    console.log("test load messages")
    const response = await Api.get('/dcm/messages/load?id='+id);
    return response.data;
  } catch (error) {
    console.error(error);
  }
};

// save file description
export const saveFileDescription = async (id, file, description) => {
  try {

    const response = await Api.post('/dcm/fileupload/saveFileDescription', { id, file, description });
    return response.data;
  } catch (error) {
    console.error(error);
  }
};

// remove file from server
export const removeFile = async (id, file) => {
  try {

    const response = await Api.post('/dcm/fileupload/removefile', { id, file });
    return response;
  } catch (error) {
    console.error(error);
  }

};


