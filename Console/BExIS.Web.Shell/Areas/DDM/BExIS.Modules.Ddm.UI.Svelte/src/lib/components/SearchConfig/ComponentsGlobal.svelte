<script lang="ts">
	import {
		Table,
		TablePlaceholder,
		TextInput,
		DropdownKVP,
	} from '@bexis2/bexis2-core-ui';
	import TableOption from '$lib/components/SearchConfig/tableOptions.svelte';
	import { writable, type Writable } from 'svelte/store';
	import { getModalStore, Modal, SlideToggle } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faPlus, faSave, faXmark } from '@fortawesome/free-solid-svg-icons';
	import { fade, slide } from 'svelte/transition';
	import { onMount } from 'svelte';
	import { type ModalSettings } from '@skeletonlabs/skeleton';
	import type { GlobalComponent } from '$lib/components/SearchConfig/SearchConfigModel.ts';
	
	const tableOptionComponent = TableOption as any;
	const modalStore = getModalStore();

	export let entitytemplates: any[] = [];
	export let meanings: any[] = [];
	export let searchConfigData: any;
	export let onChangeHandler: (event: Event | CustomEvent<any>) => void;
	export let res: any;

	type GlobalComponentType = 'facets' | 'categories' | 'properties' | 'general';

	interface GlobalComponentListItem extends GlobalComponent {
		type: GlobalComponentType;
		name: string;
		inUseByVariable?: boolean;
		inUseByMeaning?: boolean;
	}

	const tableStore: Writable<GlobalComponentListItem[]> = writable([]);
	let tableItems: GlobalComponentListItem[] = [];

	let showForm = false;
	let isEditing = false;
	let currentItem: GlobalComponentListItem | null = null;

	let selectedType: any = null;
	let componentName = '';
	let description = '';
	let placeholder = '';
	let headerItem = false;
	let defaultHeaderItem = false;

	const componentTypeSource = [
		{ id: 'facets', text: 'Facets', group: 'Component Types' },
		{ id: 'categories', text: 'Categories', group: 'Component Types' },
		{ id: 'properties', text: 'Properties', group: 'Component Types' },
		{ id: 'general', text: 'General', group: 'Component Types' }
	];

	const getGlobalComponentsByType = (type: GlobalComponentType): GlobalComponent[] => {
		return (searchConfigData?.global?.search_components?.[type] ?? []) as GlobalComponent[];
	};

	const setGlobalComponentsByType = (type: GlobalComponentType, list: GlobalComponent[]) => {
		if (!searchConfigData?.global?.search_components) return;
		searchConfigData.global.search_components[type] = list;
	};

	const buildTableItems = (): GlobalComponentListItem[] => {
		const items: GlobalComponentListItem[] = [];
		const types: GlobalComponentType[] = ['facets', 'categories', 'properties', 'general'];

		types.forEach((type) => {
			const list = getGlobalComponentsByType(type);
			list.forEach((component) => {
				const inUseByVar = entitytemplates.some((et) =>
                        et.variables?.some(
                            (v: any) => v.search_component_type === type && v.search_component_id === component.id
                        )
                    );
				const inUseByMean = entitytemplates.some((et) =>
                        et.meanings?.some(
                            (m: any) => m.search_component_type === type && m.search_component_id === component.id
                        )
                    );
				items.push({
					...component,
					type,
					header_item: component.header_item ? 'Yes' : 'No',
					default_header_item: component.default_header_item ? 'Yes' : 'No',
					inUseByVariable: inUseByVar ? 'Yes' : 'No',
					inUseByMeaning: inUseByMean ? 'Yes' : 'No',
				} as any);
			});
		});
        console.log('Built Table Items:', items);
		return items;
	};

	onMount(() => {
		tableItems = buildTableItems();
		tableStore.set(tableItems);
	});

	$: if (searchConfigData?.global?.search_components) {
		tableItems = buildTableItems();
		tableStore.set(tableItems);
	}

	function toggleForm() {
		if (showForm) clear();
		showForm = !showForm;
	}

	function clear() {
		selectedType = null;
		componentName = '';
		description = '';
		placeholder = '';
		headerItem = false;
		defaultHeaderItem = false;
		currentItem = null;
		isEditing = false;
	}

	function editComponent(obj: any) {
        
		if (obj.action == 'edit') {
        
			const item = tableItems.find((i) => i.id == obj.id);

            if (item) {
				currentItem = item;
				selectedType = componentTypeSource.find((t) => t.id == item.type) ?? null;
				componentName = item.component_name;
				description = item.description;
				placeholder = item.placeholder;
				headerItem = item.header_item;
				defaultHeaderItem = item.default_header_item;
			}
            isEditing = true;
			showForm = true;
		}
		if (obj.action === 'delete') {
			const item = buildTableItems().find((i) => i.id === obj.id);
			if (!item) return;
			const modalSettings: ModalSettings = {
				type: 'confirm',
				title: 'Delete Search Component',
				body: `Are you sure you wish to delete Search Component "${item.component_name}"?`,
				response: async (r: boolean) => {
					if (r === true) {
						const list = getGlobalComponentsByType(item.type).filter((c) => c.id !== item.id);
						setGlobalComponentsByType(item.type, list);
						onChangeHandler({ target: { id: 'search_components' } } as any);
					tableItems = buildTableItems();
					tableStore.set(tableItems);
					}
				}
			};
			modalStore.trigger(modalSettings);
		}
	}

	function applyChanges() {
		if (!currentItem || !selectedType?.id) return;

		const newType = selectedType.id as GlobalComponentType;
		const previousType = currentItem.type;
		const updatedComponent: GlobalComponent = {
			id: currentItem.id,
			component_name: componentName,
			description,
			placeholder,
			header_item: headerItem,
			default_header_item: defaultHeaderItem
		};

		if (newType !== previousType) {
			const oldList = getGlobalComponentsByType(previousType).filter((c) => c.id !== currentItem?.id);
			setGlobalComponentsByType(previousType, oldList);

			const newList = getGlobalComponentsByType(newType);
			setGlobalComponentsByType(newType, [...newList, updatedComponent]);
		} else {
			const list = getGlobalComponentsByType(newType);
			const updated = list.map((c) => (c.id === currentItem?.id ? updatedComponent : c));
			setGlobalComponentsByType(newType, updated);
		}
		onChangeHandler({ target: { id: 'search_components' } } as any);
		tableItems = buildTableItems();
		tableStore.set(tableItems);
		toggleForm();
	}

	function addComponent() {
		if (!selectedType?.id) return;
		const type = selectedType.id as GlobalComponentType;

		const list = getGlobalComponentsByType(type);
		const maxId = list.reduce((max, item) => Math.max(max, item.id ?? 0), 0);

		const newItem: GlobalComponent = {
			id: maxId + 1,
			component_name: componentName,
			description,
			placeholder,
			header_item: headerItem,
			default_header_item: defaultHeaderItem
		};

		setGlobalComponentsByType(type, [...list, newItem]);
		onChangeHandler({ target: { id: 'search_components' } } as any);
		tableItems = buildTableItems();
		tableStore.set(tableItems);
		toggleForm();
	}
