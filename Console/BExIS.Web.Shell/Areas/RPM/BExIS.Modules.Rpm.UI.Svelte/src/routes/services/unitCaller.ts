import {Api} from "@bexis2/bexis2-core-ui";

import type { UnitListItem } from "../../models/Models";

export const getUnitListItems = async () => {
  try 
  {
    const response =  await Api.get('/rpm/Unit/Get');
    return response.data;
  }
  catch (error) 
  {
    console.error(error);
  }
};

