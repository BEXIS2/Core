<script lang="ts">
	import { getConfigStore, getLabelByPath, getValueByPath, hideDescriptionHandler, showDescriptionHandler, updateValidationState} from '$lib/components/utils/metadata/metadataComponentUtils';

	import SimpleComponent from '$lib/components/metadata/simpleComponent.svelte';
	import { metadataStore } from '$lib/components/utils/metadata/stores';

	import { onMount, createEventDispatcher } from 'svelte';
	import { customComponentsCatalog } from '$lib/components/customComponents/componentCatalog';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';

	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;
	export	let isMulti: boolean = false;

	let value = getValueByPath(path);
	let label = getLabelByPath(path);

	metadataStore.subscribe(() => {
		//console.log("metadataStore subscribe in simpleComponentWrapper.svelte:", path, value)
		value = getValueByPath(path);
		//const res = suite(path);
		// setTimeout(async () => {
		// 	updateValidationState(path, res);
		// 	dispatch('updated');
		// }, 2000);
	});



 let config: any;
	let isAnchor: boolean = false;
	let isVisible: boolean = true;
	let customComponent: any;
// dispatcher to forward events to parent components
const dispatch = createEventDispatcher();

	onMount(async () => {

		config = getConfigStore();

		console.log('[simpleComponentWrapper] path:', path, 'config:', config);

		if (!config?.components) {
			console.log('[simpleComponentWrapper] no config.components, skipping');
			return;
		}

		for (const component of config.components) {

			// strip array indices from path (e.g. "A.B.0.C" -> "A.B.C") for anchorpoint matching
			let pathWithoutIndices = path.split('.').filter(p => isNaN(Number(p))).join('.');
			console.log('[simpleComponentWrapper] checking anchor:', component.globalSettings.anchorpoint, 'vs path:', path, 'pathWithoutIndices:', pathWithoutIndices);
			if (component.globalSettings.anchorpoint == path || component.globalSettings.anchorpoint == pathWithoutIndices) {
				isAnchor = true;
				let componentName = component.meta.component_name;
				console.log('[simpleComponentWrapper] MATCH! anchor:', component.globalSettings.anchorpoint, 'component:', componentName, 'in catalog:', !!customComponentsCatalog[componentName]);
				customComponent = customComponentsCatalog[componentName]?.component;
				if (!customComponent) {
					console.warn('[simpleComponentWrapper] component not found in catalog:', componentName);
				}
			} 
			for (const variable of component.mode.variables.variable) {

				if ((variable.JSONPath == path || variable.JSONPath == pathWithoutIndices) && variable.is_visible == false) {
					isVisible = false;
				}	
			}
		}

		console.log('[simpleComponentWrapper] result for path:', path, 'isAnchor:', isAnchor, 'isVisible:', isVisible, 'customComponent:', !!customComponent);

	})


	function handleShowDescription(e: CustomEvent<any>) {
		showDescriptionHandler(e, 'simple');
	}

	function handleHideDescription(e: CustomEvent<any>) {
		hideDescriptionHandler(e, 'simple');
	}
	

	// in case the custom component fails to load, we can use a fallback component
	let useFallback = false;
	function handleFallback(e) {
		useFallback = true;
	}

	
</script>

{#if path && simpleComponent.properties}
 {#if isVisible && !isAnchor}
			<SimpleComponent 
			{simpleComponent} 
			{path} 
			{required} 
			{label} 
			bind:value={value} 
			on:updated
			{isMulti} 
			/>

	{:else if isAnchor && !useFallback}
		<div class="pr-2" id={path}>
		<svelte:component this={customComponent} anchor={path}
						on:showDescription={handleShowDescription}
						on:hideDescription={handleHideDescription}
						path={path}
						mode="edit"
						on:updated
						on:fallback={handleFallback}
					/>
		</div>
	{/if}
{/if}