</script>

<h2 class="text-xl font-semibold mb-4">Global Settings</h2>

<div class="grid grid-cols-2 gap-5 my-4">
	<SlideToggle
		active="bg-secondary-500"
		size="sm"
		id="facets_to_index"
		name="Index Facets"
		bind:checked={searchConfigData.global.search_components.facets_to_index}
		on:change={onChangeHandler}>Index Facets</SlideToggle
	>
	<SlideToggle
		active="bg-secondary-500"
		size="sm"
		id="categories_to_index"
		name="Index Categories"
		bind:checked={searchConfigData.global.search_components.categories_to_index}
		on:change={onChangeHandler}>Index Categories</SlideToggle
	>
	<SlideToggle
		active="bg-secondary-500"
		size="sm"
		id="properties_to_index"
		name="Index Properties"
		bind:checked={searchConfigData.global.search_components.properties_to_index}
		on:change={onChangeHandler}>Index Properties</SlideToggle
	>
	<SlideToggle
		active="bg-secondary-500"
		size="sm"
		id="generals_to_index"
		name="Index General"
		bind:checked={searchConfigData.global.search_components.generals_to_index}
		on:change={onChangeHandler}>Index General</SlideToggle
	>
</div>

<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500 pt-4">
	<div class="h4 h-9">
		{#if showForm}
			<span in:fade={{ delay: 400 }} out:fade>{isEditing ? 'Edit a Component' : 'Create a new Component'}</span>
		{:else}
			<span in:fade={{ delay: 400 }} out:fade>Global Component Definition</span>
		{/if}
	</div>
	<div class="text-right">
		{#if !showForm}
			<button
				transition:fade
				class="btn variant-filled-secondary shadow-md h-9 w-16"
				title="Create new Component"
				id="create"
				on:click|preventDefault={toggleForm}><Fa icon={faPlus} /></button>
		{/if}
	</div>
</div>



{#if showForm}
	<div in:slide out:slide>
		<div class="grid grid-cols-2 gap-4 my-4">
			<DropdownKVP
				id="componentType"
				title="Component Type"
				source={componentTypeSource}
				bind:target={selectedType}
				required={true}
				complexTarget={true}
				on:change={() => {}}
			/>
			<TextInput
				id="componentName"
				label="Component Name"
				bind:value={componentName}
				required={true}
			/>
			<TextInput
				id="description"
				label="Description"
				bind:value={description}
			/>
			<TextInput
				id="placeholder"
				label="Placeholder"
				bind:value={placeholder}
			/>
		</div>
		<div class="grid grid-cols-2 gap-4 my-2">
			<SlideToggle
				active="bg-secondary-500"
				size="sm"
				id="header_item"
				name="Header Item"
				bind:checked={headerItem}
				on:change={onChangeHandler}>Header Item</SlideToggle
			>
			<SlideToggle
				active="bg-secondary-500"
				size="sm"
				id="default_header_item"
				name="Default Header Item"
				bind:checked={defaultHeaderItem}
				on:change={onChangeHandler}>Default Header Item</SlideToggle
			>
		</div>

		<div class="grow text-right gap-2">
			<button title="cancel" type="button" class="btn variant-filled-warning" on:click={toggleForm}
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
					on:click={addComponent}
					title="save"
					type="submit"
					class="btn variant-filled-primary"><Fa icon={faPlus} /></button
				>
			{/if}
		</div>
	</div>
{/if}

<div class="table table-compact w-full">
	{#if !searchConfigData}
		<TablePlaceholder cols={5} />
	{:else}
		<Table
			on:action={(obj) => editComponent(obj.detail.type)}
			config={{
				id: 'globalSearchComponentsTable',
				data: tableStore,
				search: false,
				optionsComponent: tableOptionComponent,
				columns: {
					id: { exclude: true },
					component_name: { header: 'Name' },
					description: { header: 'Description' },
					placeholder: { header: 'Placeholder' },
                    type: { exclude: true }
				}
			}}
		/>
	{/if}
</div>

<Modal />
