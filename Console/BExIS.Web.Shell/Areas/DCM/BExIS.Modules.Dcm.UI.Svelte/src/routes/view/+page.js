import { getView }  from '../../services/ViewCaller'
import { setApiConfig }  from '@bexis2/bexis2-core-ui'

/** @type {import('./$types').PageLoad} */

export const load = async () =>{

 setApiConfig("https://localhost:44345","davidschoene","123456");

 let id = 6;

 return {
  model: await getView(id),
 }
}
