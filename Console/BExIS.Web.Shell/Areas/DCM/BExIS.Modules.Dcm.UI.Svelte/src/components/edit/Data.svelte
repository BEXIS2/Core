<script>

 import Hook from '..//Hook.svelte'

 import HookContainer from '../HookContainer.svelte';
 import Validation from '../../pages/hooks/Validation.svelte'
 import FileUpload from '../../pages/hooks/FileUpload.svelte'
 import Metadata from '../../pages/hooks/Metadata.svelte'
 import {Spinner, Row, Col} from 'sveltestrap';
 import Fa from 'svelte-fa/src/fa.svelte'
 import { faAngleRight } from '@fortawesome/free-solid-svg-icons'

 export let id;
 export let version;
 export let hooks=null;
 
 let metadataHook;
 let dataDescriptionHook;
 let fileUploadHook;
 let validationHook;
 let submitHook;

 $:showValidation = false;



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
 }

function errorHandler(e)
{
  console.log("error event in data");

}

</script>

{#if hooks} <!-- if hooks list is loaded render hooks -->

<HookContainer displayName = {metadataHook.displayName} >
  <div slot="view">
    <Metadata {id} {version} {...metadataHook} />
  </div>
</HookContainer>

<Row>
<Col xs="1">
<div class="title-container">
  <b><Fa icon={faAngleRight} /> Data</b>
</div>
</Col> <!-- Data Title-->
<Col xs="10"><!-- Data Hooks-->
    <HookContainer displayName = {dataDescriptionHook.displayName}>
      <div slot="view">
        <Hook {id} {version} {...dataDescriptionHook} />
      </div>
    </HookContainer>

    <HookContainer displayName = 
    {fileUploadHook.displayName} 
    let:errorHandler 
    let:successHandler 
    content=9>
      <div slot="view">
        <FileUpload {id} {version} hook={fileUploadHook}  on:error={(e)=> errorHandler(e)} on:success={(e)=> successHandler(e)} />
      </div>
    </HookContainer>
  </Col>
 <Col xs="1"> <!-- validation Hooks-->
    <div class="left-container">
      <b>placeholder validation</b>
      <Hook {id} {version} {...validationHook} on:click={() => showValidation=true} />
      <Validation {id} {version} {...validationHook} open={showValidation} on:close={()=>showValidation = false}/>
    </div>
  </Col> 
</Row>
 
 {:else} <!-- while data is not loaded show a loading information -->
    <Spinner color="primary" size="sm" type ="grow" text-center />
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