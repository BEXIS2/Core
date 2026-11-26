<script lang="ts">
	import {
		Table,
		TablePlaceholder,
		DropdownKVP,
		MultiSelect,
	} from '@bexis2/bexis2-core-ui';
	import TableOption from '$lib/components/SearchConfig/tableOptions.svelte';
	import TableElements from '$lib/components/SearchConfig/tableElements.svelte';
    import { writable, type Writable } from 'svelte/store';
	import { getModalStore, Modal, SlideToggle, type ModalSettings } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faPlus, faSave, faXmark } from '@fortawesome/free-solid-svg-icons';
	import { fade, slide } from 'svelte/transition';
	import { onMount } from 'svelte';

   
	import type { LocalComponent } from '$lib/components/SearchConfig/SearchConfigModel.ts';
	import { getMetadataNodes } from '$lib/services/searchConfigServices';

    const tableOptionComponent = TableOption as any;
	const tableElementsComponent = TableElements as any;
	const modalStore = getModalStore();

    export let entitytemplates: any[] = [];
	export let searchConfigData: any;
	export let onChangeHandler: (event: Event | CustomEvent<any>) => void;

	type LocalComponentType = 'facets' | 'categories' | 'properties' | 'general';

	interface LocalComponentListItem extends LocalComponent {
		id: string;
		type: LocalComponentType;
		global_component_name: string;
		template_id: number;
		template_name: string;
	}

	const tableStore: Writable<LocalComponentListItem[]> = writable([]);
	let tableItems: LocalComponentListItem[] = [];

	let showForm = false;
	let isEditing = false;
	let currentItem: LocalComponentListItem | null = null;
	let currentTemplateId: number | null = null;

	let selectedGlobalComponent: any = null;
	let selectedDataType: any = null;
	let selectedMetadataNodes: any[] = [];
	let selectedEntityTemplate: any = null;

	let metadataNodes: any[] = [];
	getMetadataNodes().then((nodes) => {
		metadataNodes = nodes;
	});

	const componentTypeSource = [
		{ id: 'facets', text: 'Facets', group: 'Component Types' },
		{ id: 'categories', text: 'Categories', group: 'Component Types' },
		{ id: 'properties', text: 'Properties', group: 'Component Types' },
		{ id: 'general', text: 'General', group: 'Component Types' }
	];

	const dataTypeSource = [
		{ id: 'byte', text: 'Byte', group: 'Data Types' },
		{ id: 'short', text: 'Short', group: 'Data Types' },
		{ id: 'integer', text: 'Integer', group: 'Data Types' },
		{ id: 'long', text: 'Long', group: 'Data Types' },
		{ id: 'float', text: 'Float', group: 'Data Types' },
		{ id: 'half_float', text: 'Half Float', group: 'Data Types' },
		{ id: 'double', text: 'Double', group: 'Data Types' },
		{ id: 'scaled_float', text: 'Scaled Float', group: 'Data Types' },
		{ id: 'text', text: 'Text', group: 'Data Types' },
		{ id: 'keyword', text: 'Keyword', group: 'Data Types' },
		{ id: 'date', text: 'Date', group: 'Data Types' },
		{ id: 'date_nanos', text: 'Date Nanos', group: 'Data Types' },
		{ id: 'boolean', text: 'Boolean', group: 'Data Types' },
		{ id: 'geo_point', text: 'Geo Point', group: 'Data Types' },
		{ id: 'geo_shape', text: 'Geo Shape', group: 'Data Types' },
		{ id: 'object', text: 'Object', group: 'Data Types' },
		{ id: 'nested', text: 'Nested', group: 'Data Types' },
		{ id: 'ip', text: 'IP', group: 'Data Types' },
		{ id: 'version', text: 'Version', group: 'Data Types' },
		{ id: 'binary', text: 'Binary', group: 'Data Types' }
	];

	const getLocalConfigForTemplate = (templateId: number) => {
		if (!Array.isArray(searchConfigData?.local)) {
			console.log('No local array in searchConfigData');
			return null;
		}
		const found = searchConfigData.local.find((cfg: any) => cfg.entity_template_id === templateId);
		console.log('Looking for entityTemplateId:', templateId);
		console.log('Found local config:', found);
		return found ?? null;
	};

	const getLocalComponentsByType = (templateId: number, type: LocalComponentType): LocalComponent[] => {
		const localCfg = getLocalConfigForTemplate(templateId);
		console.log(`Getting components for type: ${type}`, 'localCfg:', localCfg);
		if (!localCfg) {
			console.log(`No local config found for templateId: ${templateId}`);
			return [];
		}
		const result = (localCfg?.search_components?.[type] ?? []) as LocalComponent[];
		console.log(`Components for type ${type}:`, result);
		return result;
	};

	const setLocalComponentsByType = (templateId: number, type: LocalComponentType, list: LocalComponent[]) => {
		const localCfg = getLocalConfigForTemplate(templateId);
		if (!localCfg) return;
		localCfg.search_components[type] = list;
	};

	const getGlobalComponentName = (globalId: number): string => {
		const types: LocalComponentType[] = ['facets', 'categories', 'properties', 'general'];
		for (const type of types) {
			const component = searchConfigData?.global?.search_components?.[type]?.find(
				(c: any) => c.id === globalId
			);
			if (component) return component.component_name;
		}
		return `Global Component #${globalId}`;
	};

	const buildTableItems = (): LocalComponentListItem[] => {
		const items: LocalComponentListItem[] = [];
		const types: LocalComponentType[] = ['facets', 'categories', 'properties', 'general'];

		if (!Array.isArray(searchConfigData?.local)) return items;

		searchConfigData.local.forEach((localConfig: any) => {
			const templateId = localConfig.entity_template_id;
			const templateName = entitytemplates.find((et) => et.id === templateId)?.name || `Template #${templateId}`;

			types.forEach((type) => {
				const list = localConfig?.search_components?.[type] ?? [];
				list.forEach((component: LocalComponent) => {
					items.push({
							id: `${templateId}-${component.global_id}`,
						template_name: templateName,
                        global_component_name: getGlobalComponentName(component.global_id),
						type,
                        ...component,
                        
						template_id: templateId,
						
						
					});
				});
			});
		});

		console.log('Built Local Table Items:', items);
		return items;
	};

	onMount(() => {
		console.log('searchConfigData:', searchConfigData);
		tableItems = buildTableItems();
		console.log('Initial tableItems:', tableItems);
		tableStore.set(tableItems);
	});

	$: if (searchConfigData?.local) {
		console.log('searchConfigData.local updated, rebuilding...');
		tableItems = buildTableItems();
		console.log('Updated tableItems:', tableItems);
		tableStore.set(tableItems);
	}

	$: metadataNodesSource = Array.isArray(metadataNodes)
		? metadataNodes.map((node) => ({
				id: node.xPath ?? node.id,
				text: node.displayName || node.id,
				group: 'Metadata Nodes'
			}))
		: [];

	$: globalComponentsSource = (() => {
		const components: any[] = [];
		const types: LocalComponentType[] = ['facets', 'categories', 'properties', 'general'];
		types.forEach((type) => {
			const list = searchConfigData?.global?.search_components?.[type] ?? [];
			list.forEach((comp: any) => {
				components.push({
					id: comp.id,
					text: `${comp.component_name} (${type})`,
					group: 'Global Components'
				});
			});
		});
		return components;
	})();

	$: entityTemplatesSource = Array.isArray(searchConfigData?.local)
		? searchConfigData.local.map((cfg: any) => ({
				id: cfg.entity_template_id,
				text: cfg.entity_template_name || `Template #${cfg.entity_template_id}`,
				group: 'Entity Templates'
			}))
		: [];

	const getTemplateName = (templateId: number): string => {
		const fromList = entitytemplates?.find((et) => et.id === templateId);
		return fromList?.name ?? `Template #${templateId}`;
	};

	function toggleForm() {
		if (showForm) clear();
		showForm = !showForm;
	}

	function clear() {
		selectedGlobalComponent = null;
		selectedDataType = null;
		selectedMetadataNodes = [{ id: null }];
		selectedEntityTemplate = null;
		currentItem = null;
		currentTemplateId = null;
		isEditing = false;
	}

	function addMetadataNode() {
		selectedMetadataNodes = [...selectedMetadataNodes, { id: null }];
	}

	function removeMetadataNode(index: number) {
		selectedMetadataNodes = selectedMetadataNodes.filter((_, i) => i !== index);
	}

	function editComponent(obj: any) {
		const detail = obj?.detail ?? obj;
		const row = detail?.row ?? detail;
		console.log('Edit action received:', detail);
		if (obj.action == 'edit') {
            // use the new combined id to find the item
			const item = tableItems.find((i) => i.id === row?.id);
			if (item) {
				currentItem = item;
				currentTemplateId = item.template_id;
				selectedEntityTemplate = entityTemplatesSource.find((t: any) => t.id === item.template_id) ?? null;
				selectedGlobalComponent = globalComponentsSource.find((c) => c.id === item.global_id) ?? null;
				selectedDataType = dataTypeSource.find((d) => d.id === item.data_type_id) ?? null;
				selectedMetadataNodes = (item.metadata_nodes ?? []).length > 0
					? (item.metadata_nodes ?? []).map((node: any) => ({ id: node }))
					: [{ id: null }];
				isEditing = true;
				showForm = true;
			}
			console.log('Editing item:', item);
		}
		if (detail?.action === 'delete') {
			const item = tableItems.find(
				(i) => i.template_id === row?.template_id && i.global_id === row?.global_id && i.data_type_id === row?.data_type_id
			);
			if (!item) return;
			const modalSettings: ModalSettings = {
				type: 'confirm',
				title: 'Delete Local Search Component',
				body: `Are you sure you wish to delete "${item.global_component_name}" for data type "${item.data_type_id}" in template "${item.template_name}"?`,
				response: async (r: boolean) => {
					if (r === true) {
						const list = getLocalComponentsByType(item.template_id, item.type).filter(
							(c) => !(c.global_id === item.global_id && c.data_type_id === item.data_type_id)
						);
						setLocalComponentsByType(item.template_id, item.type, list);
						onChangeHandler({ target: { id: 'local_search_components' } } as any);
						tableItems = buildTableItems();
						tableStore.set(tableItems);
					}
				}
			};
			modalStore.trigger(modalSettings);
		}
	}

	function applyChanges() {
		if (!currentItem || !currentTemplateId || !selectedGlobalComponent?.id || !selectedDataType?.id) return;

		const globalId = selectedGlobalComponent.id;
		const dataTypeId = selectedDataType.id;

		// Get the type of the global component
		const types: LocalComponentType[] = ['facets', 'categories', 'properties', 'general'];
		let globalComponentType: LocalComponentType = 'facets';
		for (const type of types) {
			const exists = searchConfigData?.global?.search_components?.[type]?.find(
				(c: any) => c.id === globalId
			);
			if (exists) {
				globalComponentType = type;
				break;
			}
		}

		const updatedComponent: LocalComponent = {
			global_id: globalId,
			data_type_id: dataTypeId,
			metadata_nodes: selectedMetadataNodes.map((n: any) => n?.id).filter((id: any) => id !== null)
		};

		const list = getLocalComponentsByType(currentTemplateId, globalComponentType);
		const updated = list.map((c) =>
			c.global_id === currentItem?.global_id && c.data_type_id === currentItem?.data_type_id ? updatedComponent : c
		);
		setLocalComponentsByType(currentTemplateId, globalComponentType, updated);
		onChangeHandler({ target: { id: 'local_search_components' } } as any);
		tableItems = buildTableItems();
		tableStore.set(tableItems);
		toggleForm();
	}

	function addComponent() {
		if (!currentTemplateId || !selectedGlobalComponent?.id || !selectedDataType?.id) return;

		const globalId = selectedGlobalComponent.id;
		const dataTypeId = selectedDataType.id;

		// Get the type of the global component
		const types: LocalComponentType[] = ['facets', 'categories', 'properties', 'general'];
		let globalComponentType: LocalComponentType = 'facets';
		for (const type of types) {
			const exists = searchConfigData?.global?.search_components?.[type]?.find(
				(c: any) => c.id === globalId
			);
			if (exists) {
				globalComponentType = type;
				break;
			}
		}

		const newItem: LocalComponent = {
			global_id: globalId,
			data_type_id: dataTypeId,
			metadata_nodes: selectedMetadataNodes.map((n: any) => n?.id).filter((id: any) => id !== null)
		};

		const list = getLocalComponentsByType(currentTemplateId, globalComponentType);
		setLocalComponentsByType(currentTemplateId, globalComponentType, [...list, newItem]);
		onChangeHandler({ target: { id: 'local_search_components' } } as any);
		tableItems = buildTableItems();
		tableStore.set(tableItems);
		toggleForm();
	}
