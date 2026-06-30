<script lang="ts">

 import { onMount } from 'svelte';
 import {getDeleted} from '../services.js';
	import Links from '../Links.svelte';
	import type { DeletedModel } from '../types.js';
	import PlaceHolderHookContent from '$lib/hooks/placeholder/PlaceHolderHookContent.svelte';
	import { Alert, TablePlaceholder } from '@bexis2/bexis2-core-ui';

 export let id: number;

 let model:DeletedModel;
 $:model;
 onMount(async ()=>{

  const res  = await getDeleted(id);
  console.log("🚀 ~ res:", res)

    model = res;
  

 })


 </script>


 {#if model}
 <div class="flex gap-4 items-start mb-5">
  
   <h2 class="h2">{model.title} </h2>
   <div class="chip variant-filled-error">Deleted</div>
 </div>
  
 <div>
 <Alert title="" message="The dataset has been withdrawn. Reason: Delete. Please check the 'Related Work' if a new version is available." cssClass="variant-filled-error">
		<b> </b>
	</Alert>
  
 </div>

 <div class="flex">
				<div class="flex-grow card	p-5 w-3/4">
		     
       <div class="p-4 space-y-4">
        <div class="placeholder" />
         <div class="grid grid-cols-3 gap-8">
          <div class="placeholder" />
          <div class="placeholder" />
          <div class="placeholder" />
         </div>
        <div class="grid grid-cols-4 gap-4">
         <div class="placeholder" />
         <div class="placeholder" />
         <div class="placeholder" />
         <div class="placeholder" />
        </div>
        
       </div>
       <div class="p-4 space-y-4">
        <div class="placeholder" />
         <div class="grid grid-cols-3 gap-8">
          <div class="placeholder" />
          <div class="placeholder" />
          <div class="placeholder" />
         </div>
       </div>
       
 
				</div>
				<div class="flex flex-col ml-5 gap-3 w-1/4 ">
      <div class="p-5 card"></div>
      <div class="card p-5"></div>
      <div class="card p-5 "></div>
      <div class="card p-5 "></div>

    </div>

 
  </div>

  <Links links={model.links.to} />

 {/if}



