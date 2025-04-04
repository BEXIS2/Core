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
	$: asciiFileReaderInfo;

	let loading: boolean = false;
	let fileReaderInfoIsSet: boolean = isSet(asciiFileReaderInfo);
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

	function isSet(type: asciiFileReaderInfoType): boolean {
		if (type === undefined || type === null) {
			return false;
		}

		if (type.cells == undefined || (type.cells != undefined && type.cells.length == 0)) {
			return false;
		}

		return true;
	}

	async function selectFile(e) {
		if (e.detail.value) {
			open = true;
			try {
				model = await load(e.detail.value, id, 0);

				target = undefined;
				fileReaderInfoIsSet = isSet(asciiFileReaderInfo);
			} catch (error) {
				notificationStore.showNotification({
					notificationType: notificationType.error,
					message: 'This file does not have a proper structure; please try another one.'
				});
			}
		}
	}

	function close(e) {
		console.log('close', e, asciiFileReaderInfo);
	}
</script>

<b>File Reader Information for tabular data import</b>

<div class="card shadow-sm border-solid border">
	<Accordion>
		<AccordionItem {open}>
			<svelte:fragment slot="summary">
				{#if isSet(asciiFileReaderInfo)}
					<span class="text-success-500"><Fa icon={faCheck} /></span>
				{:else}
					<span class="text-error-400"><Fa icon={faXmark} /></span>
				{/if}
			</svelte:fragment>
			<svelte:fragment slot="lead">Defined</svelte:fragment>
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
					<FileReaderSelectionModal {model} />
				{/if}
			</svelte:fragment>
		</AccordionItem>
	</Accordion>
</div>
