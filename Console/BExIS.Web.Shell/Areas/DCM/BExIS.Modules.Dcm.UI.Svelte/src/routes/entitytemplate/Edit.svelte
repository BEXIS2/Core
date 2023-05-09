<script lang="ts">

 
import {createEventDispatcher} from 'svelte'
import { onMount } from 'svelte'; 

// ui Components
 import Fa from 'svelte-fa/src/fa.svelte'
 import CheckBoxKvPList  from '../../lib/components/form/CheckBoxKvPList.svelte'
 import DropdownKvP  from '../../lib/components/form/DropdownKvP.svelte'
 import MultiSelect  from '../../lib/components/form/MultiSelect.svelte'
 import Input from '../../lib/components/form/Input.svelte'
 import TextArea from '../../lib/components/form/TextArea.svelte'
 import { Spinner }  from '@bexis2/bexis2-core-ui'
 import { faSave, faTrashAlt } from '@fortawesome/free-regular-svg-icons/index'
 import {SlideToggle} from '@skeletonlabs/skeleton'
 
 // validation
 import suite from './edit'

 // services
 import { setApiConfig }  from '@bexis2/bexis2-core-ui'
 import { getEntityTemplate, saveEntityTemplate }  from '../../services/EntityTemplateCaller'

 // types
 import type { EntityTemplateModel } from '../../models/EntityTemplate'
 
  export let id = 0;
  
  export let hooks= [];
  export let metadataStructures= [];
  export let dataStructures=[];
  export let systemKeys=[];
  export let entities=[];
  export let groups=[];
  export let filetypes=[];
 
  const dispatch = createEventDispatcher();
 
  let et:EntityTemplateModel;

  $:entityTemplate = et; 
 
 
   onMount(async () => {
     console.log("start entity template", id);
     setApiConfig("https://localhost:44345","davidschoene","123456");
     load();
     suite.reset();
   })
  
   async function load()
   {
     entityTemplate = await getEntityTemplate(id);
     console.log("load entity", entityTemplate);
 
     // if id > 0 then run validation
     if(id>0)
     {
       res = suite(entityTemplate);
     }
 
   }
 
   async function handleSubmit() {
     console.log("before submit", entityTemplate);
     const res = await saveEntityTemplate(entityTemplate);
     if(res!=false)
     {
       console.log("save", res);
       dispatch("save", res);
     }
   }
 
   // validation
   let res = suite.get();
   // flag to enable submit button
   $:disabled = !res.isValid();
 
   //change event: if input change check also validation only on the field
   // e.target.id is the id of the input component
   function onChangeHandler(e)
   {
    console.log("input changed", e)
     // add some delay so the entityTemplate is updated 
     // otherwise the values are old
     setTimeout(async () => {
       res = suite(entityTemplate, e.target.id)
    },10)
   }
 
 $:test = false;

  </script>


{#if entityTemplate}
<div>
 <form on:submit|preventDefault={handleSubmit}>

  <div class="w-full grid grid-cols-1 md:grid-cols-2 gap-4">

  <Input
   id="name"
   label="Name"
   bind:value={entityTemplate.name} 
   valid={res.isValid("name")} 
   invalid={res.hasErrors("name")}  
   feedback={res.getErrors("name")} 
   on:input={onChangeHandler}
   required={true}
   />

   <DropdownKvP 
    id="entityType" 
    title="Entity" 
    source={entities} 
    bind:target={entityTemplate.entityType} 
    valid={res.isValid("entityType")}
    invalid={res.hasErrors("entityType")}  
    feedback={res.getErrors("entityType")} 
    on:change={onChangeHandler} 
   />

   <TextArea 
    id="description"
    label="Description" 
    bind:value="{entityTemplate.description}" 
    valid={res.isValid("description")} 
    invalid={res.hasErrors("description")}  
    feedback={res.getErrors("description")} 
    on:input={onChangeHandler} 
    required={true} 
    />
  
  </div>

  
   
   <div class="py-5 w-full grid grid-cols-1 md:grid-cols-2 gap-4">

    <div class="flex flex-col space-y-4">
     <h3>Metadata</h3>

     <DropdownKvP 
      id="metadataStructure"
      title="Structure" 
      bind:target={entityTemplate.metadataStructure}
      source={metadataStructures} 
      valid={res.isValid("metadataStructure")}
      invalid={res.hasErrors("metadataStructure")}  
      feedback={res.getErrors("metadataStructure")} 
      on:change={onChangeHandler} 
      />

      <MultiSelect 
      title="Required input fields" 
      source={systemKeys}
      bind:target={entityTemplate.metadataFields}
      itemId="key"
      label="value"
      isComplex={true}
      />

      <SlideToggle name="Invalid-save-mode" bind:value={entityTemplate.metadataInvalidSaveMode} >
        Invalid save mode
      </SlideToggle>

     </div>

     <div class="flex flex-col space-y-4">
       <h3>Datastructure</h3>
   
       <SlideToggle name="Use datastructures?" bind:checked={entityTemplate.hasDatastructure} >
        Use datastructures?
       </SlideToggle>
   
       {#if entityTemplate.hasDatastructure}
           <CheckBoxKvPList key="datastructures" title="Datastructures" source={dataStructures} bind:target={entityTemplate.datastructureList} />
       {/if}

       </div>
   </div>

   
   <h3>Group</h3>
   <div class="py-5 w-full grid grid-cols-1 md:grid-cols-2 gap-4">
    <MultiSelect  
      title="Permission" 
      source={groups} 
      bind:target={entityTemplate.permissionGroups}
      itemId="key"
      label="value"
      isComplex={true}
      />

      <MultiSelect  
      title="Notification" 
      source={groups} 
      bind:target={entityTemplate.notificationGroups} 
      itemId="key"
      label="value"
      isComplex={true}
      />
   </div>

   <h3>Additional</h3>
   <div class="py-5 w-full grid grid-cols-1 md:grid-cols-2 gap-4">
    <MultiSelect 
      title="Disabled hooks" 
      source={hooks} 
      bind:target={entityTemplate.disabledHooks} 
      />
      
      <MultiSelect 
      title="Allowed file types" 
      source={filetypes} 
      bind:target={entityTemplate.allowedFileTypes} 
      /> 
   </div>

  <div class="py-5">

   <button type="submit" class="btn bg-slate-500" {disabled}><Fa icon={faSave}/></button>
   <button type="button" class="btn bg-warning-500" on:click={()=> dispatch("cancel")}><Fa icon={faTrashAlt}/></button>
   
  </div>
 </form>
</div>
{:else}
<Spinner/>
{/if}

