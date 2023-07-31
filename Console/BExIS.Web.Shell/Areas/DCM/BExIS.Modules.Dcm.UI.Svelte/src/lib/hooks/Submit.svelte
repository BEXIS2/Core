<script lang="ts">

	import { Spinner, ErrorMessage } from '@bexis2/bexis2-core-ui';
	import { getHookStart } from '$services/HookCaller';
	import { submit } from '../../routes/edit/services';
 import type { SubmitModel, submitResponceType } from '$models/SubmitModels';

	import {
		latestFileUploadDate,
		latestDataDescriptionDate,
		latestFileReaderDate,
		latestSubmitDate
	} from '../../routes/edit/stores';

	import { onMount, createEventDispatcher } from 'svelte';


	export let id = 0;
	export let version = 1;
	export let status = 0;
	export let displayName = '';
	export let start = '';
	export let description = '';

	const dispatch = createEventDispatcher();

 let model:SubmitModel;
 $:model;

	$: $latestFileUploadDate, reload();
	$: $latestDataDescriptionDate, reload();
	$: $latestFileReaderDate, reload();
	// $: $latestSubmitDate, reload();

let canSubmit:boolean = true;
 $:canSubmit;

	onMount(async () => {
		reload();
	});

	async function reload() {

		//console.log('reload submit');

		model = await getHookStart(start, id, version);
  return model;
	}

 async function submitBt()
 {

  const res:submitResponceType = await submit(id);

  //console.log("submit",res);

		if(!res.success)
		{
				dispatch("error",{messages: res.errors.map(e=>e.issue)});
		}
		else
		{
			if(res.asyncUpload)
			{
				dispatch("success", { text: res.asyncUploadMessage});
			}
			// update store
			latestSubmitDate.set(Date.now()); 
		}

 }

</script>

{#await reload()}
 <div class="w-full h-full text-surface-600">
  <Spinner label="...submit is loading" />
 </div>
{:then m}

<div class="flex-col">
 <button type="button" class="btn variant-filled-primary" disabled={!canSubmit} on:click={submitBt}>Submit</button>
</div>
{:catch error}
<ErrorMessage {error} />
{/await}