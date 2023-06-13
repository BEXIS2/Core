<script lang="ts">

import { onMount } from 'svelte';
import { fade } from 'svelte/transition';
import { Modal, modalStore } from '@skeletonlabs/skeleton';
import { setApiConfig, Spinner, Table } from '@bexis2/bexis2-core-ui';
import * as apiCalls  from './services/apiCalls';
import Form from './components/form.svelte';
import TableOption from './components/tableOptions.svelte';
import { writable, type Writable } from 'svelte/store';

import type { ModalSettings, ModalComponent } from '@skeletonlabs/skeleton';
import type {UnitListItem, DimensionListItem} from "./models";

  
let u: UnitListItem[] = [];
let dimension:DimensionListItem = {id:0, name:""};
let unit:UnitListItem = {
    id:0,
    name:"",
    description:"",
    abbreviation:"",
    dimension: dimension,
    datatypes:[],
    measurementSystem:""
  }; 
const tableStore = writable<any[]>([]);
let showForm=false;
$:units = u;
$:ts = setTableStore(u);
$:tableStore.set(ts);
$:unit.dimension = dimension;

onMount(async () => {
  setApiConfig("https://localhost:44345","*","*");
  u = await apiCalls.GetUnits();
  let ds:DimensionListItem[] = await (apiCalls.GetDimensions());
  dimension = ds.find(d => d.id == 1)!;
  clear();
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
            dimension:u.dimension.name,
            measurementSystem:u.measurementSystem,
          }
        ]        
      }
    );
    return t;
  }

async function reload(): Promise<void>
{
  showForm=false;
  u = await apiCalls.GetUnits();
  clear();
}

async function clear()
{
  unit= {
    id:0,
    name:"",
    description:"",
    abbreviation:"",
    dimension: dimension,
    datatypes:[],
    measurementSystem:""
  }; 
}

function editUnit(type:any)
{
  unit = units.find(u => u.id === type.id)!;
  if(type.action == 'edit')
  {
    showForm=true;
  }
  if(type.action == 'delete')
  {
    const modal: ModalSettings = {
      type: 'confirm',
      title: 'Delete Unit',
      body: 'Are you sure you wish to delete Unit '+  unit.name + ' (' + unit.abbreviation + ')?',
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

</script>

<div class="p-5">
{#if ts.length > 0 && ts}

  <h1>units</h1>
  
  <div class="py-5">
    {#if showForm}
      <div in:fade out:fade>
        <Form {unit} {units} on:cancel={() => showForm=false} on:save={reload}/>
      </div>
    {:else}
      <button type="button" class="btn variant-filled" on:click={() => (showForm = !showForm)}>+</button>
    {/if}
  
  </div>

  <div class="w-max">
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
 
{:else}
<Spinner/>  
{/if}
</div>

