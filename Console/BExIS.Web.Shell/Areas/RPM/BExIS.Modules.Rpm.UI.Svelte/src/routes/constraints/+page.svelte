<script lang="ts">
	import { onMount } from 'svelte';
	import { slide, fade } from 'svelte/transition';
	import { modalStore } from '@skeletonlabs/skeleton';
	import {
		Page,
		Table,
		ErrorMessage,
		helpStore,
		notificationStore,
		notificationType,
		TablePlaceholder
	} from '@bexis2/bexis2-core-ui';
	import * as apiCalls from './services/apiCalls';
	import Form from './components/form.svelte';
	import TableOption from '../components/tableOptions.svelte';
	import { writable, type Writable } from 'svelte/store';
	import Fa from 'svelte-fa';
	import { faPlus, faXmark } from '@fortawesome/free-solid-svg-icons';

	import type { ModalSettings } from '@skeletonlabs/skeleton';
	import type { ConstraintListItem } from './models';

	let cs: ConstraintListItem[] = [];
	const tableStore = writable<any[]>([]);
	let constraint: ConstraintListItem;
	let showForm = false;
	$: constraints = cs;
	$: tableStore.set(cs);
	
	onMount(async () => {});

	async function reload(): Promise<void> {
		showForm = false;
		cs = await apiCalls.GetDimensions();
		clear();
	}

	async function clear() {
		constraint = {
			id: 0,
			version: 0,
			name: '',
			description: '',
			formalDescription: '',
			type:'',
			inUse: false
		};
	}

	function toggleForm() {
		if (showForm) {
			clear();
		}
		showForm = !showForm;
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
							in:fade
							out:fade
							class="btn variant-filled-secondary shadow-md h-9 w-16"
							title="Create new Constraint"
							id="create"
							on:mouseover={() => {
								helpStore.show('create');
							}}
							on:click={() => toggleForm()}><Fa icon={faPlus} /></button>
					{/if}
				</div>
			</div>

			{#if showForm}
				<div in:slide out:slide>
					<Form {constraint} {constraints} on:cancel={toggleForm} on:save={reload} />
				</div>
			{/if}

			<div class="table table-compact w-full">
				<Table
					
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
							inUse: {
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