</script>

<h2 class="text-xl font-semibold mb-4 pt-8">Local Settings</h2>

<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
	<div class="h4 h-9">
		{#if showForm}
			<span in:fade={{ delay: 400 }} out:fade>{isEditing ? 'Edit a Component' : 'Create a new Component'}</span>
		{:else}
			<span in:fade={{ delay: 400 }} out:fade>Local Component Definition</span>
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
		<div class="grid grid-cols-3 gap-4 my-4">
			<DropdownKVP
				id="entityTemplate"
				title="Entity Template"
				source={entityTemplatesSource}
				bind:target={selectedEntityTemplate}
				required={true}
				complexTarget={true}
				on:change={() => { currentTemplateId = selectedEntityTemplate?.id ?? null; }}
			/>
			<DropdownKVP
				id="globalComponent"
				title="Global Component"
				source={globalComponentsSource}
				bind:target={selectedGlobalComponent}
				required={true}
				complexTarget={true}
				on:change={() => {}}
			/>
			<DropdownKVP
				id="dataType"
				title="Data Type"
				source={dataTypeSource}
				bind:target={selectedDataType}
				required={true}
				complexTarget={true}
				on:change={() => {}}
			/>
		</div>

		<div class="grid grid-cols-1 gap-3 my-4">
			<div class="flex items-center justify-between">
				<label class="block text-sm font-medium">Metadata Nodes</label>
				<button
					type="button"
					class="btn btn-sm variant-filled-primary"
					title="Add Metadata Node"
					on:click={addMetadataNode}
				>
					<Fa icon={faPlus} /> Add
				</button>
			</div>
			{#each selectedMetadataNodes as node, index (index)}
				<div class="flex gap-2 items-end">
					<div class="flex-1">
						<DropdownKVP
							id="metadataNode_{index}"
							title="{index === 0 ? 'Metadata Node' : ''}"
							source={metadataNodesSource}
							bind:target={node}
							complexTarget={true}
							on:change={() => {}}
						/>
					</div>
					{#if selectedMetadataNodes.length > 1}
						<button
							type="button"
							class="btn btn-sm variant-filled-error"
							title="Delete Metadata Node"
							on:click={() => removeMetadataNode(index)}
						>
							<Fa icon={faXmark} />
						</button>
					{/if}
				</div>
			{/each}
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
				id: 'localSearchComponentsTable',
				data: tableStore,
				search: false,
				optionsComponent: tableOptionComponent,
				columns: {
					id: { exclude: true },
                    global_id: { exclude: true },
                    template_id: { exclude: true },
					template_name: { header: 'Template' },
					global_component_name: { header: 'Global Component' },
					data_type_id: { header: 'Data Type' },
					metadata_nodes: {
						header: 'Metadata Nodes',
						disableFiltering: true,
						instructions: {
							renderComponent: tableElementsComponent
						}
					}
				}
			}}
		/>
	{/if}
</div>

<section class="mt-6">
    <h3 class="text-lg font-semibold mb-2">Local Index Not Completed Metadata</h3>
    {#if !searchConfigData?.local}
        <p class="text-sm text-surface-500">No local configuration available.</p>
    {:else}
        <div class="grid grid-cols-2 gap-4">
            {#each searchConfigData.local as localCfg (localCfg.entity_template_id)}
                <div class="p-3 rounded-md border border-surface-200">
                    <div class="font-medium mb-2">{getTemplateName(localCfg.entity_template_id)}</div>
                    <SlideToggle
                        active="bg-secondary-500"
                        size="sm"
                        id={`index_not_completed_metadata_${localCfg.entity_template_id}`}
                        name="Index Not Completed Metadata"
                        bind:checked={localCfg.index_not_completed_metadata}
                        on:change={onChangeHandler}
                    >
                        Index Not Completed Metadata
                    </SlideToggle>
                </div>
            {/each}
        </div>
    {/if}
</section>

<Modal />
