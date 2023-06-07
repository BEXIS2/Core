import {Api} from "@bexis2/bexis2-core-ui";

import type {UnitListItem} from "../models";

export const GetUnits = async () => {
  try 
  {
    
    const response =  await Api.get('/rpm/Unit/GetUnits');
    // console.log('get units', response.data);
    return response.data;
  }
  catch (error) 
  {
    console.error(error);
  }
};

export const GetMeasurementSystems = async () => {
  try 
  {
    const response =  await Api.get('/rpm/Unit/GetMeasurementSystems');
    return response.data;
  }
  catch (error) 
  {
    console.error(error);
  }
};

export const GetDataTypes = async () => {
  try 
  {
    const response =  await Api.get('/rpm/Unit/GetDataTypes');
    return response.data;
  }
  catch (error) 
  {
    console.error(error);
  }
};

export const EditUnit = async (unitListItem:UnitListItem) => {
  try 
  {
    const response =  await Api.post('/rpm/Unit/EditUnit',unitListItem);
    return response.data;
  }
  catch (error) 
  {
    console.error(error);
  }
};

