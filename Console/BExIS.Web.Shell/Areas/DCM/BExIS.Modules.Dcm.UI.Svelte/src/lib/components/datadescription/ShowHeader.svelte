<script>
import {Spinner} from '@bexis2/bexis2-core-ui';

import Fa from 'svelte-fa/src/fa.svelte'
import { faTrash } from '@fortawesome/free-solid-svg-icons'

import {deleteStructure} from '../../../services/DataDescriptionCaller'
import { latestDataDescriptionDate } from '../../../routes/edit/stores';

export let id;
export let structureId;
export let title;
export let description;

let loading = false;

async function remove()
{
 loading = true;
 console.log("remove")
 const res = await deleteStructure(id,structureId);

 console.log(res);
 if(res.success = true)
 {
   // update store
   latestDataDescriptionDate.set(Date.now());
 }
 else
 {
   //show message
 }

 loading = false;

}

</script>

<div class="show-datadescription-header-container grid grid-cols-2">

  <div>
    <h2 class="h2">{title} ({structureId}) </h2>
  </div>
  <div>
   <div class="text-end">
    <button class="btn bg-warning-500" on:click="{remove}"><Fa icon={faTrash}/></button>
   </div>
   {#if loading}
    <Spinner/>
   {/if}
  </div>


 <p>{description}</p>
 
</div>

