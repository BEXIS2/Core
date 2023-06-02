import { getView }  from '../../services/ViewCaller'
import { setApiConfig }  from '@bexis2/bexis2-core-ui'

/** @type {import('./$types').PageLoad} */

export const load = async () =>{

 setApiConfig("https://localhost:44345","davidschoene","123456");

	console.log("id",window.view.id);

 let id = 6;//window.view.id; 

 return {
  model: await getView(id),
 }
}
