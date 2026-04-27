<script lang="ts">
	import {
		Table,
		TablePlaceholder,
		DropdownKVP,
		NumberInput, 
	} from '@bexis2/bexis2-core-ui';
	import TableElements from '$lib/components/SearchConfig/tableElements.svelte';
	import TableOption from '$lib/components/SearchConfig/tableOptions.svelte';
	import { writable, type Writable } from 'svelte/store';
	import { getModalStore, Modal, type ModalSettings } from '@skeletonlabs/skeleton';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faPlus, faSave, faXmark } from '@fortawesome/free-solid-svg-icons';
	import { fade, slide } from 'svelte/transition';
	import suite from './spatialDataValidation';
	import { onMount, tick } from 'svelte';
  
	const tableOptionComponent = TableOption as any;
	const tableElementsComponent = TableElements as any;
	const modalStore = getModalStore();

	import type { MeaningModel, LocalSpatialMetadata, BBoxSpatialMetadata, PointSpatialMetadata } from '$lib/components/SearchConfig/SearchConfigModel.ts';
	import { getMetadataNodes } from '$lib/services/searchConfigServices';

	interface SpatialDataListItem {
		id: string;
		template_name: string;
		type: 'bbox' | 'point';
		metadata_nodes: string[];
		config: LocalSpatialMetadata;
	}

	export let entitytemplates: any[] = [];
	export let meanings: MeaningModel[] = [];
	export let searchConfigData: any;
	export let onChangeHandler: (event: Event | CustomEvent<any>) => void;
	export let res: any;
	// local validation result for the primary data form
	let resValidation: any = suite.get();

	let loading: boolean = false;

	let metadataNodes: any[] = [];
	getMetadataNodes().then((nodes) => {
		metadataNodes = nodes;
	});

	const tableStore: Writable<any[]> = writable([]);

	let currentItem: SpatialDataListItem | null = null;
	let isEditing: boolean = false;

	onMount(async () => {
		// reset suite and initialize with an empty item so resValidation is valid
		suite.reset();
		const initial: SpatialDataListItem = {
			id: '',
			template_name: '',
			type: 'bbox',
			metadata_nodes: [],
			config: { type: 'bbox', WestBoundLongitude: "", EastBoundLongitude: "", SouthBoundLatitude: "", NorthBoundLatitude: "" }
		};
		resValidation = suite(initial);
	});

	// change event: run validation on current PrimaryData form state
	async function onChangeHandlerPrimaryData(e: any, fieldName: string) {
		// wait for Svelte bind:target updates (selected* vars) to flush
		await tick();
		
		console.log('After tick - fieldName:', fieldName);
		console.log('After tick - bboxWestNode:', bboxWestNode);
		console.log('After tick - event:', e);
		console.log('After tick - event.detail:', e?.detail);

		setTimeout(async () => {
			console.log('input changed', e);
			console.log('fieldName', fieldName);
			console.log('currentItem', currentItem);
			console.log('Bbox nodes:', bboxWestNode, bboxEastNode, bboxSouthNode, bboxNorthNode);
			
			const configForValidation: LocalSpatialMetadata = selectedSpatialType === 'bbox'
				? {
						type: 'bbox',
						WestBoundLongitude: nodeToId(bboxWestNode),
						EastBoundLongitude: nodeToId(bboxEastNode),
						SouthBoundLatitude: nodeToId(bboxSouthNode),
						NorthBoundLatitude: nodeToId(bboxNorthNode)
					}
				: {
						type: 'point',
						longitude: nodeToId(pointLongitudeNode),
						latitude: nodeToId(pointLatitudeNode),
						radius: parseFloat(pointRadiusValue)
					};

			// keep the current item in sync while editing
			if (currentItem) {
				currentItem = {
					...currentItem,
					type: selectedSpatialType,
					config: configForValidation
				};
			}

			// Build the data object for validation from current form state,
			// regardless of whether we are editing an existing item or creating a new one.
			const dataForValidation: SpatialDataListItem = {
				id: currentItem?.id ?? '',
				template_name:
					currentItem?.template_name ??
					(selectedEntityTemplate ? selectedEntityTemplate.id.toString() : ''),
				type: (currentItem?.type ?? selectedSpatialType ?? 'bbox') as any,
				metadata_nodes: selectedMetadataNodes,
				config: configForValidation
			};
			console.log('dataForValidation', dataForValidation);

			// reset Vest state so old errors don't linger between runs
			suite.reset();
			resValidation = suite(dataForValidation);
			console.log('res after change', resValidation);
		}, 100);
	}

	function editSpatialData(type: any) {
		if (type.action == 'edit') {
			currentItem = spatialData.find((item) => item.id === type.id) ?? null;
			console.log('currentItem', currentItem);
			if (currentItem) {
				didHydrateFromItem = false;
				selectedMetadataNodes = currentItem.metadata_nodes;
				selectedSpatialType = currentItem.type;
				updateCoordinatesFromCurrentItem();
				if (currentItem?.template_name === 'global') {
					selectedEntityTemplate = { id: 'global', name: 'global' };
				} else {
					selectedEntityTemplate =
						entitytemplates.find((et) => et.id.toString() === currentItem?.template_name) ?? null;
				}
			}
			isEditing = true;
			showForm = true;
			console.log('edit spatial data', type.id);
		}
		if (type.action == 'delete') {
			console.log('delete spatial data', type.id);
			let item: SpatialDataListItem = spatialData.find((item) => item.id === type.id)!;
			const modalSettings: ModalSettings = {
				type: 'confirm',
				title: 'Delete Spatial Data Configuration',
				body:
					'Are you sure you wish to delete Spatial Data Configuration with Type "' +
					item.type +
					'" for Entity Template "' +
					item.template_name +
					'"?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: async (r: boolean) => {
					if (r === true) {
						// remove from spatialData (table view)
						spatialData = spatialData.filter((sd) => sd.id !== item.id);
						console.log('Updated spatialData after deletion:', spatialData);
						// propagate deletion into underlying searchConfigData structure
						if (Array.isArray(searchConfigData?.local)) {
							searchConfigData.local.forEach((localCfg: any) => {
								if (
									!localCfg.spatial_data &&
									localCfg.entity_template_id.toString() !== item.template_name
								)
									return;
								if (localCfg.spatial_data?.spatial_metadata) {
									localCfg.spatial_data.spatial_metadata = undefined;
								}
							});
						}
						// notify parent so validation can run for this logical field
						onChangeHandler({ target: { id: 'spatial_metadata' } } as any);
					}
				}
			};
			modalStore.trigger(modalSettings);
		}
	}
    


	let spatialData: SpatialDataListItem[] = [];

	// create reactive statement to build table data from ALL local entries
	$: {
		const items: SpatialDataListItem[] = [];
		console.log('searchConfigData', searchConfigData);

		if (Array.isArray(searchConfigData?.local)) {
			searchConfigData.local.forEach((localCfg: any) => {
				const spatialMetadata = localCfg.spatial_data?.spatial_metadata;
				if (!spatialMetadata) return;

				items.push({
					id: 'local_' + localCfg.entity_template_id.toString(),
					template_name: localCfg.entity_template_id.toString(),
					type: spatialMetadata.type,
					metadata_nodes: getMetadataNodeNamesFromConfig(spatialMetadata),
					config: spatialMetadata
				});
			});
		}

		spatialData = items;
		console.log('spatialData', spatialData);
		tableStore.set(spatialData);
	}

	$: tableStore.set(spatialData);

	let showForm = false;
	let selectedMetadataNodes: string[] = [];
	let selectedEntityTemplate: any = null;
	let selectedSpatialType: 'bbox' | 'point' = 'bbox';
	let didHydrateFromItem: boolean = false;
	
	// Spatial coordinate inputs
	// Spatial coordinate inputs - metadata nodes
	let bboxWestNode: any = null;
	let bboxEastNode: any = null;
	let bboxSouthNode: any = null;
	let bboxNorthNode: any = null;
	let pointLongitudeNode: any = null;
	let pointLatitudeNode: any = null;
	let pointRadiusValue: string = "0";

	// adapt entity templates to DropdownKVP's expected shape (id, text, group)
	$: entitytemplatesSource = Array.isArray(entitytemplates)
		? entitytemplates.map((et) => ({
				...et,
				text: et.name,
				group: 'Entity Templates'
			}))
		: [];

	let spatialTypeSource: any[] = [
		{ id: 'bbox', text: 'Bounding Box', group: 'Spatial Types' },
		{ id: 'point', text: 'Point with Radius', group: 'Spatial Types' }
	];

	let metadataNodesSource: any[] = [];
	$: metadataNodesSource = Array.isArray(metadataNodes)
		? metadataNodes.map((node) => ({
				id: node.xPath ?? node.id,
				text: node.displayName || node.id,
				group: 'Metadata Nodes'
			}))
		: [];

	const nodeToId = (node: any): string => {
		if (node == null) return '';
		if (typeof node === 'string') return node;
		return (node.id ?? node.value ?? node.key ?? '') as string;
	};

	const resolveNodeById = (id: string | undefined | null): any => {
		if (!id) return null;
		return (
			metadataNodesSource.find((n) => n.id === id) ??
			{ id: id, text: id, value: id, label: id, group: 'Metadata Nodes' }
		);
	};

	const getMetadataNodeNamesFromConfig = (config: LocalSpatialMetadata): string[] => {
		const paths: string[] = [];
		if (config.type === 'bbox' && 'WestBoundLongitude' in config) {
			if (config.WestBoundLongitude) paths.push(config.WestBoundLongitude);
			if (config.EastBoundLongitude) paths.push(config.EastBoundLongitude);
			if (config.SouthBoundLatitude) paths.push(config.SouthBoundLatitude);
			if (config.NorthBoundLatitude) paths.push(config.NorthBoundLatitude);
		} else if (config.type === 'point' && 'longitude' in config) {
			if (config.longitude) paths.push(config.longitude);
			if (config.latitude) paths.push(config.latitude);
			if (config.radius) paths.push(config.radius.toString());
		}
		
console.log(paths);

		// Resolve xPaths to display names
		return paths.map(path => {
			const node = metadataNodesSource.find(n => n.id === path);
			return node?.text ?? path;
		});
	};

	function toggleForm() {
		if (showForm) {
			clear();
		}

		showForm = !showForm;
	}

	function updateCoordinatesFromCurrentItem() {
		if (!currentItem) return;
		
		if (currentItem.type === 'bbox' && 'WestBoundLongitude' in currentItem.config) {
			bboxWestNode = resolveNodeById(currentItem.config.WestBoundLongitude ?? '');
			bboxEastNode = resolveNodeById(currentItem.config.EastBoundLongitude ?? '');
			bboxSouthNode = resolveNodeById(currentItem.config.SouthBoundLatitude ?? '');
			bboxNorthNode = resolveNodeById(currentItem.config.NorthBoundLatitude ?? '');
		} else if (currentItem.type === 'point' && 'longitude' in currentItem.config) {
			pointLongitudeNode = resolveNodeById(currentItem.config.longitude ?? '');
			pointLatitudeNode = resolveNodeById(currentItem.config.latitude ?? '');
			pointRadiusValue = currentItem.config.radius?.toString() ?? "0";
		}
	}

	$: if (isEditing && showForm && currentItem && metadataNodesSource.length && !didHydrateFromItem) {
		updateCoordinatesFromCurrentItem();
		didHydrateFromItem = true;
	}

	function applyChanges() {
		console.log('Applying changes with selected metadata nodes:', selectedMetadataNodes);
		console.log('Current Item being edited:', currentItem);

		if (!currentItem) return;

		// update the item in spatialData (table view)
		spatialData = spatialData.map((item) => {
			if (item.id === currentItem?.id) {
				const updatedConfig: LocalSpatialMetadata = selectedSpatialType === 'bbox'
					? { type: 'bbox', WestBoundLongitude: nodeToId(bboxWestNode), EastBoundLongitude: nodeToId(bboxEastNode), SouthBoundLatitude: nodeToId(bboxSouthNode), NorthBoundLatitude: nodeToId(bboxNorthNode) }
					: { type: 'point', longitude: nodeToId(pointLongitudeNode), latitude: nodeToId(pointLatitudeNode), radius: parseFloat(pointRadiusValue) };
				
				return {
					...item,
					metadata_nodes: selectedMetadataNodes,
					config: updatedConfig
				};
			}
			return item;
		});
		console.log('Updated spatialData:', spatialData);

		// propagate changes into underlying searchConfigData structure
		if (Array.isArray(searchConfigData?.local) && currentItem.id.startsWith('local_')) {
			const templateId = currentItem.template_name;
			searchConfigData.local.forEach((localCfg: any) => {
				if (localCfg.entity_template_id.toString() === templateId) {
					if (!localCfg.spatial_data) {
						localCfg.spatial_data = {};
					}
					const updatedConfig: LocalSpatialMetadata = selectedSpatialType === 'bbox'
						? { type: 'bbox', WestBoundLongitude: nodeToId(bboxWestNode), EastBoundLongitude: nodeToId(bboxEastNode), SouthBoundLatitude: nodeToId(bboxSouthNode), NorthBoundLatitude: nodeToId(bboxNorthNode) }
						: { type: 'point', longitude: nodeToId(pointLongitudeNode), latitude: nodeToId(pointLatitudeNode), radius: parseFloat(pointRadiusValue) };
					
					localCfg.spatial_data.spatial_metadata = updatedConfig;
				}
			});
		}

		// notify parent so validation can run for this logical field
		onChangeHandler({ target: { id: 'spatial_metadata' } } as any);

		toggleForm();
	}

	function addSpatialData() {
		console.log(
			'Adding new spatial data configuration with type:',
			selectedSpatialType,
			'and metadata nodes:',
			selectedMetadataNodes
		);

		// create the appropriate spatial metadata object based on type
		const spatialMetadata: LocalSpatialMetadata = selectedSpatialType === 'bbox'
			? {
					type: 'bbox',
					WestBoundLongitude: nodeToId(bboxWestNode),
					EastBoundLongitude: nodeToId(bboxEastNode),
					SouthBoundLatitude: nodeToId(bboxSouthNode),
					NorthBoundLatitude: nodeToId(bboxNorthNode)
				}
			: {
					type: 'point',
					longitude: nodeToId(pointLongitudeNode),
					latitude: nodeToId(pointLatitudeNode),
					radius: parseFloat(pointRadiusValue)
				};

		// create new spatial data item
		const newItem: SpatialDataListItem = {
			id: 'local_' + selectedEntityTemplate.id.toString() + '_' + Date.now(),
			template_name: selectedEntityTemplate.id.toString(),
			type: selectedSpatialType,
			metadata_nodes: selectedMetadataNodes,
			config: spatialMetadata
		};

		// add to spatialData (table view)
		spatialData = [...spatialData, newItem];
		console.log('Updated spatialData:', spatialData);

		// propagate changes into underlying searchConfigData structure
		if (Array.isArray(searchConfigData?.local)) {
			let localCfg = searchConfigData.local.find(
				(cfg: any) => cfg.entity_template_id.toString() === selectedEntityTemplate.id.toString()
			);
			if (!localCfg) {
				// create new local config if not existing
				localCfg = {
					entity_template_id: selectedEntityTemplate.id,
					index_not_completed_metadata: false,
					search_components: {},
					spatial_data: {},
					primary_data: { to_index: false },
					external_sources: { source: '', local_path: '', external_name: '' }
				};
				searchConfigData.local.push(localCfg);
			}
			if (!localCfg.spatial_data) {
				localCfg.spatial_data = {};
			}
			localCfg.spatial_data.spatial_metadata = spatialMetadata;
		}

		// notify parent so validation can run for this logical field
		onChangeHandler({ target: { id: 'spatial_metadata' } } as any);
		toggleForm();
	}

	function onCancel() {
		toggleForm();
		clear();
	}

	function clear() {
		selectedMetadataNodes = [];
		selectedEntityTemplate = null;
		selectedSpatialType = 'bbox';
		currentItem = null;
		isEditing = false;
		didHydrateFromItem = false;
		bboxWestNode = null;
		bboxEastNode = null;
		bboxSouthNode = null;
		bboxNorthNode = null;
		pointLongitudeNode = null;
		pointLatitudeNode = null;
		pointRadiusValue = "0";
	}
