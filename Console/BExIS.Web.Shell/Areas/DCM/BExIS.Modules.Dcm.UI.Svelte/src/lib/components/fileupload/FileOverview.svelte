<script lang="ts">
	import Fa from 'svelte-fa';
	import FileOverviewItem from './FileOverviewItem.svelte';

	import { Spinner } from '@bexis2/bexis2-core-ui';
	import { onMount, createEventDispatcher } from 'svelte';

	export let id;
	export let files;
	// action to save description
	export let save;
	// action to remove file
	export let remove;

	// add description to file display row
	export let descriptionType;
	let withDescription: boolean;


	const dispatch = createEventDispatcher();

	let el;
	let date: number;
	$: date;

	onMount(async () => {
		date = Date.now();

		setDescriptionValues(descriptionType);
		// console.log("mount file overview");
		// console.log(descriptionType);
		// console.log(withDescription);
		// console.log("files",files);
	});

	async function handleRemoveFile(e, index) {
		//console.log("handleRemoveFile",e,index);

		files.splice(index, 1);
		files = [...files];

		dispatch('warning', { text: e.detail.text });
	}

	async function handleSave(e) {
		dispatch('success', { text: e.detail.text });
	}

	function setDescriptionValues(type) {
		//type can be : none = 0, active = 1, required = 2
		if (type == 0) withDescription = false;
		if (type == 1) withDescription = true;
		if (type == 2) withDescription = true;
	}

	const x = "max-h-[180px"

</script>

{#if files}
	{#if files.length > 0}
	<div class="flex-col">
		<div class="pt-2">
			<b>Uploaded File(s)</b>
		</div>

		<div class="flex gap-16">
			<div class="w-1/4"></div>
			<div class="text-sm">
				File description (optional)
				<button on:click="{()=>colapsed!=colapsed}">xyz</button>
			</div>
			<div class="text-right w-10"></div>
		</div>

		<div class="grid gap-2 divide-y-2 pb-3 overflow-auto">

			<!--<Container> -->
			{#each files as file, index}
				<FileOverviewItem
					{id}
					file={file.name}
					{...file}
					{save}
					{remove}
					on:removed={(e) => handleRemoveFile(e, index)}
					on:saved={handleSave}
					{withDescription}
				/>
			{/each}
			<!-- </Container> -->
		</div>
	</div>
	{/if}
{:else}
	<!-- spinner here -->
	<Spinner textCss="text-surface-800" label="loading files list" />
{/if}
