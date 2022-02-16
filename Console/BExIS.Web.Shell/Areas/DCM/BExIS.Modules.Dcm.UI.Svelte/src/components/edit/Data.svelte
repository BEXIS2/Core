<script>

 import Hook from '..//Hook.svelte'

 import HookContainer from '../HookContainer.svelte';
 import Validation from '../../pages/hooks/Validation.svelte'
 import FileUpload from '../../pages/hooks/FileUpload.svelte'
 import Metadata from '../../pages/hooks/Metadata.svelte'
 import {Spinner} from 'sveltestrap';

 export let id;
 export let version;
 export let hooks=null;
 
 let metadataHook;

 let dataDescriptionHook;

 let fileUploadHook;

 let validationHook;
 $:showValidation = false;

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
 }


</script>

{#if hooks} <!-- if hooks list is loaded render hooks -->

<HookContainer displayName = {metadataHook.displayName}>
  <div slot="view">
    <Metadata {id} {version} {...metadataHook}/>
  </div>
</HookContainer>
 
<HookContainer displayName = {fileUploadHook.displayName}>
  <div slot="view">
    <FileUpload {id} {version} hook={fileUploadHook} />
  </div>
</HookContainer>

<HookContainer displayName = {dataDescriptionHook.displayName}>
  <div slot="view">
    <Hook {id} {version} {...dataDescriptionHook} />
  </div>
</HookContainer>

<HookContainer displayName = {validationHook.displayName}>
  <div slot="view">
    <Hook {id} {version} {...validationHook} on:click={() => showValidation=true} />
    <Validation {id} {version} {...validationHook} open={showValidation} on:close={()=>showValidation = false}/>
  </div>
</HookContainer>

 
 {:else} <!-- while data is not loaded show a loading information -->
    <Spinner color="primary" size="sm" type ="grow" text-center />
 {/if}