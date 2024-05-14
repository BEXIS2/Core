<script lang="ts">
	import { host } from '@bexis2/bexis2-core-ui';
	import { getHookStart } from '$services/HookCaller';

	import Fa from 'svelte-fa';
	import { faPen, faCopy } from '@fortawesome/free-solid-svg-icons';
	import MetadataDrawer from '$lib/components/metadata/MetadataDrawer.svelte';
	import { onMount } from 'svelte';
	import {type MetadataModel} from '$models/MetadataModel';


	export let id;
	export let version;

	export let status;
	export let start;
	export let isExtern:boolean = true;

	const defaultFormUrl = host+"/dcm/metadata/loadform" + '?id=' + id + '&version=' + version;


	let open:boolean = false;
	let model:MetadataModel;
	

	let isEnabled = setEnable(status);

	onMount(async () => {
		load();
	});


	async function load() {

		
		model = await getHookStart(start, id, version);
		console.log("LOAD METADATA",model);
	}

	function setEnable(status) {
		if (status == 0 || status == 1) {
			// disabled || access denied
			return false;
		}

		return true; // every other status enable the hook
	}

	function editFn() {

		if(model.useExternalMetadataForm)	{	
			open = true;
			window.open(defaultFormUrl, '_blank')		}
		else{	window.open(defaultFormUrl, '_blank')?.focus();} // default form 		
	}

	// function copyFn() {
	// 	const copyurl = "/dcm/metadata/copy"+ '?id=' + id + '&version=' + version;
	// 	window.open(copyurl, '_blank')?.focus();
	// }

	function clickEditVisibility() {}

	function clickEditStatus() {}


</script>

<div class="flex-col space-y-1">
	<div class="flex gap-3 justify-left">
		<button class="chip variant-filled-secondary flex-none" on:click={editFn}
			><Fa icon={faPen} /></button>
	</div>
	{#if open}
		<MetadataDrawer {id} url={model.externalMetadataFormUrl} on:close={() => open = false}></MetadataDrawer>
	{/if}

</div>
