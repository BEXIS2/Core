<script lang="ts">
	import Hook from '$lib/components/Hook.svelte';

	import HookContainer from '$lib/components/HookContainer.svelte';
	import Validation from '$lib/hooks/Validation.svelte';
	import FileUpload from '$lib/hooks/FileUpload.svelte';
	import DataDescription from '$lib/hooks/DataDescription.svelte';
	import Submit from '$lib/hooks/Submit.svelte';
	import Metadata from '$lib/hooks/Metadata.svelte';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faAngleRight } from '@fortawesome/free-solid-svg-icons';

	// models
	import type { HookModel } from './types';

	export let id;
	export let version;
	export let hooks: HookModel[];

	let metadataHook;
	let dataDescriptionHook;
	let fileUploadHook;
	let validationHook;
	let submitHook;

	$: hooks, setHooks(hooks);

	function setHooks(_hooks) {
		_hooks.forEach((h) => {
			if (h.name == 'metadata') {
				metadataHook = h;
			}
			if (h.name == 'fileupload') {
				fileUploadHook = h;
			}
			if (h.name == 'validation') {
				validationHook = h;
			}
			if (h.name == 'datadescription') {
				dataDescriptionHook = h;
			}
			if (h.name == 'submit') {
				submitHook = h;
			}
		});

		//console.log("_hooks",_hooks);
	}

	function errorHandler(e) {
		console.log('error event in data');
	}


</script>

{#if hooks}
	<!-- if hooks list is loaded render hooks -->
	<div class="grid divide-y">
		<HookContainer {...metadataHook}>

			<div>
				<Metadata {id} {version} {...metadataHook} />
			</div>
		</HookContainer>

		<HookContainer {...dataDescriptionHook} let:dateHandler >
			<div>
				<DataDescription
					{id}
					{version}
					hook={dataDescriptionHook}
					on:error={(e) => errorHandler(e)}
					on:dateChanged={(e) => dateHandler(e)}
				/>
			</div>
		</HookContainer>

		<HookContainer
			{...fileUploadHook}
			let:errorHandler
			let:successHandler
			let:warningHandler
			let:dateHandler
		>
			<div >
				<FileUpload
					{id}
					{version}
					hook={fileUploadHook}
					on:error={(e) => errorHandler(e)}
					on:success={(e) => successHandler(e)}
					on:warning={(e) => warningHandler(e)}
					on:dateChanged={(e) => dateHandler(e)}
				/>
			</div>
		</HookContainer>

		<HookContainer {...validationHook}>
			<div >
				<Validation {id} {version} {...validationHook} />
			</div>
		</HookContainer>

		<HookContainer {...submitHook} let:errorHandler let:successHandler let:warningHandler>
			<div >
				<Submit
					{id}
					{version}
					{...submitHook}
					on:error={(e) => errorHandler(e)}
					on:success={(e) => successHandler(e)}
					on:warning={(e) => warningHandler(e)}
				/>
			</div>
		</HookContainer>
	</div>
{:else}
	<!-- while data is not loaded show a loading information -->
	<Spinner />
{/if}
