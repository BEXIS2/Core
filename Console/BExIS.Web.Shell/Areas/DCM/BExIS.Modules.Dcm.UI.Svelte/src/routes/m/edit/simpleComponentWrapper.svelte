<script lang="ts">
	import { getConfigStore, getValueByPath, hideDescriptionHandler, showDescriptionHandler} from '$lib/components/utils/metadata/metadataComponentUtils';

	import SimpleComponent from './simpleComponent.svelte';
	import { metadataStore } from '$lib/components/utils/metadata/stores';
	import type { MappingComponentConfig } from '$lib/components/utils/metadata/models';
	import { onMount } from 'svelte';
	import { getMappingComponentConfig } from '$lib/components/utils/metadata/mappingHelper';
	import { customComponentsCatalog } from '$lib/components/customComponents/componentCatalog';

	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;
	export	let isMulti: boolean = false;

	let label: string = !path
		? ''
		: path.split('.').length > 1
			? path.split('.')[path.split('.').length - 1]
			: path;

	let value = getValueByPath(path);

	metadataStore.subscribe(() => {
		value = getValueByPath(path);
		//console.log('value updated', value);
	});



 let config: any;
	let isAnchor: boolean = false;
	let isVisible: boolean = true;
	let customComponent: any;

	onMount(async () => {

		config = getConfigStore();

				// check if this component is an anchor point
		//console.log("check for anchorpoin", config)
		for (const component of config.components) {
			console.log("ghjgJ", component.globalSettings.anchorpoint, path)
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
			<SimpleComponent {simpleComponent} {path} {required} {label} {value} on:reload {isMulti} />
	{:else if isAnchor}
		<div class="pr-2" id={path}>
			<svelte:component this={customComponent} anchor={path}
							on:showDescription={handleShowDescription}
							on:hideDescription={handleHideDescription}
							path={path}
						/>
		</div>
	{/if}
{/if}

