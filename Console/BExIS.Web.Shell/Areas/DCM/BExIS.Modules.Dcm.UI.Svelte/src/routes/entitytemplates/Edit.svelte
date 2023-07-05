<script lang="ts">

 
import {createEventDispatcher} from 'svelte'
import { onMount } from 'svelte'; 
import { fade } from 'svelte/transition';

// ui Components
 import Fa from 'svelte-fa/src/fa.svelte'
 import { DropdownKVP, MultiSelect, TextArea,TextInput, Spinner } from "@bexis2/bexis2-core-ui";
 import { faSave, faTrash, faQuestion } from '@fortawesome/free-solid-svg-icons/index'
 import {SlideToggle} from '@skeletonlabs/skeleton'
 import ContentContainer from '../../lib/components/ContentContainer.svelte'
 import { Modal, modalStore } from '@skeletonlabs/skeleton';

 import EntryContainer from './EntryContainer.svelte';



 // validation
 import suite from './edit'

 // services
 import { getEntityTemplate, saveEntityTemplate, getSystemKeys}  from '../../services/EntityTemplateCaller'

 // types
 import type { EntityTemplateModel } from '../../models/EntityTemplate'
 import type { listItemType } from '@bexis2/bexis2-core-ui'
 import type { ModalSettings,PopupSettings } from '@skeletonlabs/skeleton';
 

 // help
 import {editHelp} from './help'
 import { helpStore } from '@bexis2/bexis2-core-ui'
 import type { helpItemType, helpStoreType } from "@bexis2/bexis2-core-ui"

 //Set list of help items and clear selection
 helpStore.setHelpItemList(editHelp);


  export let id = 0;
  
  export let hooks= [];
  export let metadataStructures:listItemType[]= [];
  export let dataStructures=[];
  let systemKeys;
  export let entities=[];
  export let groups=[];
  export let filetypes=[];
 
  const dispatch = createEventDispatcher();
 
  let entityTemplate:EntityTemplateModel;

  $:entityTemplate;
  $:systemKeys;

  $:loaded = false

  suite.reset();
   onMount(async () => {
     console.log("start entity template", id);
     load();
     console.log("onmount");
     
   })
  
   async function load()
   {
     entityTemplate = await getEntityTemplate(id);
     console.log("load entity", entityTemplate);
     updateSystemKeys("metadataStructure");

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
   async function onChangeHandler(e)
   {
     //console.log("input changed", e)
     // add some delay so the entityTemplate is updated 
     // otherwise the values are old
     setTimeout(async () => {
       res = suite(entityTemplate, e.target.id)
      },10)

      // reload systemkeys if metdatatstructure has chagened
      console.log(e.target.id);

      // if metadata structure selection changed, 
      updateSystemKeys(e.target.id);
   }

   async function updateSystemKeys(targetid)
   {
    
      if(targetid === "metadataStructure")
      {
        let id = 0;
        if(entityTemplate.metadataStructure!= undefined)
        {
          id = entityTemplate.metadataStructure.id;
        }

        systemKeys = await getSystemKeys(id);
        console.log(systemKeys);
      }


   }

   function onCancel()
   {
    let x = "create"
     if(id > 0){x = "edit"}

    const modal: ModalSettings = {
      type: 'confirm',
      title: 'Cancel '+x+' entity template',
      body: 'Are you sure you wish to cancel the current form?',
      // TRUE if confirm pressed, FALSE if cancel pressed
      response: (r: boolean) => {if(r === true){
        dispatch("cancel")
      }},
    };

    modalStore.trigger(modal);

   }


</script>


{#if entityTemplate}
<ContentContainer>
 <form on:submit|preventDefault={handleSubmit}>

  <div class="w-full grid grid-cols-1 md:grid-cols-2 gap-4">

  <TextInput
   id="name"
   label="Name"
   bind:value={entityTemplate.name} 
   valid={res.isValid("name")} 
   invalid={res.hasErrors("name")}  
   feedback={res.getErrors("name")} 
   on:input={onChangeHandler}
   required={true}
   placeholder="Define a unique content-related name for your template."
   help={true}
   />

   <DropdownKVP 
    id="entityType" 
    title="Entity" 
    required = {true}
    source={entities} 
    bind:target={entityTemplate.entityType} 
    valid={res.isValid("entityType")}
    invalid={res.hasErrors("entityType")}  
    feedback={res.getErrors("entityType")} 
    on:change={onChangeHandler} 
    complexTarget = {true}
    help={true}
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
    placeholder="Briefly describe in which cases this template should be used. Based on entity or usecase."
    help={true}
    />
  
  </div>

  
   
   <div class="py-5 w-full grid grid-cols-1 md:grid-cols-2 gap-4">

    <div class="flex flex-col space-y-4">
     <h3 class="h3">Metadata</h3>

     <DropdownKVP 
      id="metadataStructure"
      title="Structure" 
      bind:target={entityTemplate.metadataStructure}
      source={metadataStructures} 
      valid={res.isValid("metadataStructure")}
      invalid={res.hasErrors("metadataStructure")}  
      feedback={res.getErrors("metadataStructure")} 
      on:change={onChangeHandler} 
      complexTarget={true}
      required={true}
      help={true}
      />

      {#if systemKeys}
        <EntryContainer>
          <MultiSelect 
          id="metadataFields"
          title="Required input fields" 
          source={systemKeys}
          bind:target={entityTemplate.metadataFields}
          itemId="id"
          itemLabel="text"
          itemGroup="group"
          complexSource={true}
          help={true}
          />

        </EntryContainer>
      {/if}

      <EntryContainer>
        <div id="invalidSaveMode"></div>
        <SlideToggle name="Invalid-save-mode" bind:value={entityTemplate.metadataInvalidSaveMode} >
          Invalid save mode
        </SlideToggle>

      </EntryContainer>
      

    </div>
    <div class="flex flex-col space-y-4">
      <h3 class="h3">Datastructure</h3>
      <EntryContainer>
        <div class="mt-7 space-y-5" on:mouseover={()=>helpStore.show("hasDatastructure")}>
        <SlideToggle name="Use datastructures?" bind:checked={entityTemplate.hasDatastructure} >
          Use datastructures?
        </SlideToggle>

        {#if entityTemplate.hasDatastructure}
        <MultiSelect  
          id="datastructures"
          title="Limit the selection of datastrutcures" 
          source={dataStructures} 
          bind:target={entityTemplate.datastructureList}
          itemId="key"
          itemLabel="value"
          complexSource={true}
          help={true}
          />
        {/if}
       </div>
      
      </EntryContainer>


    </div>
  

   </div>

   <h3 class="h3">Group</h3>
   <div class="py-5 w-full grid grid-cols-1 md:grid-cols-2 gap-4">

      <EntryContainer>
        <MultiSelect  
          id="permissions"
          title="Permission" 
          source={groups} 
          bind:target={entityTemplate.permissionGroups}
          itemId="key"
          itemLabel="value"
          complexSource={true}
          help={true}
          />
      </EntryContainer>
      
 
      <EntryContainer>
        <MultiSelect  
          id="notification"
          title="Notification" 
          source={groups} 
          bind:target={entityTemplate.notificationGroups} 
          itemId="key"
          itemLabel="value"
          complexSource={true}
          help={true}
          />

      </EntryContainer>
  
     
    
   </div>

   <h3 class="h3">Additional</h3>
   <div class="py-5 w-full grid xs:grid-cols-1 md:grid-cols-2 gap-4">
    <EntryContainer>
      <MultiSelect 
        id="disabledHooks"
        title="Disabled hooks" 
        source={hooks} 
        bind:target={entityTemplate.disabledHooks} 
        help={true}
      />

    </EntryContainer>

    <EntryContainer>
      <MultiSelect 
        id="allowedFileTypes"
        title="Allowed file types" 
        source={filetypes} 
        bind:target={entityTemplate.allowedFileTypes}
        help={true} 
      /> 
 
    </EntryContainer>

  </div>

  <div class="py-5">
   <button type="submit" class="btn variant-filled-surface" {disabled}><Fa icon={faSave}/></button>
   <button type="button" class="btn variant-filled-warning" on:click={onCancel}><Fa icon={faTrash}/></button>
  </div>
 </form>
</ContentContainer> 
{:else}
  <Spinner textCss="text-secondary-500"/>
{/if}


<Modal/>