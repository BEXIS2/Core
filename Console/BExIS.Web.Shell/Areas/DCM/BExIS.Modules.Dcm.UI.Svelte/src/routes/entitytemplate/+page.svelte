<script lang="ts">
 
 import { onMount } from 'svelte'; 
 import { fade } from 'svelte/transition';

 // ui components
 import Fa from 'svelte-fa/src/fa.svelte'
 import { faPlus } from '@fortawesome/free-solid-svg-icons'
 import Overview from './Overview.svelte';
 import Edit from './Edit.svelte';
 
 // services
 import { setApiConfig, Page }  from '@bexis2/bexis2-core-ui'
 import { dev } from '$app/environment'

 import { 
   getEntities,
   getDataStructures,
   getMetadataStructures,
   getSystemKeys,
   getGroups, 
   getHooks,
   getFileTypes,
   getEntityTemplateList
 }  from '../../services/EntityTemplateCaller'

 // types
 import type { EntityTemplateModel } from '../../models/EntityTemplate'


 let hooks= [];
 let metadataStructures= [];
 let dataStructures=[];
 let systemKeys=[];
 let entities=[];
 let groups=[];
 let filetypes=[];
 
 let isOpen = false;
 let header = "";
 
 $:selectedEntityTemplate = 0;

 let entitytemplatesArray:EntityTemplateModel[];
 $:entitytemplates = entitytemplatesArray;
 
 onMount(async () => {
   console.log("start entity template");

   if (import.meta.env.DEV) {
			console.log('dev');
			setApiConfig('https://localhost:44345', 'davidschoene', '123456');
		}

 
   hooks = await getHooks();
   metadataStructures = await getMetadataStructures();
   dataStructures = await getDataStructures();
   systemKeys = await getSystemKeys();
   entities = await getEntities();
   groups = await getGroups();
   filetypes = await getFileTypes();
 
   entitytemplates = await getEntityTemplateList();
 

   console.log("hooks", hooks);
   console.log("metadataStructures", metadataStructures);
   console.log("dataStructures",dataStructures);
   console.log("systemKeys",systemKeys);
   console.log("entities",entities);
   console.log("groups",groups);
   console.log("filetypes",filetypes);
   console.log("entitytemplates", entitytemplates);
  
 })
 
 async function refresh(e:any)
 {
   const newEnityTemplate = e.detail;
   console.log(newEnityTemplate);
   
   //remove object from list & add to list again
   entitytemplates = entitytemplates.filter(e => {
       return e.id !== newEnityTemplate.id;
     });
   entitytemplates = [...entitytemplates, newEnityTemplate];
 
   // close the form to reset
   isOpen = false;
 }
 
 // open form as new
 async function create()
 {
    // set Form header 
    header="Create a new entity template";
 
   // set id
   selectedEntityTemplate = 0;
   isOpen = !isOpen;
 
 }
 
 // open form in edit with id in e.detail
 async function edit(e:any)
 {
   // set Form header 
   header="Edit a new entity template ("+e.detail+")";
 
   //remove form from dom
   isOpen = false;
  
   // reopen form with new object
   setTimeout(async () => {
     selectedEntityTemplate = e.detail;
     isOpen = true;
   },500)
 }
 
 const toggle = () => (isOpen = !isOpen);
 

</script>

<Page title="Entity Templates" note="On this page you can edit entity template.">

{#if isOpen}

<Edit id = {selectedEntityTemplate} 
   {hooks} 
   {metadataStructures} 
   {dataStructures} 
   {systemKeys} 
   {entities} 
   {groups} 
   {filetypes} 
   on:save={refresh} 
   on:cancel={()=>isOpen=false}/>

{:else}
<div class="w-full">
  <button type="button" on:click={create} class="btn variant-filled bg-secondary-400"><Fa icon={faPlus}/></button>
</div>
{/if}

<Overview bind:entitytemplates={entitytemplates} on:edit={edit} />

</Page>