<script lang="ts">
	 import { onMount, createEventDispatcher } from 'svelte';
  import { getHookStart } from '$services/HookCaller';
  import type { DataModel } from '$models/Data';
  import Files from '$lib/components/data/Files.svelte';
  import PlaceHolderHookContent from './placeholder/PlaceHolderHookContent.svelte';
  import {ErrorMessage} from '@bexis2/bexis2-core-ui';
	 import { latestSubmitDate, latestDataDate } from '../../routes/edit/stores';
 	
		export let id = 0;
  export let version = 1;
  export let hook;

  let model:DataModel;



  const dispatch = createEventDispatcher();
  
	onMount(async () => {

  console.log("DATA",hook);

		load();
		latestSubmitDate.subscribe((s) => {

		});

	});

	async function reload() {
		/*update store*/
		latestDataDate.set(Date.now());

		/* load data*/
		load();
	}


	async function load() {
		model = await getHookStart(hook.start, id, version);
		console.log("🚀 ~ load ~ model:", model)
 	
		dispatch('dateChanged', { lastModification: model.lastModification });
	}

	function success(e) {
		console.log('success');
		//reload() 
		dispatch('success', { text: e.detail.text });
		
	}

	function warning(e) {
		console.log('warning');
		//reload() 
		dispatch('warning', { text: e.detail.text });
	}

</script>

<div class="space-y-2">
	{#await load()}
		<PlaceHolderHookContent />
	{:then result}

 {#if model.hasStructure}
   <div>table needed</div>
 {:else}

  <Files id={model.id} 
		bind:files={model.existingFiles} 
		bind:deletedFiles={model.deleteFiles} 
		bind:descriptionType={model.descriptionType}
		on:success={success}
		on:warning={warning}
		/>
 {/if}


	{:catch error}
		<ErrorMessage {error} />
	{/await}
</div>
