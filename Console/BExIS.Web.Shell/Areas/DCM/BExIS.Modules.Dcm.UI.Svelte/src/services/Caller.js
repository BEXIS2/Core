// Implementations for all the calls for the pokemon endpoints.
//import Api from "./Api";
import {Api} from "@bexis2/svelte-bexis2-core-ui";

// get Model for Edit page
export const getEdit = async (id) => {
    console.log("edit");
    try {
      const response = await Api.get('/dcm/edit/load?id='+id);
      console.log(response);

      return response;
      
    } catch (error) {
      console.error(error);
    }
};

// get model for View page
export const getView = async (id) => {
  try {
    const response = await Api.get('/dcm/view/load?id='+id);
    return response;
  } catch (error) {
    console.error(error);
  }
};

// get start page from hook
export const getHookStart = async (action, id, version) => {
  try {
    const url = action+'?id='+id+'&version='+version
    const response = await Api.get(url);
    return response;
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
    return response;
  } catch (error) {
    console.error(error);
  }
};

// save file description
export const saveFileDescription = async (action, id, file, description ) => {
  try {

    const response = await Api.post(action, { id, file, description });
    return response;
  } catch (error) {
    console.error(error);
  }
};

// remove file from server
export const removeFile = async (action, id, file ) => {
  try {
    console.log("remove")
    console.log(action)
    console.log(id)
    console.log(file)

    const response = await Api.post(action, { id, file });
    return response;
  } catch (error) {
    console.error(error);
  }

};


/*
* EntityTemplate
*/

export const getEntityTemplate = async (id) => {
  try {
    const response = await Api.get('/dcm/entitytemplates/Get?id='+id);
    return response;
  } catch (error) {
    console.error(error);
  }
};

export const getEntityTemplateList = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/Load');
    return response;
  } catch (error) {
    console.error(error);
  }
};

export const getEntities = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/Entities');
    return response;
  } catch (error) {
    console.error(error);
  }
};

export const getMetadataStructures = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/MetadataStructures');
    return response;
  } catch (error) {
    console.error(error);
  }
};


export const getSystemKeys = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/SystemKeys');
    return response;
  } catch (error) {
    console.error(error);
  }
};

export const getDataStructures = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/DataStructures');
    return response;
  } catch (error) {
    console.error(error);
  }
};

export const getHooks = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/Hooks');
    return response;
  } catch (error) {
    console.error(error);
  }
};

export const getGroups = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/Groups');
    return response;
  } catch (error) {
    console.error(error);
  }
};

export const getFileTypes = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/FileTypes');
    return response;
  } catch (error) {
    console.error(error);
  }
};


export const saveEntityTemplate = async (entityTemplate) => {
  try {
    const response = await Api.post('/dcm/entitytemplates/Update', entityTemplate);
    return response;
  } catch (error) {
    console.error(error);
  }
};

export const deleteEntityTemplate = async (id) => {
  try {
    const response = await Api.post('/dcm/entitytemplates/Delete?id='+id);
    return response;
  } catch (error) {
    console.error(error);
  }
};

