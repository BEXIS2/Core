<script>
import {getHookStart}  from '../../services/Caller'
import { latestFileUploadDate, latestDataDescriptionDate } from '../../stores/editStores';
import { Spinner, Alert } from 'sveltestrap';
import { onMount }from 'svelte'


export let id=0;
export let version=1;
export let status=0;
export let displayName="";
export let start="";
export let description="";

let model;

$:$latestFileUploadDate, reload()
$:$latestDataDescriptionDate, reload()

onMount(async () => {
  load();
})

async function load()
{
  //const res = await fetch(url);
  model = await getHookStart(start,id,version);
  console.log("validation",model);
}

async function reload()
{
  console.log("run validation");
  load();
} 

</script>
  
  {#if model}

    {#if model.isValid == true}
          <Alert color="success" dismissible>valid</Alert>  
      {:else}
          <Alert color="danger" dismissible>not valid</Alert>

          <b>errors</b>
          
          <ul>
            {#each model.errors as error}
              <!-- content here -->
              <li>{error}</li>
            {/each}
          </ul>

          <b>sorted errors</b>
          <ul>
            {#each model.sortedErrors as error}
            <!-- content here -->
            <li >{error}</li>
            {/each}
          </ul>
    {/if}
  


    {:else} <!-- while data is not loaded show a loading information -->

    <Spinner color="info" size="sm" type ="grow" text-center />

  {/if}
