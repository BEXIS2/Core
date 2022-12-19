import {Api} from "@bexis2/bexis2-core-ui/src/lib/index";

export const getEntityTemplate = async (id) => {
  try {
    const response = await Api.get('/dcm/entitytemplates/Get?id='+id);
    return response.json();
  } catch (error) {
    console.error(error);
  }
};

export const getEntityTemplateList = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/Load');
    return response.json();
  } catch (error) {
    console.error(error);
  }
};

export const getEntities = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/Entities');
    return response.json();
  } catch (error) {
    console.error(error);
  }
};

export const getMetadataStructures = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/MetadataStructures');
    return response.json();
  } catch (error) {
    console.error(error);
  }
};


export const getSystemKeys = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/SystemKeys');
    return response.json();
  } catch (error) {
    console.error(error);
  }
};

export const getDataStructures = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/DataStructures');
    return response.json();
  } catch (error) {
    console.error(error);
  }
};

export const getHooks = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/Hooks');
    return response.json();
  } catch (error) {
    console.error(error);
  }
};

export const getGroups = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/Groups');
    return response.json();
  } catch (error) {
    console.error(error);
  }
};

export const getFileTypes = async () => {
  try {
    const response = await Api.get('/dcm/entitytemplates/FileTypes');
    return response.json();
  } catch (error) {
    console.error(error);
  }
};


export const saveEntityTemplate = async (entityTemplate) => {
  try {
    const response = await Api.post('/dcm/entitytemplates/Update', entityTemplate);
    return response.json();
  } catch (error) {
    console.error(error);
  }
};

export const deleteEntityTemplate = async (id) => {
  try {
    const response = await Api.post('/dcm/entitytemplates/Delete?id='+id);
    return response.json();
  } catch (error) {
    console.error(error);
  }
};

