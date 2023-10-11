<script lang="ts">
	import { Modal, modalStore, type ModalSettings } from '@skeletonlabs/skeleton';
	import { getMeanings, remove } from './services';
	import { MeaningModel, type ExternalLink } from './types';
	import {
		Page,
		Table,
		ErrorMessage,
		helpStore,
		TablePlaceholder,
		type TableConfig,
		notificationStore,
		notificationType
	} from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faPlus} from '@fortawesome/free-solid-svg-icons';
	import { meaningsStore } from './stores';
	import { fade, slide } from 'svelte/transition';
	import TableIsApproved from './table/tableIsApproved.svelte';
	import TableExnternalLink from './table/tableExnternalLink.svelte';
	import TableMeaning from './table/tableMeaning.svelte';
	import TableOptions from './table/tableOptions.svelte';

	//stores
	let meanings: MeaningModel[];
	let meaning: MeaningModel = new MeaningModel();

	let showForm = false;

	async function reload() {
		showForm = false;
		const res = await getMeanings();
		meanings = JSON.parse(res.Value);

		meaningsStore.set(meanings);
		console.log('store', $meaningsStore);
	}


	const m: TableConfig<MeaningModel> = {
				id: 'Meaning',
				data: meaningsStore,
				optionsComponent: TableOptions,
				columns: {
							Id: {
								disableFiltering: true,
								exclude: true
							},
							VersionNo: {
								disableFiltering: true,
								exclude: true
							},
							ShortName: {
								disableFiltering: true,
								exclude: true
							},
						 Selectable: {
								instructions: {
									renderComponent: TableIsApproved
								},
								disableFiltering: true
							},
							Approved: {
								disableFiltering: true,
								instructions: {
									renderComponent: TableIsApproved
								}
							},
							ExternalLink: {
								header: 'External Link',
								instructions: {
									renderComponent: TableExnternalLink						
								},
								disableFiltering: true
							},
							Related_meaning: {
								header: 'Related to',
								instructions: {
									renderComponent: TableMeaning,
								},
								disableFiltering: true
							},
							Extra:{
								exclude:true,
								disableFiltering:true
							},
							Variable:{
								exclude:true,
								disableFiltering:true
							}
						}
			}

	function toggleForm() {
		if (showForm) {
			clear();
		}
		showForm = !showForm;
	}

	function clear() {
		meaning = new MeaningModel();
		showForm = false;
	}

	function edit(type: any) {
		if (type.action == 'edit') {
			// variableTemplate = $variableTemplatesStore.find((u) => u.id === type.id)!;
			showForm = true;
		}
		if (type.action == 'delete') {
		console.log("🚀 ~ file: +page.svelte:113 ~ edit ~ type:", type)

			

			const confirm: ModalSettings = {
				type: 'confirm',
				title: 'Delete Meaning',
				body: 'Are you sure you wish to delete Meaning ' + meaning.Name + '?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: (r: boolean) => {
					if (r === true) {
						deleteFn(type.id);
					}
				}
			};
			modalStore.trigger(confirm);
		}
	}

	async function deleteFn(id) {

		const res = await remove(id);
		console.log("🚀 ~ file: +page.svelte:135 ~ deleteFn ~ res:", res)

		if (res.Key !="Success") {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: "Can't delete Meaning."
			});
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Meaning deleted.'
			});

			reload();
		}
	}
</script>

<Page help={false} title="Manage Meanings">
	{#await reload()}
		<div class="grid w-full grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h-9 w-96 placeholder animate-pulse" />
			<div class="flex justify-end">
				<button class="btn placeholder animate-pulse shadow-md h-9 w-16"
					><Fa icon={faPlus} /></button
				>
			</div>
		</div>
		<div class="table-container w-full">
			<TablePlaceholder cols={5} />
		</div>
	{:then}
		<!-- svelte-ignore a11y-click-events-have-key-events -->
		<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h3 h-9">
				{#if meaning.Id < 1}
					<span in:fade={{ delay: 400 }} out:fade>Create neẇ Meaning</span>
				{:else}
					<span in:fade={{ delay: 400 }} out:fade>{meaning.Name}</span>
				{/if}
			</div>
			<div class="text-right">
				{#if !showForm}
					<!-- svelte-ignore a11y-mouse-events-have-key-events -->
					<button
						transition:fade
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						title="Create neẇ Meaning"
						id="create"
						on:mouseover={() => {
							helpStore.show('create');
						}}
						on:click={() => toggleForm()}><Fa icon={faPlus} /></button
					>
				{/if}
			</div>
		</div>

		{#if showForm}
			<div in:slide out:slide>
				<!-- <VariableTemplate variable = {variableTemplate} {missingValues} on:cancel={()=>clear()} on:success={()=>onSuccessFn(variableTemplate.id)} on:fail={onFailFn}/> -->
			</div>
		{/if}

		<div class="table table-compact w-full">
			<Table 
				on:action={(obj) => edit(obj.detail.type)}
				config={m} />

		</div>
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</Page>

<Modal />
