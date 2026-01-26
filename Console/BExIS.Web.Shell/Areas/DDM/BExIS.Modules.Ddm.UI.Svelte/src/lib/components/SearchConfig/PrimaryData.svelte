<script lang="ts">
	import {
		Table,
		ErrorMessage,
		TablePlaceholder,
		TextInput,
		MultiSelect,
		DropdownKVP, 
	} from '@bexis2/bexis2-core-ui';
	import TableElements from '$lib/components/SearchConfig/tableElements.svelte';
	import TableOption from '$lib/components/SearchConfig/tableOptions.svelte';
	import { writable, type Writable } from 'svelte/store';
	import { getModalStore, Modal } from '@skeletonlabs/skeleton';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faPlus, faSave, faXmark } from '@fortawesome/free-solid-svg-icons';
	import { fade, slide } from 'svelte/transition';
	import suite from './primaryDataValidation';
	import { onMount, tick } from 'svelte';
  

	const modalStore = getModalStore();

	import type { MeaningModel } from '$lib/components/SearchConfig/SearchConfigModel.ts';
	import type { CalcBlockListItem } from '$lib/components/SearchConfig/SearchConfigModel.ts';

	export let entitytemplates: any[] = [];
	export let meanings: MeaningModel[] = [];
	export let searchConfigData: any;
	export let onChangeHandler: (event: Event | CustomEvent<any>) => void;
	export let res: any;
	// local validation result for the primary data form
	let resValidation: any = suite.get();

	let loading: boolean = false;

	// adapt meanings to MultiSelect's expected shape (id, text, group)
	let meaningsSource: any[] = [];
	$: meaningsSource = Array.isArray(meanings)
		? meanings.map((m) => ({
				...m,
				text: m.name,
				group: 'Meanings'
			}))
		: [];
	// adapt entity templates to DropdownKVP's expected shape (id, text, group)
	// add global option on first position
	$: entitytemplatesSource = [{ id: 'global', text: 'Global', group: 'Entity Templates' }].concat(
		Array.isArray(entitytemplates)
			? entitytemplates.map((et) => ({
					...et,
					text: et.name,
					group: 'Entity Templates'
				}))
			: []
	);

	// add operations to DropdownKVP's expected shape (id, text, group)
	let operationsSource: any[] = [
		{ id: 'min/max', text: 'Min/Max', group: 'Operations' },
		{ id: 'avg', text: 'Average', group: 'Operations' },
		{ id: 'sum', text: 'Sum', group: 'Operations' }
	];

	const tableStore: Writable<any[]> = writable([]);

	let currentItem: CalcBlockListItem | null = null;
	let isEditing: boolean = false;

	onMount(async () => {
		// reset suite and initialize with an empty item so resValidation is valid
		suite.reset();
		const initial: CalcBlockListItem = {
			id: '',
			template_name: '',
			operation: '',
			allowed_meanings: []
		};
		resValidation = suite(initial);
	});

	// change event: run validation on current PrimaryData form state
	async function onChangeHandlerPrimaryData(e: any, fieldName: string) {
		// wait for Svelte bind:target updates (selected* vars) to flush
		await tick();

		setTimeout(async () => {
			console.log('input changed', e);
			console.log('fieldName', fieldName);
			console.log('currentItem', currentItem);

			// Build the data object for validation from current form state,
			// regardless of whether we are editing an existing item or creating a new one.
			const dataForValidation: CalcBlockListItem = {
				id: currentItem?.id ?? '',
				template_name:
					currentItem?.template_name ??
					(selectedEntityTemplate ? selectedEntityTemplate.id.toString() : ''),
				operation: (currentItem?.operation ?? selectedOperation?.id ?? '') as any,
				allowed_meanings: selectedAllowedMeanings.map((m) => ({ id: m.id, name: m.name }))
			};
			console.log('dataForValidation', dataForValidation);
			console.log('length meanings', dataForValidation.allowed_meanings.length);

			// reset Vest state so old errors don't linger between runs
			suite.reset();
			resValidation = suite(dataForValidation);
			console.log('res after change', resValidation);
		}, 100);
	}

	function editPrimaryDataCalc(type: any) {
		if (type.action == 'edit') {
			// set selectedAllowedMeanings based on the current item's allowed_meanings
			currentItem = primaryData.find((item) => item.id === type.id);
			console.log('currentItem', currentItem);
			if (currentItem) {
				selectedAllowedMeanings = currentItem.allowed_meanings;
				if (currentItem?.template_name === 'global') {
					// use your special global option
					selectedEntityTemplate = { id: 'global', name: 'global' };
				} else {
					selectedEntityTemplate =
						entitytemplates.find((et) => et.id.toString() === currentItem?.template_name) ?? null;
				}
				console.log('currentItem operation', currentItem?.operation);
				console.log('operationsSource', selectedEntityTemplate);
				selectedOperation = operationsSource.find((op) => op.id === currentItem?.operation);
			}
			isEditing = true;
			showForm = true;
			console.log('edit primary data', type.id);
		}
		if (type.action == 'delete') {
			console.log('delete primary data', type.id);
			let item: CalcBlockListItem = primaryData.find((item) => item.id === type.id)!;
			const modalSettings = {
				type: 'confirm',
				title: 'Delete Primary Data Configuration',
				body:
					'Are you sure you wish to delete Primary Data Configuration with Operation "' +
					item.operation +
					'" for Entity Template "' +
					item.template_name +
					'"?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: async (r: boolean) => {
					if (r === true) {
						// remove from primaryData (table view)
						primaryData = primaryData.filter((pd) => pd.id !== item.id);
						console.log('Updated primaryData after deletion:', primaryData);
						// propagate deletion into underlying searchConfigData structure
						if (item.template_name === 'global') {
							// global calc
							const calc = searchConfigData.global.primary_data?.calc;
							if (Array.isArray(calc)) {
								searchConfigData.global.primary_data.calc = calc.filter(
									(block) => block.operation !== item.operation
								);
							} else if (calc) {
								if (calc.operation === item.operation) {
									searchConfigData.global.primary_data.calc = null;
								}
							}
						} else if (Array.isArray(searchConfigData?.local)) {
							searchConfigData.local.forEach((localCfg) => {
								if (
									!localCfg.primary_data &&
									localCfg.entity_template_id.toString() !== item.template_name
								)
									return;
								const calc = localCfg.primary_data.calc;
								if (Array.isArray(calc)) {
									localCfg.primary_data.calc = calc.filter(
										(block) => block.operation !== item.operation
									);
								} else if (calc) {
									if (calc.operation === item.operation) {
										localCfg.primary_data.calc = null;
									}
								}
							});
						}
						// notify parent so validation can run for this logical field
						onChangeHandler({ target: { id: 'allowed_meanings' } } as any);
					}
				}
			};
			modalStore.trigger(modalSettings);
		}
	}
    


	let primaryData: CalcBlockListItem[] = [];

	// create reactive statement to build table data from ALL local entries
	$: {
		const items: CalcBlockListItem[] = [];
		console.log('searchConfigData', searchConfigData);

		const calc = searchConfigData.global.primary_data?.calc;
		console.log('global calc', calc);
		// if (!calc) return;

		const calcArray = Array.isArray(calc) ? calc : [calc];

		calcArray.forEach((item) => {
			console.log('global calc item', item);
			// convert allowed_meanings to object array (id and name)
			const meaningsArray = item.allowed_meanings.map((meaningId: number) => {
				const meaning = meanings.find((m) => m.id === meaningId);
				return meaning ? { id: meaning.id, name: meaning.name } : { id: meaningId, name: '' };
			});

			items.push({
				id: 'global_' + (items.length + 1),
				template_name: 'global',
				operation: item.operation,

				// add the meanings array at string
				allowed_meanings: meaningsArray
			});
		});

		if (Array.isArray(searchConfigData?.local)) {
			searchConfigData.local.forEach((localCfg) => {
				const calc = localCfg.primary_data?.calc;
				if (!calc) return;

				const calcArray = Array.isArray(calc) ? calc : [calc];
				let position = 0;
				calcArray.forEach((item) => {
					// convert allowed_meanings to object array (id and name)
					const meaningsArray = item.allowed_meanings.map((meaningId: number) => {
						const meaning = meanings.find((m) => m.id === meaningId);
						return meaning ? { id: meaning.id, name: meaning.name } : { id: meaningId, name: '' };
					});

					items.push({
						id: 'local_' + localCfg.entity_template_id.toString() + '_' + position,
						template_name: localCfg.entity_template_id.toString(),
						operation: item.operation,
						// add the meanings array at string
						allowed_meanings: meaningsArray
					});
					position++;
				});
			});
		}

		primaryData = items;
		console.log('primaryData', primaryData);
		tableStore.set(primaryData);
	}

	$: tableStore.set(primaryData);

	let showForm = false;
	let selectedAllowedMeanings: any[] = [];
	let selectedEntityTemplate: any = null;
	let selectedOperation: any = null;

	function toggleForm() {
		if (showForm) {
			clear();
		}

		showForm = !showForm;
	}

	function applyChanges() {
		console.log('Applying changes with selected meanings:', selectedAllowedMeanings);
		// numeric ids from selected meanings
		const allowedIds = selectedAllowedMeanings.map((m) => m.id);

		// update allowed_meanings for all items in primaryData (table view)
		primaryData = primaryData.map((item) => ({
			...item,
			allowed_meanings: selectedAllowedMeanings.map((m) => ({ id: m.id, name: m.name }))
		}));
		// tableStore.set(primaryData);
		console.log('Updated primaryData:', primaryData);

		console.log('Before update, searchConfigData:', searchConfigData);
		console.log('Allowed IDs to set:', allowedIds);
		console.log('Current Item being edited:', currentItem);

		// propagate changes into underlying searchConfigData structure
		if (selectedEntityTemplate.id === 'global') {
			// global calc
			const calc = searchConfigData.global.primary_data?.calc;
			if (Array.isArray(calc)) {
				calc.forEach((block) => {
					if (block.operation === currentItem?.operation) {
						block.allowed_meanings = allowedIds;
					}
				});
			} else if (calc) {
				if (calc.operation === currentItem?.operation) {
					calc.allowed_meanings = allowedIds;
				}
			}
		} else if (
			Array.isArray(searchConfigData?.local) &&
			currentItem &&
			currentItem.id.startsWith('local_')
		) {
			searchConfigData.local.forEach((localCfg) => {
				if (!localCfg.primary_data && localCfg.entity_template_id !== currentItem.id.split('_')[1])
					return;
				const calc = localCfg.primary_data.calc;
				if (Array.isArray(calc)) {
					calc.forEach((block) => {
						if (block.operation === currentItem?.operation) {
							block.allowed_meanings = allowedIds;
						}
					});
				} else if (calc) {
					calc.allowed_meanings = allowedIds;
				}
			});
		}

		// notify parent so validation can run for this logical field
		onChangeHandler({ target: { id: 'allowed_meanings' } } as any);

		toggleForm();
	}

	function addPrimaryDataCalc() {
		console.log(
			'Adding new primary data calculation with selected meanings:',
			selectedAllowedMeanings
		);
		// numeric ids from selected meanings
		const allowedIds = selectedAllowedMeanings.map((m) => m.id);
		console.log('Allowed IDs to add:', allowedIds);
		// create new calc block
		const CalcBlockListItem = {
			id: 'local_' + selectedEntityTemplate.id.toString() + '_' + Date.now(),
			template_name: selectedEntityTemplate.id.toString(),
			operation: selectedOperation.id,
			allowed_meanings: selectedAllowedMeanings.map((m) => ({ id: m.id, name: m.name }))
		};
		// add to primaryData (table view)
		primaryData = [...primaryData, CalcBlockListItem];
		console.log('Updated primaryData:', primaryData);
		// propagate changes into underlying searchConfigData structure
		if (selectedEntityTemplate.id === 'global') {
			// global calc
			if (!searchConfigData.global.primary_data) {
				searchConfigData.global.primary_data = { calc: [] };
			}
			if (!Array.isArray(searchConfigData.global.primary_data.calc)) {
				searchConfigData.global.primary_data.calc = [];
			}
			searchConfigData.global.primary_data.calc.push({
				operation: selectedOperation.id,
				allowed_meanings: allowedIds
			});
		} else if (Array.isArray(searchConfigData?.local)) {
			let localCfg = searchConfigData.local.find(
				(cfg) => cfg.entity_template_id.toString() === selectedEntityTemplate.id.toString()
			);
			if (!localCfg) {
				// create new local config if not existing
				localCfg = {
					entity_template_id: selectedEntityTemplate.id,
					primary_data: { calc: [] }
				};
				searchConfigData.local.push(localCfg);
			}
			if (!localCfg.primary_data) {
				localCfg.primary_data = { calc: [] };
			}
			if (!Array.isArray(localCfg.primary_data.calc)) {
				localCfg.primary_data.calc = [];
			}
			localCfg.primary_data.calc.push({
				operation: selectedOperation.id,
				allowed_meanings: allowedIds
			});
		}
		// notify parent so validation can run for this logical field
		onChangeHandler({ target: { id: 'allowed_meanings' } } as any);
		toggleForm();
	}

	function onCancel() {
		toggleForm();
		clear();
	}

	function clear() {
		selectedAllowedMeanings = [];
		selectedEntityTemplate = null;
		selectedOperation = null;
		currentItem = null;
		isEditing = false;
	}
