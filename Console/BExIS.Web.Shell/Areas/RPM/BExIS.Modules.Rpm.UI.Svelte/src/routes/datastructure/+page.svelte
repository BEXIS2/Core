<script lang="ts">
	//bexis2 core ui
	import {
		Page,
		TablePlaceholder,
		Table,
		pageContentLayoutType,
		ErrorMessage,
		FileUploader,
		notificationStore,
		notificationType,
		helpStore
	} from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faPlus, faXmark } from '@fortawesome/free-solid-svg-icons';

	// types & stores
	import { getModalStore, type ModalSettings, Modal } from '@skeletonlabs/skeleton';
	import type { DataStructureModel } from '$lib/components/datastructure/types';
	import { writable } from 'svelte/store';

	// services
	import { getDataStructures } from '$lib/components/datastructure/services';
	import { goTo } from '$services/BaseCaller';
	import { remove } from '$lib/components/datastructure/services';

	import TableDatasets from './tableDatasets.svelte';
	import TableOption from './tableOptions.svelte';

	// svelte
	import { fade, slide } from 'svelte/transition';

	import type { linkType } from '@bexis2/bexis2-core-ui';

	// load data
	let structures: DataStructureModel[];
	const structuresStore = writable<DataStructureModel[]>([]);

	const modalStore = getModalStore();

	let showOptions: boolean = false;

	$: structures, structuresStore.set(structures);

	async function reload() {
		structures = await getDataStructures();
		structuresStore.set(structures);
	}

	let submit = '/rpm/datastructure/upload';

	function success(e) {
		console.log('success', e.detail.text, e.detail.files);
		notificationStore.showNotification({
			notificationType: notificationType.success,
			message: e.detail.files[0] + ' is uploaded'
		});

		goTo('/rpm/datastructure/create?file=' + e.detail.files[0]);
	}

	function error(e) {
		console.log('error', e);

		notificationStore.showNotification({
			notificationType: notificationType.error,
			message: e.detail.messages.join(', ')
		});
	}

	function addEmpty() {
		goTo('/rpm/datastructure/create?file=');
	}

	function copy(id: number) {
		goTo('/rpm/datastructure/create?structureId=' + id + '&&file=');
	}

	function edit(id: number) {
		goTo('/rpm/datastructure/edit?structureId=' + id);
	}

	function download(id: number) {
		goTo('/rpm/datastructure/downloadTemplate?id=' + id);
	}

	async function deleteFn(ds: DataStructureModel) {
		const success = await remove(ds.id);

		if (success != true) {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: 'Can\'t delete Structure "'+ ds.title +'".'
			});
			return false;
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Structure "'+ ds.title +'" deleted.'
			});
			return true;
		}
	}

	async function tableFn(type: any) {
		// edit data data structure based on id
		if (type.action == 'edit') {
			edit(type.id);
			window.scrollTo({ top: 0, behavior: 'smooth' });
		}

		// copy data data structure based on id
		if (type.action == 'download') {
			download(type.id);
		}

		// copy data data structure based on id
		if (type.action == 'copy') {
			copy(type.id);
		}

		// copy data data structure based on id
		if (type.action == 'delete') {
			console.log('delete');
			let ds: DataStructureModel = structures.find((u) => u.id === type.id)!;
			const confirm: ModalSettings = {
				type: 'confirm',
				title: 'Delete Structure',
				body: 'Are you sure you wish to delete structure "'+ds.title + ' (' + ds.id + ')"?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: async (r: boolean) => {
					let success :boolean = await deleteFn(ds);
						if (success)
						{
							reload();
						}
				}
			};

			modalStore.trigger(confirm);
		}
	}

	let links:linkType[] = [
		{
			label: 'Manual',
			url: '/home/docs/Data%20Description#data-structures',
		}
	];
</script>

<Page
	title="Data Structures"
	note="overview of data structures in the system"
	contentLayoutType={pageContentLayoutType.center}
	{links}
>
	{#await reload()}
		<div class="grid w-full grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h-9 w-96 placeholder animate-pulse" />
			<div class="flex justify-end">
				<button class="btn placeholder animate-pulse shadow-md h-9 w-16"
					><Fa icon={faPlus} /></button
				>
			</div>
		</div>

		<TablePlaceholder cols={4} rows={5} />
	{:then}
		<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h3 h-9">
				<span in:fade={{ delay: 400 }} out:fade>Create new Data Structure</span>
			</div>
			<div class="text-right">
				{#if !showOptions}
					<!-- svelte-ignore a11y-mouse-events-have-key-events -->
					<button
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						title="Create new Unit"
						id="create"
						on:mouseover={() => {
							helpStore.show('create');
						}}
						on:click={() => (showOptions = !showOptions)}><Fa icon={faPlus} /></button
					>
				{:else}
					<button
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						on:click={() => (showOptions = false)}><Fa icon={faXmark} /></button
					>
				{/if}
			</div>
		</div>

		{#if showOptions}
			<div class="flex gap-10" in:slide out:slide>
				<div class="w-80">a) Upload a file to create a new data structure based on it</div>
				<div class="grow">
					<FileUploader
						context="data structure creation"
						data={{
							accept: ['.csv', '.txt', '.tsv'],
							multiple: true,
							existingFiles: [],
							descriptionType: 0,
							maxSize: 1024
						}}
						{submit}
						on:error={error}
						on:success={success}
					/>
				</div>
			</div>
			<div class="flex gap-10">
				<div class="w-80">b) or start with an empty data structure.</div>

				<button class="btn variant-filled-primary grow" on:click={addEmpty}
					><Fa icon={faPlus} /></button
				>
			</div>
		{/if}

		<Table
			on:action={(obj) => tableFn(obj.detail.type)}
			config={{
				id: 'datastructure',
				data: structuresStore,
				optionsComponent: TableOption,
				columns: {
					linkedTo: {
						header: 'Linked',
						disableFiltering: true,
						instructions: {
							renderComponent: TableDatasets
						}
					},

					optionsColumn: {
						fixedWidth: 180
					}
				}
			}}
		/>
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</Page>
<Modal />
