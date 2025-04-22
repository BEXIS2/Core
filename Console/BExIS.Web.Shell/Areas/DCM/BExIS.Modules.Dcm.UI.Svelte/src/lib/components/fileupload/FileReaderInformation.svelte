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

	let loading: boolean = false;
	let fileReaderInfoIsSet: boolean = isSet(asciiFileReaderInfo);
	console.log('🚀 ~ fileReaderInfoIsSet:', fileReaderInfoIsSet);
	let style: string = fileReaderInfoIsSet ? 'success' : 'warning';
	let open: boolean = !fileReaderInfoIsSet;

	export let target: string | undefined = undefined;
	$: target;
	let model: DataStructureCreationModel | null;
	$: model;
	let list: string[] = [];
	$: list, update(readableFiles);

	function update(files) {
		loading = true;
		list = files.map((f) => f.name);
		target = undefined;
		loading = false;
	}

	function isSet(type: any): boolean {
		if (type === undefined) {
			return false;
		}

		if (type.cells == undefined || type.cells.length == 0) {
			return false;
		}

		return true;
	}

	async function selectFile(e) {
		console.log('file reader select file', e.detail.value);

		if (e.detail.value) {
			open = true;
			try {
				model = await load(e.detail.value, id, 0);
				console.log('🚀 ~ selectFile ~ model:', model);

				target = undefined;
			} catch (error) {
				notificationStore.showNotification({
					notificationType: notificationType.error,
					message: 'This file does not have a proper structure; please try another one.'
				});
			}
		}
	}
</script>

<b>File Reader Information for tabular data import</b>
<div class="card shadow-sm border-{style}-600 border-solid border">
	<Accordion {open}>
		<AccordionItem>
			<svelte:fragment slot="summary">
				{#if fileReaderInfoIsSet}
					<span class="variant-filled-surface text-{style}-500"><Fa icon={faCheck} /></span>
				{:else}
					<span class="text-success-900"><Fa icon={faXmark} /></span>
				{/if}
			</svelte:fragment>
			<svelte:fragment slot="lead">Defined</svelte:fragment>
			<svelte:fragment slot="content">
				<FileReader {...asciiFileReaderInfo} />

				<MultiSelect
					id="fileselection"
					title="test"
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
