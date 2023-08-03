<script lang="ts">

	import { Spinner, ErrorMessage, positionType } from '@bexis2/bexis2-core-ui';
	import { getHookStart } from '$services/HookCaller';
	import { submit } from '../../routes/edit/services';
 import type { SubmitModel, submitResponceType } from '$models/SubmitModels';
	import GoToView from '$lib/components/submit/GoTo.svelte'

	import { Modal, modalStore } from '@skeletonlabs/skeleton';
	import type { ModalSettings, ModalComponent } from '@skeletonlabs/skeleton';


	import {
		latestFileUploadDate,
		latestDataDescriptionDate,
		latestFileReaderDate,
		latestSubmitDate
	} from '../../routes/edit/stores';

	import { onMount, createEventDispatcher } from 'svelte';
	import { goto } from '$app/navigation';


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

let canSubmit:boolean = false;
 $:canSubmit;

	onMount(async () => {
		reload();
	});

	async function reload() {

		//console.log('reload submit');
		model = await getHookStart(start, id, version);
		canSubmit = activateSubmit();

  return model;
	}

	const confirm: ModalSettings = {
		type: 'confirm',
		title: 'Copy',
		body: 'Are you sure you wish to the data?',
		// TRUE if confirm pressed, FALSE if cancel pressed
		response: (r: boolean) => {
			if (r === true) {
				submitBt()

			}
		}
	};

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

	// return a boolean value for 2 diffrent usecases for submit
	//1. upload files only
 //2. updload data with datastructure
	function activateSubmit()
	{
			//check usecase 1
			if(model.hasStructrue == false && model.files.length>0)
			{
				 return true;
			}

   //check usecase 2
			if(model.hasStructrue == true && model.files.length>0 && model.allFilesReadable && model.isDataValid)
			{
				return true;
			}

			return false

	}

</script>

{#await reload()}
 <div class="w-full h-full text-surface-600">
  <Spinner label="loading" position="{positionType.start}"/>
 </div>
{:then m}

<div class="flex-col">
 <button type="button" class="btn variant-filled-primary" disabled={!canSubmit} on:click={()=>modalStore.trigger(confirm)}>Submit</button>
</div>
{:catch error}
<ErrorMessage {error} />
{/await}
