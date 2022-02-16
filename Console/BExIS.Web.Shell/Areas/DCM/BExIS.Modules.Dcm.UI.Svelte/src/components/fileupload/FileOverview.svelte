<script>
import Fa from 'svelte-fa/src/fa.svelte'

import {FileInfo} from '@bexis2/svelte-bexis2-core-ui'
import { Spinner, Button, Input,Table  } from 'sveltestrap';
import { faTrash, faSave } from '@fortawesome/free-solid-svg-icons'
import { onMount }from 'svelte'

import {removeFile, saveFileDescription}  from '../../services/Caller'



export let id;
export let files;

let el;
$:date=null;

onMount(async () => {
  date = Date.now();
})


async function handleRemoveFile(e, index, file) {

//remove from server
console.log(index +"*"+ file);

const res = await removeFile(id,file);
let result = await res.json();

 if(res.status==200 && result)
 {
   files.splice(index, 1)
   files = [...files];
 }

}

async function handleSaveFileDescription(e, index, file, description) {

//save description on server
console.log(index +"*"+ file + "*"+description);
console.log(id);

const res = await saveFileDescription(id, file, description );

}

</script>

{#if files}
<!-- <h1>{date}</h1> -->
<Table >
 <!-- <thead>
  <tr>
    <th></th>
    <th>file</th>
    <th>description</th>
    <th>action</th>
  </tr>
</thead> -->
 <tbody>
 {#each files as item, index}
      <tr>
       <td><FileInfo type={item.Type} /></td>
       <td>{item.Name}</td>
       <td><Input  bind:value="{item.Description}" placeholder="description" on:change={e => handleSaveFileDescription(e, index, item.Name, item.Description)}/></td>
       <td>
        <Button size="sm" on:click={e => handleRemoveFile(e, index, item.Name)}><Fa icon={faTrash}/></Button>
       </td>
      </tr>
 {/each}
</tbody>
</Table>


   
  {:else}
   <!-- spinner here -->
   <Spinner color="primary" size="sm" type ="grow" text-center />
  {/if}