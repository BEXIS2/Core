<script>
	import { onMount } from 'svelte';

	import { Alert, Spinner } from '@bexis2/bexis2-core-ui';

	import { hooksStatus } from '../../routes/edit/stores';

	export let name;
	export let displayName;
	export let content = 9;
	export let visible = true;
	export let status = 0;
	export let color = '';

	$: error = [];
	$: success = null;
	$: warnings = [];

	$: active = false;
	$: wait = false;

	onMount(async () => {
		//active = setActive(status);
		hooksStatus.subscribe((h) => {
			if (h[name] != undefined) {
				setStatus(h[name]);
			}
		});
	});

	function errorHandler(e) {
		console.log('handle errors here');
		console.log(e.detail.messages);
		resetInformations();
		error = e.detail.messages;
	}

	function successHandler(e) {
		console.log('handle success here');
		console.log(e.detail.text);
		resetInformations();
		success = e.detail.text;
	}

	function warningHandler(e) {
		console.log('handle warnings here');
		console.log(e.detail.text);
		resetInformations();
		warnings[0] = e.detail.text;
	}

	function resetInformations() {
		error = [];
		warnings = [];
		success = null;
	}

	// visibility
	function setStatus(status) {
		if (status == 0 || status == 1 || status == 5) {
			// disabled || access denied || inactive
			active = false;
		}
		else
		{	
			active = true; // every other status enable the hook

		}

		wait = status == 6?true:false // wait for somthing

	}


</script>

<!-- {$hooksStatus[name]}
{status}
{active} -->

{#if visible && active}
	
	<div class="grid p-5 m-2 grid-cols-6 {color}">
		<div class="flex gap-2 text-center">
			<h4 class="h5">{displayName}</h4>
		</div>
		<div class="col-span-5 grid gap-5">
			{#if error}
				{#each error as item}
					<Alert cssClass="variant-filled-error" message={item} />
				{/each}
			{/if}
			{#if warnings}
				{#each warnings as item}
					<Alert cssClass="variant-filled-warning" message={item} />
				{/each}
			{/if}
			{#if success}
				<Alert cssClass="variant-filled-success" message={success} />
			{/if}
			 {#if !wait}
					<slot name="view" {errorHandler} {successHandler} {warningHandler}>render view</slot>
				{:else}
				<div class="h-full w-full">
					<Spinner label="please waiting"/>
				</div>
			{/if}
		</div>
	</div>

{/if}
