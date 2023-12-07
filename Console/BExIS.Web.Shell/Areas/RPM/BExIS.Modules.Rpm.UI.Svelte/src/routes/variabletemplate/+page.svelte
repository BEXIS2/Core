<script lang="ts">
	import { onMount } from 'svelte';
	import { slide, fade } from 'svelte/transition';
	import { Modal, getModalStore, Toast, type ModalSettings } from '@skeletonlabs/skeleton';
	const modalStore = getModalStore();
	import {
		Page,
		Table,
		ErrorMessage,
		helpStore,
		TablePlaceholder,
		type helpItemType,
		type listItemType,
		type TableConfig,
		notificationStore,
		notificationType
	} from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';

	// Components
	import VariableTemplate from './VariableTemplate.svelte';
	import TableListItem from './table/tableListItem.svelte';
	import TableListString from './table/tableListString.svelte';
	import TableIsApproved from './table/tableIsApproved.svelte';
	import TableOptions from './table/tableOptions.svelte';

	// types
	import {
		VariableTemplateModel,
		type missingValueType,
		type unitListItemType
	} from '$lib/components/datastructure/types';

	// services
	import {
		getDataTypes,
		getUnitsWithDataTypes,
		getDisplayPattern,
	} from '$lib/components/datastructure/services';
	import { getVariableTemplates, remove, getMeanings, getConstraints } from './services';

	// data
	import { variableTemplateHelp } from './help';

	//stores
	import {
		displayPatternStore,
		unitStore,
		dataTypeStore,
		constraintsStore
	} from '$lib/components/datastructure/store';
	import { variableTemplatesStore, meaningsStore } from './stores';

	let vt: VariableTemplateModel[] = [new VariableTemplateModel()];
	let variableTemplate: VariableTemplateModel = new VariableTemplateModel();

	let helpItems: helpItemType[] = variableTemplateHelp;
	let missingValues: missingValueType[] = [];
	let showForm = false;

	const tc: TableConfig<VariableTemplateModel> = {
		id: 'VariableTemplates',
		data: variableTemplatesStore,
		optionsComponent: TableOptions,
		columns: {
			id: {
				disableFiltering: true,
				exclude: true
			},
			approved: {
				header: 'Approved',
				instructions: {
					renderComponent: TableIsApproved
				},
				disableFiltering: true,
				exclude: false
			},
			inUse: {
				header: 'Approved',
				instructions: {
					renderComponent: TableIsApproved
				},
				disableFiltering: true,
				exclude: true
			},
			dataType: {
				header: 'Data Type',
				disableFiltering: true,
				instructions: {
					renderComponent: TableListItem,
					toFilterableValueFn: (value: listItemType) => value.text,
					toStringFn: (d: listItemType) => d.text
				}
			},
			systemType: {
				exclude: true
			},
			unit: {
				header: 'Unit',
				instructions: {
					renderComponent: TableListItem,
					toStringFn: (value: listItemType) => value.text,
					toFilterableValueFn: (value: listItemType) => value.text
				}
			},
			displayPattern: {
				header: 'Display Pattern',
				instructions: {
					renderComponent: TableListItem,
					toStringFn: (value: listItemType) => value.text,
					toFilterableValueFn: (value: listItemType) => value.text
				}
			},
			missingValues: {
				instructions: {
					renderComponent: TableListString
				},
				disableFiltering: true,
				exclude: true
			},
			meanings: {
				instructions: {
					renderComponent: TableListString
				},
				disableFiltering: true,
				exclude: true
			},
			constraints: {
				instructions: {
					renderComponent: TableListString
				},
				disableFiltering: true,
				exclude: true
			}
		}
	};

	onMount(async () => {
		helpStore.setHelpItemList(helpItems);

		const datatypes = await getDataTypes();
		dataTypeStore.set(datatypes);

		const units = await getUnitsWithDataTypes();
		unitStore.set(units);

		const meanings = await getMeanings();
		meaningsStore.set(meanings);

		const constraints = await getConstraints();
		constraintsStore.set(constraints);

		// load display pattern onces for all edit types
		const displayPattern = await getDisplayPattern();
		displayPatternStore.set(displayPattern);

		showForm = false;
	});

	async function reload() {
		showForm = false;
		variableTemplate = new VariableTemplateModel();
		vt = await getVariableTemplates();
		console.log('variable templates', vt);
		variableTemplatesStore.set(vt);
		console.log('store', $variableTemplatesStore);
	}

	function onSuccessFn(id) {
		const message = id > 0 ? 'Variable Template updated.' : 'Variable Template created.';

		notificationStore.showNotification({
			notificationType: notificationType.success,
			message: message
		});
		reload();
		showForm = false;
	}

	function toggleForm() {
		if (showForm) {
			clear();
		}
		showForm = !showForm;
	}

	function clear() {
		variableTemplate = new VariableTemplateModel();
		showForm = false;
	}

	function onFailFn() {
		notificationStore.showNotification({
			notificationType: notificationType.error,
			message: "Can't save Variable Template."
		});
	}

	async function deleteFn(id) {
		const success = await remove(id);

		if (success != true) {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: "Can't delete Variable Template."
			});
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Variable Template deleted.'
			});

			reload();
		}
	}

	function edit(type: any) {
		if (type.action == 'edit') {
			variableTemplate = $variableTemplatesStore.find((u) => u.id === type.id)!;
			showForm = true;
			window.scrollTo({ top: 60, behavior: 'smooth' })
		}
		if (type.action == 'delete') {
			const confirm: ModalSettings = {
				type: 'confirm',
				title: 'Delete Variable Template',
				body: 'Are you sure you wish to delete variable template "' + variableTemplate.name + '"?',
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
</script>

<Page help={true} title="Manage Variable Template">
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
				{#if variableTemplate.id < 1}
					<span in:fade={{ delay: 400 }} out:fade>Create neẇ Variable Template</span>
				{:else}
					<span in:fade={{ delay: 400 }} out:fade>{variableTemplate.name}</span>
				{/if}
			</div>
			<div class="text-right">
				{#if !showForm}
					<!-- svelte-ignore a11y-mouse-events-have-key-events -->
					<button
						transition:fade
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						title="Create neẇ Variable Template"
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
				<VariableTemplate
					variable={variableTemplate}
					{missingValues}
					on:cancel={() => clear()}
					on:success={() => onSuccessFn(variableTemplate.id)}
					on:fail={onFailFn}
				/>
			</div>
		{/if}

		<div class="table table-compact w-full">
			<Table on:action={(obj) => edit(obj.detail.type)} config={tc} />
		</div>
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</Page>
<Modal />
