<script lang="ts">
	import { DropdownKVP, NumberInput, TextInput } from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';
    import type { MeaningModel } from '$lib/components/SearchConfig/SearchConfigModel.ts';

	export let entitytemplates: any[] = [];
	export let meanings: MeaningModel[] = [];
	export let searchConfigData: any;
	export let onChangeHandler: (event: Event | CustomEvent<any>) => void;
	export let res: any;

	const ensureGlobalGeneral = () => {
		if (!searchConfigData?.global?.general) {
			searchConfigData.global.general = {};
		}
		return searchConfigData.global.general;
	};

	const ensureSpatialSearchSettings = () => {
		if (!searchConfigData?.global?.spatial_data) {
			searchConfigData.global.spatial_data = {};
		}
		if (!searchConfigData.global.spatial_data.spatial_search_settings) {
			searchConfigData.global.spatial_data.spatial_search_settings = {
				crs: 'EPSG:4326',
				axis_order: [],
				basemap: 'satellite',
				start_extend: ''
			};
		}
		return searchConfigData.global.spatial_data.spatial_search_settings;
	};

	const ensureGlobalPrimarySpatial = () => {
		if (!searchConfigData?.global?.primary_data) {
			searchConfigData.global.primary_data = { to_index: false };
		}
		if (!searchConfigData.global.primary_data.spatial_data) {
			searchConfigData.global.primary_data.spatial_data = {
				allowed_data_type_ids: [],
				lat_meaning: 0,
				long_meaning: 0
			};
		}
		return searchConfigData.global.primary_data.spatial_data;
	};

	const crsSource = [{ id: 'EPSG:4326', text: 'EPSG:4326', group: 'CRS' }];
	const basemapSource = [{ id: 'satellite', text: 'Satellite', group: 'Basemap' }];

	$: meaningsSource = Array.isArray(meanings)
		? meanings.map((m) => ({ id: m.id, text: m.name, group: 'Meanings' }))
		: [];

	let axisOrderText = '';
	let allowedDataTypeIdsText = '';
	let selectedCrs: any = null;
	let selectedBasemap: any = null;
	let selectedLatMeaning: any = null;
	let selectedLongMeaning: any = null;
	let globalGeneral: any = null;
	let primarySpatial: any = null;
	let spatialSettings: any = null;

	$: if (searchConfigData?.global) {
		spatialSettings = ensureSpatialSearchSettings();
		axisOrderText = Array.isArray(spatialSettings.axis_order)
			? spatialSettings.axis_order.join(', ')
			: '';
		selectedCrs = crsSource.find((c) => c.id === spatialSettings.crs) ?? crsSource[0];
		selectedBasemap = basemapSource.find((b) => b.id === spatialSettings.basemap) ?? basemapSource[0];

		primarySpatial = ensureGlobalPrimarySpatial();
		allowedDataTypeIdsText = Array.isArray(primarySpatial.allowed_data_type_ids)
			? primarySpatial.allowed_data_type_ids.join(', ')
			: '';
		selectedLatMeaning = meaningsSource.find((m) => m.id === primarySpatial.lat_meaning) ?? null;
		selectedLongMeaning = meaningsSource.find((m) => m.id === primarySpatial.long_meaning) ?? null;
	}

	$: if (searchConfigData?.global) {
		globalGeneral = ensureGlobalGeneral();
	}

	const getTemplateName = (templateId: number): string => {
		const fromList = entitytemplates?.find((et) => et.id === templateId);
		return fromList?.name ?? `Template #${templateId}`;
	};

	function updateAxisOrder() {
		const spatialSettings = ensureSpatialSearchSettings();
		spatialSettings.axis_order = axisOrderText
			.split(',')
			.map((v) => v.trim())
			.filter(Boolean);
		onChangeHandler({ target: { id: 'spatial_search_settings' } } as any);
	}

	function updateAllowedDataTypeIds() {
		
		const primarySpatial = ensureGlobalPrimarySpatial();
		primarySpatial.allowed_data_type_ids = allowedDataTypeIdsText
			.split(',')
			.map((v) => parseInt(v.trim(), 10))
			.filter((v) => !Number.isNaN(v));
		onChangeHandler({ target: { id: 'allowed_data_type_ids' } } as any);
	}

	function updateCrs() {
		const spatialSettings = ensureSpatialSearchSettings();
		spatialSettings.crs = selectedCrs?.id ?? 'EPSG:4326';
		onChangeHandler({ target: { id: 'spatial_search_settings' } } as any);
	}

	function updateBasemap() {
		const spatialSettings = ensureSpatialSearchSettings();
		spatialSettings.basemap = selectedBasemap?.id ?? 'satellite';
		onChangeHandler({ target: { id: 'spatial_search_settings' } } as any);
	}

	function updateLatMeaning() {
		const primarySpatial = ensureGlobalPrimarySpatial();
		primarySpatial.lat_meaning = selectedLatMeaning?.id ?? 0;
		onChangeHandler({ target: { id: 'lat_meaning' } } as any);
	}

	function updateLongMeaning() {
		const primarySpatial = ensureGlobalPrimarySpatial();
		primarySpatial.long_meaning = selectedLongMeaning?.id ?? 0;
		onChangeHandler({ target: { id: 'long_meaning' } } as any);
	}
