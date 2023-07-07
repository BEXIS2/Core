<script lang="ts">
 
 import { fade } from 'svelte/transition';

 import { positionType, ErrorMessage } from '@bexis2/bexis2-core-ui';

 // ui components
 import Fa from 'svelte-fa/src/fa.svelte'
 import { faPlus } from '@fortawesome/free-solid-svg-icons'
 import Overview from './Overview.svelte';
 import Edit from './Edit.svelte';
 
 // services
 import { Page, Spinner }  from '@bexis2/bexis2-core-ui'


 import { 
   getEntities,
   getDataStructures,
   getMetadataStructures,
   getGroups, 
   getHooks,
   getFileTypes,
   getEntityTemplateList
 }  from '../../services/EntityTemplateCaller'

 // types
 import type { EntityTemplateModel } from '../../models/EntityTemplate'


 // Store 
 import { entityTemplatesStore } from "./store"

 let hooks= [];
 let metadataStructures= [];
 let dataStructures=[];
 let entities=[];
 let groups=[];
 let filetypes=[];
 
 let isOpen = false;
 let header = "";
 
 $:selectedEntityTemplate = 0;

 let entitytemplates:EntityTemplateModel[];
 $:entitytemplates;
 

 async function load()
 { 
   hooks = await getHooks();
   console.log("hooks",hooks);
   metadataStructures = await getMetadataStructures();
   dataStructures = await getDataStructures();
   entities = await getEntities();
   groups = await getGroups();
   filetypes = await getFileTypes();
 
   entitytemplates =  await getEntityTemplateList();
   entityTemplatesStore.set(entitytemplates);

 }
 
 async function refresh(e:any)
 {
   const newEnityTemplate = e.detail;
   //console.log(newEnityTemplate);
   
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

<Page title="Entity Templates" note="On this page you can edit entity template." help={true} >


<svelte:fragment>

{#await load()}
  <div class="text-surface-800">
    <Spinner position={positionType.center} label="loading entity templates" />
  </div>
{:then result}

  {#if isOpen}

  <Edit id = {selectedEntityTemplate} 
    {hooks} 
    {metadataStructures} 
    {dataStructures} 
    {entities} 
    {groups} 
    {filetypes} 
    on:save={refresh} 
    on:cancel={()=>isOpen=false}/>

  {:else}
  <div class="w-screen">
    <button type="button" on:click={create} class="btn variant-filled bg-secondary-400 "><Fa icon={faPlus}/></button>
  </div>
  {/if}

  <Overview bind:entitytemplates={entitytemplates} on:edit={edit} />

{:catch error}
 
  <ErrorMessage {error}/>
 
{/await}


</svelte:fragment>
</Page>