</script>

<h2 class="text-xl font-semibold mb-4">Spatial Search Configuration</h2>

<div>
	<SlideToggle
		active="bg-secondary-500"
		size="sm"
		id="spatial_search"
		name="Enable Spatial Search"
		bind:checked={searchConfigData.global.spatial_data.spatial_search}
		on:change={onChangeHandler}>Enable Spatial Search</SlideToggle
	>
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
						<div><b>Spatial Type:</b> {selectedSpatialType === 'bbox' ? 'Bounding Box' : 'Point with Radius'}</div>
					</div>
				{:else}
					<div class="grow w-1/2">
						<DropdownKVP
							id="entityTemplate"
							title="Entity Template"
							bind:target={selectedEntityTemplate}
							source={entitytemplatesSource.filter(
								(et) =>
									!spatialData.some(
										(item) => item.template_name.toString() === et.id.toString()
									)
							)}
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
							id="spatialType"
							title="Spatial Type"
							bind:target={selectedSpatialType}
							source={spatialTypeSource}
							required={true}
							complexTarget={false}
							help={true}
							invalid={resValidation?.hasErrors('spatialType')}
							feedback={resValidation?.getErrors('spatialType')}
							on:change={(e) => onChangeHandlerPrimaryData(e, 'spatialType')}
						/>
					</div>
				{/if}
			</div>
			
			{#if selectedSpatialType === 'bbox'}
				<div class="grid grid-cols-2 gap-4 my-4">
					<DropdownKVP
						id="bboxWest"
						title="West Bound Longitude"
						source={metadataNodesSource}
						bind:target={bboxWestNode}
						complexTarget={true}
						on:change={(e) => onChangeHandlerPrimaryData(e, 'bboxWest')}
					/>
					<DropdownKVP
						id="bboxEast"
						title="East Bound Longitude"
						source={metadataNodesSource}
						bind:target={bboxEastNode}
						complexTarget={true}
						on:change={(e) => onChangeHandlerPrimaryData(e, 'bboxEast')}
					/>
					<DropdownKVP
						id="bboxSouth"
						title="South Bound Latitude"
						source={metadataNodesSource}
						bind:target={bboxSouthNode}
						complexTarget={true}
						on:change={(e) => onChangeHandlerPrimaryData(e, 'bboxSouth')}
					/>
					<DropdownKVP
						id="bboxNorth"
						title="North Bound Latitude"
						source={metadataNodesSource}
						bind:target={bboxNorthNode}
						complexTarget={true}
						on:change={(e) => onChangeHandlerPrimaryData(e, 'bboxNorth')}
					/>
				</div>
			{:else if selectedSpatialType === 'point'}
				<div class="grid grid-cols-3 gap-4 my-4">
					<DropdownKVP
						id="pointLongitude"
						title="Longitude"
						source={metadataNodesSource}
						bind:target={pointLongitudeNode}
						complexTarget={true}
						on:change={(e) => onChangeHandlerPrimaryData(e, 'pointLongitude')}
					/>
					<DropdownKVP
						id="pointLatitude"
						title="Latitude"
						source={metadataNodesSource}
						bind:target={pointLatitudeNode}
						complexTarget={true}
						on:change={(e) => onChangeHandlerPrimaryData(e, 'pointLatitude')}
					/>
					<!--@ts-ignore-->
					<NumberInput
						id="pointRadiusValue"
						label="Radius Value"
						bind:value={pointRadiusValue}  
						on:change={(e) => onChangeHandlerPrimaryData(e, 'pointRadiusValue')}
					/>
	
				</div>
			{/if}
			
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
						on:click={addSpatialData}
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
			on:action={(obj) => editSpatialData(obj.detail.type)}
			config={{
				id: 'spatialDataTable',
				data: tableStore,
				search: false,
				optionsComponent: tableOptionComponent,
				columns: {
					id: {
						exclude: true
					},
					config: {
						exclude: true
					},
					template_name: {
						header: 'Entity Template',
						instructions: {
							toStringFn: (key) => {
								const et = entitytemplates.find((item) => item.id.toString() === key.toString());
								if (et) {
									return et.name;
								} else {
									return key.toString();
								}
							}
						}
					},
					type: {
						header: 'Spatial Type',
						instructions: {
							toStringFn: (key) => key === 'bbox' ? 'Bounding Box' : 'Point with Radius'
						}
					},
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
	</div>
{/if}
<Modal />
