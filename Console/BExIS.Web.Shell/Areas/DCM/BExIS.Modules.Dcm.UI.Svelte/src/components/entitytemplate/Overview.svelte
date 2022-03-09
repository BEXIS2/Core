<script>
 
 import { onMount } from 'svelte'; 
 import {Spinner} from 'sveltestrap';
 
 import { setApiConfig }  from '@bexis2/svelte-bexis2-core-ui'
 import { getEntityTemplateList }  from '../../services/Caller'
 
 
 $:entitytemplates= [];

 onMount(async () => {
   console.log("start entity template");
   setApiConfig("https://localhost:44345","davidschoene","123456");
   load();
 })
 
 async function load()
 {
  entitytemplates = await getEntityTemplateList();
  console.log("entitytemplates", entitytemplates)
 }
 
 </script>
 
 <h1>Overview</h1>