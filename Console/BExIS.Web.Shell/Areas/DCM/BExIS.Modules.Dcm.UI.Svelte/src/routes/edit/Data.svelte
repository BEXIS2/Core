<script lang="ts">

 import Hook from '../../lib/components/Hook.svelte'

 import HookContainer from '../../lib/components/HookContainer.svelte';
 import Validation from '../../pointer/hooks/Validation.svelte'
 import FileUpload from '../../pointer/hooks/FileUpload.svelte'
 import DataDescription from '../../pointer/hooks/DataDescription.svelte'
 import Metadata from '../../pointer/hooks/Metadata.svelte'
 import {Spinner, Row, Col} from 'sveltestrap';
 import Fa from 'svelte-fa/src/fa.svelte'
 import { faAngleRight } from '@fortawesome/free-solid-svg-icons'

// models
import type { HookModel} from './types'


 export let id;
 export let version;
 export let hooks:HookModel[];
 
 let metadataHook;
 let dataDescriptionHook;
 let fileUploadHook;
 let validationHook;
 let submitHook;

 $:hooks, setHooks(hooks);

 function setHooks(_hooks)
 {
   _hooks.forEach(h => {
    
    if(h.name == "metadata"){ metadataHook = h; }
    if(h.name == "fileupload"){ fileUploadHook = h; }
    if(h.name == "validation"){ validationHook = h; }
    if(h.name == "datadescription"){ dataDescriptionHook = h; }
    if(h.name == "submit"){ submitHook = h; }

  });

  //console.log("_hooks",_hooks);
  
}

function errorHandler(e)
{
  console.log("error event in data");
}


</script>

{#if hooks} <!-- if hooks list is loaded render hooks -->

<HookContainer  {...metadataHook} >
  <div slot="view">
    <Metadata {id} {version} {...metadataHook} />
  </div>
</HookContainer>

<Row>

<Col ><!-- Data Hooks-->

    <HookContainer {...dataDescriptionHook} >
      <div slot="view">
        <DataDescription {id} {version} hook={dataDescriptionHook} />
      </div>
    </HookContainer>
    
    <HookContainer 
      {...fileUploadHook}
      let:errorHandler 
      let:successHandler >
      
      <div slot="view">
        <FileUpload {id} {version} hook={fileUploadHook}  on:error={(e)=> errorHandler(e)} on:success={(e)=> successHandler(e)} />
      </div>
    </HookContainer>

    <HookContainer {...validationHook} >
      <div slot="view"> <!-- validation Hooks-->
          <Validation {id} {version} {...validationHook} />
      </div>
    </HookContainer>
  </Col>

</Row>
 
 {:else} <!-- while data is not loaded show a loading information -->
    <Spinner color="primary" size="sm" type ="grow" />
 {/if}

 <style>
  .title-container
  {
    padding: 1rem 2rem 1rem 1rem;
  }

  .left-container
  {
    padding: 1rem;
    margin: 1rem;
    border: 1px solid var(--bg-grey);
    color: var(--text-color);
    height: calc(100% - 2rem);
  }

 </style>