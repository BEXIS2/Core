<script lang="ts">
	import PrimaryData from '$lib/components/SearchConfig/PrimaryData.svelte';
	import SpatialData from '$lib/components/SearchConfig/SpatialData.svelte';
	import type { MeaningModel } from './SearchConfigModel';

	import ComponentsGlobal from '$lib/components/SearchConfig/ComponentsGlobal.svelte';
	import ComponentsLocal from '$lib/components/SearchConfig/ComponentsLocal.svelte';
	import ExternalSources from '$lib/components/SearchConfig/ExternalSources.svelte';
	import MissingSettings from '$lib/components/SearchConfig/MissingSettings.svelte';

	export let entitytemplates: any[] = [];
	export let meanings: MeaningModel[] = [];
	export let searchConfigData: any;
	export let onChangeHandler: (event: Event | CustomEvent<any>) => void;
	export let res: any;

	let activeTab = 'primary';
</script>

<div class="flex gap-2 border-b border-gray-300 mb-4">
	<button
		type="button"
		class="px-4 py-2 border-b-2 border-transparent transition-colors"
		class:border-blue-600={activeTab === 'components'}
		class:text-blue-600={activeTab === 'components'}
		class:font-semibold={activeTab === 'components'}
		on:click={() => (activeTab = 'components')}>Search Components</button
	>
	<button
		type="button"
		class="px-4 py-2 border-b-2 border-transparent transition-colors"
		class:border-blue-600={activeTab === 'spatial'}
		class:text-blue-600={activeTab === 'spatial'}
		class:font-semibold={activeTab === 'spatial'}
		on:click={() => (activeTab = 'spatial')}>Spatial Data</button
	>
	<button
		type="button"
		class="px-4 py-2 border-b-2 border-transparent transition-colors"
		class:border-blue-600={activeTab === 'primary'}
		class:text-blue-600={activeTab === 'primary'}
		class:font-semibold={activeTab === 'primary'}
		on:click={() => (activeTab = 'primary')}>Primary Data</button
	>
	<button
		type="button"
		class="px-4 py-2 border-b-2 border-transparent transition-colors"
		class:border-blue-600={activeTab === 'missing'}
		class:text-blue-600={activeTab === 'missing'}
		class:font-semibold={activeTab === 'missing'}
		on:click={() => (activeTab = 'missing')}>Unclear Settings</button
	>
	<button
		type="button"
		class="px-4 py-2 border-b-2 border-transparent transition-colors"
		class:border-blue-600={activeTab === 'external'}
		class:text-blue-600={activeTab === 'external'}
		class:font-semibold={activeTab === 'external'}
		on:click={() => (activeTab = 'external')}>External Sources</button
	>
</div>

<div class="p-4">
	{#if activeTab === 'primary'}
		<PrimaryData {entitytemplates} {meanings} bind:searchConfigData {res} {onChangeHandler} />
	{:else if activeTab === 'spatial'}
		<SpatialData {entitytemplates} {meanings} bind:searchConfigData {res} {onChangeHandler} />
	{:else if activeTab === 'components'}
		<ComponentsGlobal {entitytemplates} {meanings} bind:searchConfigData {res} {onChangeHandler} />
		<ComponentsLocal {entitytemplates} bind:searchConfigData {onChangeHandler} />
	{:else if activeTab === 'missing'}
		<MissingSettings {entitytemplates} {meanings} bind:searchConfigData {res} {onChangeHandler} />
	{:else if activeTab === 'external'}
		<ExternalSources {entitytemplates}  bind:searchConfigData {res} {onChangeHandler} />
	{/if}
</div>
