import {Api} from "@bexis2/bexis2-core-ui";


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
    return response.data;
  } catch (error) {
    console.error(error);
  }
 };
 