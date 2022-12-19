<script>

import {
    ButtonDropdown,
    DropdownItem,
    DropdownMenu,
    DropdownToggle
  } from 'sveltestrap';

import {goTo}  from '../../../services/BaseCaller'

export let id = 0; // entity id
export let files = null;

function goToGenerate(file)
{
   // if its possible the file will be used to start structure analyze
    goTo('/dcm/StructureSuggestion/?id='+id+'&file='+file);
}

</script>

<ButtonDropdown>
 <DropdownToggle color="primary" caret>Create Strutcure</DropdownToggle>
 <DropdownMenu>
   <DropdownItem header>Create</DropdownItem>
   <DropdownItem>New</DropdownItem>

   {#if files && files.length>0}
     <DropdownItem divider />
     <DropdownItem header>File</DropdownItem>
     {#each files as file}
       <!-- content here -->
       <DropdownItem on:click={goToGenerate(file.name)}>{file.name}</DropdownItem>
    {/each}
   {/if}

 </DropdownMenu>
</ButtonDropdown>


