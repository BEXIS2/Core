<script lang="ts">
	import { onMount } from 'svelte';
	import {
		getValueByPath,
		updateMetadataStore,
		getVariableSoursePathFromConfig,
		getRefByPath,
		getFullConfig,
		getTargetVariablesWithValues
	} from '../../utils/metadata/metadataComponentUtils';
	import * as ts4nfdiWidgets from '@ts4nfdi/terminology-service-suite-js';
	import {convertDisplayName} from '../../../../routes/metadata/metadataShared';

	const controller = 'search';
	let containerElement: HTMLDivElement;
	let data: Array<{ iri: string; label: string }> = [];

	let componentName: string = 'terminology_v2.2.26';

	export let label: string;
	export let anchor: string;

	let date: Date = new Date();
	let valuePath: string = getVariableSoursePathFromConfig(componentName, anchor, 'value');
	console.log('Value Path:', valuePath);
	try {
		console.log('Getting initial value for path:', valuePath);
	} catch (error) {
		console.error('Error getting initial value for path:', error);
	}
	let value: any = getValueByPath(valuePath);
	let ref: any = getRefByPath(valuePath);
	let config = getFullConfig(componentName, anchor);
	let targetVar = getTargetVariablesWithValues(config)
	
	console.log("target", targetVar, config, componentName, anchor)

	// get value for "initViewExtent" from targetVar  { target_variable: string; value: string }
	let initViewExtent = targetVar?.find(v => v.target_variable === 'initViewExtent')?.value ?? '';

	console.log('Initial Value:', value);
	console.log('Initial Ref:', ref);

	// update metadata store on value change
	$: if (value || ref) {
		console.log('Updating metadata store with value:', value, 'and ref:', ref);
		updateMetadataStore(
			valuePath,
			value != undefined && value != null ? value.toString() : '',
			ref != undefined && ref != null ? ref.toString() : ''
		);
	}

	let preselectedItems: Array<{ iri: string; label: string }> = [];
	if (value && ref) preselectedItems = [{ iri: ref.toString(), label: value.toString() }];
	console.log('Preselected Items:', preselectedItems);

	onMount(async () => {
		console.log('Container element:', containerElement);

		if (containerElement) {
			console.log('Rendering TS4NFDI AutocompleteWidget...');

			try {
				ts4nfdiWidgets.createAutocomplete(
					{
						api: 'https://semanticlookup.zbmed.de/api/',
						selectionChangedEvent: (props) => {
							value = props.map((item) => item.label).toString();
							ref = props.map((item) => item.iri).toString();
							data = props;
							console.log('Autocomplete selection changed:', data);
						},
						parameter: initViewExtent,
						placeholder: 'Select a term within pre-selected ontologies ..',
						singleSelection: true,
						preselected: preselectedItems
					},
					containerElement
				);

				console.log('TS4NFDI AutocompleteWidget rendered.');
			} catch (error) {
				console.error('Error creating autocomplete widget:', error);
			}
		} else {
			console.error('Autocomplete widget container not found!');
		}
	});
</script>

<div class="module-output-container">
	<p>{convertDisplayName(label)}</p>
</div>

<div id="tswidget" bind:this={containerElement}></div>
{#if data}
	<ul>
		{#each data as item}
			<li>({item.iri})</li>
		{/each}
	</ul>
{/if}

<style>
	:global(#tswidget input) {
		flex-grow: 1;
		padding-left: 10px;
	}
</style>
