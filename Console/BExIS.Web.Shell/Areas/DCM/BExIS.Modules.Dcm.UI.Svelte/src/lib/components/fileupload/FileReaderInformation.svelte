<script lang="ts">
	import { MultiSelect, notificationStore, notificationType } from '@bexis2/bexis2-core-ui';

	import type { DataStructureCreationModel } from '@bexis2/bexis2-rpm-ui';

	import type { asciiFileReaderInfoType, fileInfoType } from '@bexis2/bexis2-core-ui';

	import { load } from './services';
	import FileReaderSelectionModal from './FileReaderSelectionModal.svelte';
	import FileReader from './FileReader.svelte';
	import { Accordion, AccordionItem } from '@skeletonlabs/skeleton';

	import Fa from 'svelte-fa';
	import { faCheck, faXmark } from '@fortawesome/free-solid-svg-icons';

	export let id;
	export let readableFiles: fileInfoType[] = [];
	export let asciiFileReaderInfo: asciiFileReaderInfoType;

	export let target: string | undefined = undefined;
	$: target;
	let model: DataStructureCreationModel | null;
	$: model;
	let list: string[] = [];
	$: list, update(readableFiles);

	let loading = false;
	let open = false;

	let style = asciiFileReaderInfo ? 'success' : 'warning';

	async function selectFile(e) {
		console.log('file reader select file', e.detail.value);

		if (e.detail.value) {
			open = true;
			try {
				model = await load(e.detail.value, id, 0);
				console.log("🚀 ~ selectFile ~ model:", model)
				
				target = undefined;
			} catch (error) {
				notificationStore.showNotification({
					notificationType: notificationType.error,
					message: 'This file does not have a proper structure; please try another one.'
				});
			}
		}
	}

	function update(files) {
		loading = true;
		list = files.map((f) => f.name);
		target = undefined;
		loading = false;
	}

	// after closing the selection window reset values
	function close() {
		open = false;
		model = null;
	}
</script>
<div class="card shadow-sm border-{style}-600 border-solid border">
	<Accordion>
		<AccordionItem {open}>
			<svelte:fragment slot="summary">
				{#if asciiFileReaderInfo}
					<span class="variant-filled-surface text-{style}-500"><Fa icon={faCheck} /></span>
				{/if}
			</svelte:fragment>
			<svelte:fragment slot="lead">File Reader Information</svelte:fragment>
			<svelte:fragment slot="content">
				<FileReader {...asciiFileReaderInfo} />

				<MultiSelect
					id="fileselection"
					title=""
					bind:target
					source={list}
					on:change={selectFile}
					isMulti={false}
					complexTarget={true}
					{loading}
					placeholder="Please select a file to set/update the file reader information"
				/>
				{#if model}
					<FileReaderSelectionModal {model} on:close={close} />
				{/if}
			</svelte:fragment>
		</AccordionItem>
	</Accordion>
</div>
