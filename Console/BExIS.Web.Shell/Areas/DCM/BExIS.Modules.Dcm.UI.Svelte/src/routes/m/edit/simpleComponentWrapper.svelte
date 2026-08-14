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

				// check if this component is an anchor point
		//console.log("check for anchorpoin", config)
		for (const component of config.components) {

			// check if path is array which is indicated if the last part after the point is a number
			let isPathArray = path.includes('.') && !isNaN(Number(path.split('.').pop()));

			if (component.globalSettings.anchorpoint == path || (isPathArray && component.globalSettings.anchorpoint == path.split('.').slice(0, -1).join('.'))) {
				isAnchor = true;
				customComponent = customComponentsCatalog[component.meta.component_name].component;
			} 
			for (const variable of component.mode.variables.variable) {

				if (variable.JSONPath == path && variable.is_visible == false) {
					isVisible = false;
				}	
			}
		}


	})


	function handleShowDescription(e: CustomEvent<any>) {
		showDescriptionHandler(e, 'simple');
	}

	function handleHideDescription(e: CustomEvent<any>) {
		hideDescriptionHandler(e, 'simple');
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

	{:else if isAnchor}
		<div class="pr-2" id={path}>
			<svelte:component this={customComponent} anchor={path}
							on:showDescription={handleShowDescription}
							on:hideDescription={handleHideDescription}
							path={path}
							on:updated
						/>
		</div>
	{/if}
{/if}

