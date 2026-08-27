<script lang="ts">

	import { getConfigStore, getValueByPath, hideDescriptionHandler, showDescriptionHandler } from '$lib/components/utils/metadata/metadataComponentUtils';

	import SimpleComponent from './simpleComponent.svelte';
	import { onMount } from 'svelte';
	import { customComponentsCatalog } from '$lib/components/customComponents/componentCatalog';

	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;
	export let backgroundClass: string = '';

	let label: string = !path
		? ''
		: path.split('.').length > 1
			? path.split('.')[path.split('.').length - 1]
			: path;

	let value = getValueByPath(path);

	let config: any;
	let isAnchor: boolean = false;
	let isVisible: boolean = true;
	let customComponent: any;

	onMount(async () => {
		config = getConfigStore();

		if (!config?.components) return;

		for (const component of config.components) {
			let pathWithoutIndices = path.split('.').filter(p => isNaN(Number(p))).join('.');
			if (
				component.globalSettings.anchorpoint == path ||
				component.globalSettings.anchorpoint == pathWithoutIndices
			) {
				isAnchor = true;
				customComponent = customComponentsCatalog[component.meta.component_name].component;
			}

			for (const variable of component.mode.variables.variable) {
				if ((variable.JSONPath == path || variable.JSONPath == pathWithoutIndices) && variable.is_visible == false) {
					isVisible = false;
				}
			}
		}
	});

	function handleShowDescription(e: CustomEvent<any>) {
		showDescriptionHandler(e, 'simple');
	}

	function handleHideDescription(e: CustomEvent<any>) {
		hideDescriptionHandler(e, 'simple');
	}
</script>

{#if path && simpleComponent.properties}
	{#if isVisible && !isAnchor}
		<SimpleComponent {simpleComponent} {path} {required} {label} {value} {backgroundClass} />
	{:else if isAnchor}
		<div id={path}>
			<svelte:component
				this={customComponent}
				anchor={path}
				path={path}
				mode="view"
				on:showDescription={handleShowDescription}
				on:hideDescription={handleHideDescription}
			/>
		</div>
	{/if}
{/if}
