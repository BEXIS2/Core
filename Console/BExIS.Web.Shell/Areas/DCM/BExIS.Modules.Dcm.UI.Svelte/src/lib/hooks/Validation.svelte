<script>
import {Spinner} from '@bexis2/bexis2-core-ui'

import {getHookStart}  from '../../services/HookCaller'
import { latestFileUploadDate, latestDataDescriptionDate } from '../../routes/edit/stores';
import { onMount }from 'svelte'

// import Selection from '../../routes/structuresuggestion/Selection.svelte'


export let id=0;
export let version=1;
export let status=0;
export let displayName="";
export let start="";
export let description="";

let model;

// modal window for selection
let open = false;


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
  //console.log("run validation");
  load();

} 

const toggle = () => {
    open = !open;
  };

</script>
  
  {#if model}

    {#if model.isValid == true}
          <aside class="alert variant-filled-success">
            <div class="alert-message">
              valid
            </div>
          </aside>
 
      {:else}
          <aside class="alert variant-filled-error">
            <div class="alert-message">
              not valid
            </div>
          </aside>

          {#if model.fileErrors}
            {#each model.fileErrors as fileerror}
              <hr>
              <b>{fileerror.file}</b>
              <ul>
                {#each fileerror.errors as error}
                  <li>{error}</li>
                {/each}
              </ul>

            {/each}
          {/if}

          <br/>
          <button class="btn bg-primary-500" on:click={toggle}>open reader informations</button>
          <!-- <Modal isOpen={open} {toggle} fullscreen="{true}">
            <ModalHeader {toggle}>Setup filer reader information {model.isValid}</ModalHeader>
            <ModalBody>  
              <Selection id={id} on:saved={reload}/>
            </ModalBody>
          </Modal> -->
          <p>modal missing</p>
    {/if}
  


    {:else} <!-- while data is not loaded show a loading information -->

    <Spinner color="info" size="sm" type ="grow" text-center />

  {/if}
