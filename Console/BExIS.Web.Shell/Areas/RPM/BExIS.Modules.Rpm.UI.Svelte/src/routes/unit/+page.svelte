<script lang="ts">
import { onMount } from 'svelte';
import { slide, fade } from 'svelte/transition';
import { Modal, modalStore } from '@skeletonlabs/skeleton';
import { Page, Table, ErrorMessage, helpStore } from '@bexis2/bexis2-core-ui';
import * as apiCalls  from './services/apiCalls';
import Form from './components/form.svelte';
import TablePlaceholder from '../components/tablePlaceholder.svelte';
import TableOption from './components/tableOptions.svelte';
import { writable, type Writable } from 'svelte/store';
import Fa from 'svelte-fa';
import { faPlus, faXmark} from '@fortawesome/free-solid-svg-icons';

import type { ModalSettings } from '@skeletonlabs/skeleton';
import type { UnitListItem } from "./models";
import type {helpItemType} from "@bexis2/bexis2-core-ui"; 

//help
import help from './help/help.json';
let helpItems: helpItemType[] = help.helpItems;
  
let u: UnitListItem[] = [];
let unit:UnitListItem;
const tableStore = writable<any[]>([]);
let showForm=false;
$:units = u;
$:tableStore.set(setTableStore(u));

onMount(async () => {
  helpStore.setHelpItemList(helpItems);
});

function setTableStore(unitListItems:UnitListItem[]):any[]
{
  let datatypes: string;
  let t:any[] = [];
  unitListItems.forEach
    (u => {
        datatypes = '';
        u.datatypes.forEach(dt => {
            if(datatypes === '')
            {
              datatypes = dt.name
            }
            else
            {
              datatypes = datatypes + ', ' + dt.name
            }  
          }); 
        t = [...t, {
            id:u.id,
            name:u.name,
            description:u.description,
            abbreviation:u.abbreviation,
            datatypes:datatypes,
            dimension:(u.dimension === undefined) ? "": u.dimension.name,
            measurementSystem:u.measurementSystem,
          }
        ]        
      }
    );
    return t;
  }

async function reload()
{
  showForm=false;
  u = await apiCalls.GetUnits();
  clear();
}

function clear()
{
  unit= {
    id:0,
    name:"",
    description:"",
    abbreviation:"",
    dimension: undefined,
    datatypes:[],
    measurementSystem:""
  }; 
}

function editUnit(type:any)
{
  unit = units.find(u => u.id === type.id)!;
  console.log("Unit", unit);
  if(type.action == 'edit')
  {
    showForm=true;
  }
  if(type.action == 'delete')
  {
    const modal: ModalSettings = {
      type: 'confirm',
      title: 'Delete Unit',
      body: 'Are you sure you wish to delete Unit "'+  unit.name + '" (' + unit.abbreviation + ')?',
      // TRUE if confirm pressed, FALSE if cancel pressed
      response: (r: boolean) => {if(r === true){
        deleteUnit(type.id);
      }},
    };
    modalStore.trigger(modal);
  }
}

async function deleteUnit(id:number)
{
  await apiCalls.DeleteUnit(id);
  reload();
}

function toggleForm()
{
  if(showForm)
  {
    clear()
  }
  showForm = !showForm;
}

</script>

<Page help={true}>
<!-- <div class="flex justify-center">
<div class="p-5 max-w-7xl"> -->
  <h1 class="h1">Units</h1>
  {#await reload()}
  <div class="grid w-full grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
    <div class="h-9 w-96 placeholder animate-pulse">
    </div>
    <div class="flex justify-end">
      <button class="btn placeholder animate-pulse shadow-md h-9 w-16"><Fa icon={faPlus}></Fa></button>
    </div>
  </div>

  <div>
    <TablePlaceholder cols={7}/>
  </div>
  {:then}
    <!-- svelte-ignore a11y-click-events-have-key-events -->
    <div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
      <div class="h3 h-9">
        {#if unit.id < 1}
          Create neẇ Unit
        {:else}
          {unit.name}
        {/if}
      </div>
      <div class="text-right">
        {#if !showForm}
        <!-- svelte-ignore a11y-mouse-events-have-key-events -->
        <button in:fade out:fade class="btn variant-filled-secondary shadow-md h-9 w-16" title="Create neẇ Unit" id="create"on:mouseover={() => {helpStore.show('create');}} on:click={() => toggleForm()}><Fa icon={faPlus}></Fa></button>
        {/if}
      </div>
    </div>

      {#if showForm}
        <div in:slide out:slide>
          <Form {unit} {units} on:cancel={toggleForm} on:save={reload}/>
        </div>
      {/if}
    
    <div>
    <Table on:action=
      {(obj) => editUnit(obj.detail.type)}  
    config=
      {{
        id: 'Units',
        data: tableStore,
        optionsComponent: TableOption,
        columns: {
          id: {
          exclude: true
          },
        }
      }}
    />
    </div>
  {:catch error}
      <ErrorMessage {error}/>
  {/await}
<!-- </div>
</div> -->
</Page>
<Modal/>