</script>

<h4 class="text-xl font-semibold mb-4">Settings which are unclear or maybe better defined in Application settings</h4>

<section class="mb-6">
	<h3 class="text-lg font-semibold mb-2">Global General</h3>
	<div class="grid grid-cols-3 gap-4">
		<SlideToggle
			active="bg-secondary-500"
			size="sm"
			id="show_only_completed_metadata"
			name="Show Only Completed Metadata"
			bind:checked={globalGeneral.show_only_completed_metadata}
			on:change={onChangeHandler}>Show Only Completed Metadata</SlideToggle
		>
		<SlideToggle
			active="bg-secondary-500"
			size="sm"
			id="show_empty_facets"
			name="Show Empty Facets"
			bind:checked={globalGeneral.show_empty_facets}
			on:change={onChangeHandler}>Show Empty Facets</SlideToggle
		>
		<NumberInput
			id="auto_complete_trigger"
			label="Auto Complete Trigger"
			bind:value={globalGeneral.auto_complete_trigger}
			on:change={onChangeHandler}
		/>
	</div>
</section>

<section class="mb-6">
	<h3 class="text-lg font-semibold mb-2">Global Spatial Search Settings</h3>
	<div class="grid grid-cols-2 gap-4">
		<DropdownKVP
			id="crs"
			title="CRS"
			source={crsSource}
			bind:target={selectedCrs}
			required={true}
			complexTarget={true}
			on:change={updateCrs}
		/>
		<DropdownKVP
			id="basemap"
			title="Basemap"
			source={basemapSource}
			bind:target={selectedBasemap}
			required={true}
			complexTarget={true}
			on:change={updateBasemap}
		/>
		<TextInput
			id="axis_order"
			label="Axis Order (comma-separated)"
			bind:value={axisOrderText}
			on:change={updateAxisOrder}
		/>
		<TextInput
			id="start_extend"
			label="Start Extend"
			bind:value={spatialSettings.start_extend}
			on:change={onChangeHandler}
	/>
	</div>
</section>

<section class="mb-6">
	<h3 class="text-lg font-semibold mb-2">Global Primary Spatial Data</h3>
	<div class="grid grid-cols-2 gap-4">
		<TextInput
			id="allowed_data_type_ids"
			label="Allowed Data Type IDs (comma-separated)"
			bind:value={allowedDataTypeIdsText}
			on:change={(e) => updateAllowedDataTypeIds}
		/>
		<DropdownKVP
			id="lat_meaning"
			title="Latitude Meaning"
			source={meaningsSource}
			bind:target={selectedLatMeaning}
			complexTarget={true}
			on:change={updateLatMeaning}
		/>
		<DropdownKVP
			id="long_meaning"
			title="Longitude Meaning"
			source={meaningsSource}
			bind:target={selectedLongMeaning}
			complexTarget={true}
			on:change={updateLongMeaning}
		/>
	</div>
</section>
