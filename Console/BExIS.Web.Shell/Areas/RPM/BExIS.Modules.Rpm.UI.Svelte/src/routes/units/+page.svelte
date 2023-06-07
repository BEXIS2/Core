<script lang="ts">

import { onMount } from 'svelte';
import { fade } from 'svelte/transition';
import { SlideToggle } from '@skeletonlabs/skeleton';
import { setApiConfig, Spinner, Table } from '@bexis2/bexis2-core-ui';
import * as apiCalls  from './services/apiCalls';

import Form from './components/form.svelte';
import TableOption from './components/tableOptions.svelte';

import type {UnitListItem} from "./models";
import { writable, type Writable } from 'svelte/store';
  
let u: UnitListItem[];
let ts: any[];
$:units = u;
const tableStore = writable<any[]>([]);
$:tableStore.set(ts)

let unit: UnitListItem = {
  id:0,
  name:"",
  description:"",
  abbreviation:"",
  dimension: {id:0, name:""},
  datatypes:[],
  measurementSystem:""
}; 
let showForm=false;

onMount(async () => {
  setApiConfig("https://localhost:44345","*","*");
  u = await apiCalls.GetUnits();
  ts = setTableStore(u);
});

function setTableStore(unitListItems:UnitListItem[]):any
{
  let datatypes: string;
  let ts:any[] = []; 
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
        ts = [...ts, {
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
    return ts;
  }

async function reload(): Promise<void>
{
  showForm=false;
  u = await apiCalls.GetUnits();
  ts = setTableStore(u);
}

function editUnit(type:any)
{
  if(type.action == 'edit')
  {
    unit = units.map(id => units.find(item => item.id === type.id)!)[0];
    showForm=true;
  }
}

</script>

<div class="p-5">
{#if units}

  <h1>units</h1>
  
  <div class="py-5">
    {#if showForm}
      <div in:fade out:fade>
        <Form {unit} on:cancel={() => showForm=false} on:save={reload}/>
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