</script>

<h2 class="text-xl font-semibold mb-4">Primary Data Configuration</h2>

<div>
	<SlideToggle
		active="bg-secondary-500"
		size="sm"
		id="to_index"
		name="Index Primary Data"
		bind:checked={searchConfigData.global.primary_data.to_index}
		on:change={onChangeHandler}>Index Primary Data</SlideToggle
	>
</div>
<div class="flex w-full">
	<!--List of entity templates which overwrite global settings (to_index = true)-->
	<div class="my-4 pl-16">
		<h4 class="text-lg font-semibold mb-2">Overriding</h4>
		{#if searchConfigData.local && searchConfigData.local.length > 0}
			<ul class="list-disc list-inside">
				{#each searchConfigData.local as localCfg}
					{#if localCfg.primary_data}
						{#if localCfg.primary_data.to_index != searchConfigData.global.primary_data.to_index}
							<SlideToggle
								active="bg-secondary-500"
								size="sm"
								id="to_index"
								name={`Index Primary Data for Entity Template ID: ${localCfg.entity_template_id}`}
								bind:checked={localCfg.primary_data.to_index}
								on:change={onChangeHandler}
								>{entitytemplates.find((et) => et.id === localCfg.entity_template_id)?.name ||
									localCfg.entity_template_id}</SlideToggle
							>
							<br />
						{/if}
					{/if}
				{/each}
			</ul>
		{:else}
			<p class="text-surface-600 italic">No entity templates are overriding the global settings.</p>
		{/if}
	</div>
	<div class="my-4 pl-8">
		<h4 class="text-lg font-semibold mb-2">Matching</h4>
		{#if searchConfigData.local && searchConfigData.local.length > 0}
			<ul class="list-disc list-inside">
				{#each searchConfigData.local as localCfg}
					{#if localCfg.primary_data}
						{#if localCfg.primary_data.to_index === searchConfigData.global.primary_data.to_index}
							<SlideToggle
								active="bg-secondary-500"
								size="sm"
								id="to_index"
								name={`Index Primary Data for Entity Template ID: ${localCfg.entity_template_id}`}
								bind:checked={localCfg.primary_data.to_index}
								on:change={onChangeHandler}
								>{entitytemplates.find((et) => et.id === localCfg.entity_template_id)?.name ||
									localCfg.entity_template_id}</SlideToggle
							>
							<br />
						{/if}
					{/if}
				{/each}
			</ul>
		{:else}
			<p class="text-surface-600 italic">No entity templates are matching the global settings.</p>
		{/if}
	</div>
</div>

{#if !searchConfigData}
	<div class="grid w-full grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
		<div class="h-9 w-96 placeholder animate-pulse" />
		<div class="flex justify-end">
			<button class="btn placeholder animate-pulse shadow-md h-9 w-16"><Fa icon={faPlus} /></button>
		</div>
	</div>

	<div class="table-container w-full">
		<TablePlaceholder cols={7} />
	</div>
{:else}
	<!-- svelte-ignore a11y-click-events-have-key-events -->
	<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
		<div class="h4 h-9">
			{#if 0 < 1}
				<span in:fade={{ delay: 400 }} out:fade
					>{#if isEditing}Edit a Configuration{:else}Create a new Configuration{/if}</span
				>
			{:else}
				<span in:fade={{ delay: 400 }} out:fade>{'name'}</span>
			{/if}
		</div>
		<div class="text-right">
			{#if !showForm}
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<button
					transition:fade
					class="btn variant-filled-secondary shadow-md h-9 w-16"
					title="Create new Configuration"
					id="create"
					on:mouseover={() => {}}
					on:click|preventDefault={toggleForm}><Fa icon={faPlus} /></button
				>
			{/if}
		</div>
	</div>

	{#if showForm}
		<div in:slide out:slide>
			<div class="flex gap-5">
				{#if isEditing}
					<div class="pb-2">
						<div><b>Entity Template:</b> {selectedEntityTemplate.name}</div>
						<div><b>Operation:</b> {selectedOperation.text}</div>
					</div>
				{:else}
					<div class="grow w-1/2">
						<DropdownKVP
							id="entityTemplate"
							title="Entity Template"
							bind:target={selectedEntityTemplate}
							source={entitytemplatesSource}
							required={true}
							complexTarget={true}
							help={true}
							invalid={resValidation?.hasErrors('entityTemplate')}
							feedback={resValidation?.getErrors('entityTemplate')}
							on:change={(e) => onChangeHandlerPrimaryData(e, 'entityTemplate')}
						/>
					</div>
					<div class="grow w-1/2">
						<DropdownKVP
							id="operation"
							title="Operation"
							bind:target={selectedOperation}
							source={operationsSource.filter(
								(op) =>
									!primaryData.some(
										(item) =>
											item.template_name.toString() === selectedEntityTemplate?.id.toString() &&
											item.operation === op.id
									)
							)}
							required={true}
							complexTarget={true}
							help={true}
							invalid={resValidation?.hasErrors('operation')}
							feedback={resValidation?.getErrors('operation')}
							on:change={(e) => onChangeHandlerPrimaryData(e, 'operation')}
						/>
					</div>
				{/if}
			</div>
			<MultiSelect
				id="meanings"
				title="Meanings"
				bind:source={meaningsSource}
				itemId="id"
				itemLabel="text"
				itemGroup="group"
				complexSource={true}
				complexTarget={true}
				bind:target={selectedAllowedMeanings}
				isMulti={true}
				placeholder="-- Please select --"
				invalid={resValidation?.hasErrors('meanings')}
				feedback={resValidation?.getErrors('meanings')}
				clearable={false}
				on:clear={(e) => onChangeHandlerPrimaryData(e, 'meanings')}
				on:change={(e) => onChangeHandlerPrimaryData(e, 'meanings')}
				{loading}
			/>
		</div>

		<div>
			<div class="grow text-right gap-2">
				<button title="cancel" type="button" class="btn variant-filled-warning" on:click={onCancel}
					><Fa icon={faXmark} /></button
				>

				{#if isEditing}
					<button
						on:click={applyChanges}
						title="save"
						type="submit"
						class="btn variant-filled-primary"><Fa icon={faSave} /> Apply</button
					>
				{:else}
					<button
						on:click={addPrimaryDataCalc}
						title="save"
						type="submit"
						class="btn variant-filled-primary"><Fa icon={faPlus} /></button
					>
				{/if}
			</div>
		</div>
	{/if}

	<div class="table table-compact w-full">
		<Table
			on:action={(obj) => editPrimaryDataCalc(obj.detail.type)}
			config={{
				id: 'primaryDataCalcTable',
				data: tableStore,
				search: false,
				paginated: false,
				optionsComponent: TableOption,
				columns: {
					id: {
						exclude: true
					},
					template_name: {
						header: 'Entity Template',
						instructions: {
							toStringFn: (key) => {
								// find entity template name by id
								const et = entitytemplates.find((item) => item.id.toString() === key.toString());
								if (et) {
									return et.name;
								} else {
									return key.toString();
								}
							}
						}
					},
					operation: {
						header: 'Calculation Operation'
					},
					allowed_meanings: {
						header: 'Meanings to include',
						disableFiltering: true,
						instructions: {
							renderComponent: TableElements
						}
					}
				}
			}}
		/>
	</div>
{/if}
<Modal />