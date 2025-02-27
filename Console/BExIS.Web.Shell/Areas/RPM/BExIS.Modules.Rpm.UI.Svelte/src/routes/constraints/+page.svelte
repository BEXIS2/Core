<script lang="ts">
	import { onMount } from 'svelte';
	import { slide, fade } from 'svelte/transition';
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	import {
		Page,
		Table,
		ErrorMessage,
		helpStore,
		notificationStore,
		notificationType,
		TablePlaceholder,
		type helpItemType
	} from '@bexis2/bexis2-core-ui';
	import * as apiCalls from './services/apiCalls';
	import Form from './components/form.svelte';
	import TableOption from './components/tableOptions.svelte';
	import ConstraintElement from '../components/constraintElement.svelte';
	import { writable, type Writable } from 'svelte/store';
	import Fa from 'svelte-fa';
	import { faPlus, faXmark } from '@fortawesome/free-solid-svg-icons';

	import type { ModalSettings } from '@skeletonlabs/skeleton';
	import type { ConstraintListItem } from './models';

	const modalStore = getModalStore();
	let cs: ConstraintListItem[] = [];
	const tableStore = writable<ConstraintListItem[]>([]);
	let constraint: ConstraintListItem;
	let showForm = false;
	$: constraints = cs;
	$: tableStore.set(cs);

	//help
	import help from './help/help.json';
	let helpItems: helpItemType[] = help.helpItems;

	onMount(async () => {});
	{
		helpStore.setHelpItemList(helpItems);
		showForm = false;
		clear();
	}

	async function save(): Promise<void> {
		reload();
		toggleForm();
	}

	async function reload(): Promise<void> {
		cs = await apiCalls.GetConstraints();
	}

	async function clear() {
		constraint = {
			id: 0,
			version: 0,
			name: '',
			description: '',
			formalDescription: '',
			type: '',
			negated: false,
			inUseByVariable: false,
			inUseByMeaning: false,
			variableIDs: []
		};
	}

	function toggleForm() {
		if (showForm) {
			clear();
		}
		showForm = !showForm;
	}

	function editConstraint(type: any) {
		if (type.action == 'edit') {
			constraint = { ...constraints.find((c) => c.id === type.id)! };
			showForm = true;
		}
		if (type.action == 'delete') {
			let c :ConstraintListItem = constraints.find((c) => c.id === type.id)!;
			const modal: ModalSettings = {
				type: 'confirm',
				title: 'Delete Constraint',
				body: 'Are you sure you wish to delete Constraint "' + constraint.name + '?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: async (r: boolean) => {
					if (r === true) {
						let success :boolean = await deleteConstraint(type.id);
						if (success) {
							reload();
							if (c.id === constraint.id) {
								toggleForm();
							}
						}
					}
				}
			};
			modalStore.trigger(modal);
		}
	}

	async function deleteConstraint(id: number): Promise<boolean> {
		let success = await apiCalls.DeleteConstraint(id);
		if (success != true) {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: 'Can\'t delete Constraint "' + constraint.name + '".'
			});
			return false;
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Constraint "' + constraint.name + '" deleted.'
			});
			return true;
		}
	}
</script>

<Page help={true} title="Manage Constraints">
	<div class="w-full">
		<h1 class="h1">Constraints</h1>
		{#await reload()}
			<div class="grid w-full grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
				<div class="h-9 w-96 placeholder animate-pulse" />
				<div class="flex justify-end">
					<button class="btn placeholder animate-pulse shadow-md h-9 w-16"
						><Fa icon={faPlus} /></button
					>
				</div>
			</div>
			<div>
				<TablePlaceholder cols={4} />
			</div>
		{:then}
			<!-- svelte-ignore a11y-click-events-have-key-events -->
			<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
				<div class="h3 h-9">
					{#if constraint.id < 1}
						Create new Constraint
					{:else}
						{constraint.name}
					{/if}
				</div>
				<div class="text-right">
					{#if !showForm}
						<!-- svelte-ignore a11y-mouse-events-have-key-events -->
						<button
							class="btn variant-filled-secondary shadow-md h-9 w-16"
							title="Create new Constraint"
							id="create"
							on:mouseover={() => {
								helpStore.show('create');
							}}
							on:click={() => toggleForm()}><Fa icon={faPlus} /></button
						>
					{:else if constraint.creationDate && constraint.lastModified && constraint.creationDate != '' && constraint.lastModified != ''}
						<div class="text-sm">
							<p>Created: {constraint.creationDate}</p>
							<p>Last modified: {constraint.lastModified}</p>
						</div>
					{/if}
				</div>
			</div>

			{#if showForm}
				<div in:slide out:slide>
					<Form {constraint} {constraints} on:cancel={toggleForm} on:save={save} />
				</div>
			{/if}

			<div class="table table-compact w-full">
				<Table
					on:action={(obj) => editConstraint(obj.detail.type)}
					config={{
						id: 'constraints',
						data: tableStore,
						optionsComponent: TableOption,
						columns: {
							id: {
								exclude: true
							},
							version: {
								exclude: true
							},
							formalDescription: {
								header: 'Formal Description',
								instructions: {
									renderComponent: ConstraintElement
								}
							},
							negated: {
								disableFiltering: true,
								exclude: true
							},
							inUseByVariable: {
								disableFiltering: true,
								exclude: true
							},
							inUseByMeaning: {
								disableFiltering: true,
								exclude: true
							},
							variableIDs: {
								disableFiltering: true,
								exclude: true
							},
							creationDate: {
								disableFiltering: true,
								exclude: true
							},
							lastModified: {
								disableFiltering: true,
								exclude: true
							}
						}
					}}
				/>
			</div>
		{:catch error}
			<ErrorMessage {error} />
		{/await}
	</div>
</Page>
<Modal />
