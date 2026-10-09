<script lang="ts">
	import SimpleComponent from '$lib/components/metadata/simpleComponent.svelte';
	import {
		getFullConfig,
		getIsRequiredBySchemaAndPath,
		getLabelByPath,
		getTargetVariablesWithValues,
		getValueByPath,
		resolveNode
	} from '$lib/components/utils/metadata/metadataComponentUtils';
	import { metadataStore } from '$lib/components/utils/metadata/stores';
	import { onMount } from 'svelte';


	export let anchor: string;
	export let path: string = '';
	export let mode: 'edit' | 'view' = 'edit';

	// set values	for each field
	$:vr = 'test';
	$:vm ='test';
	$:vl = 'test';

	// set paths
	$: pr = '';
	$: pm = '';
 $: pl = '';


	
	let componentName: string = 'horizontalAlignment_v1.0.0';

	// get config
	let config = getFullConfig(componentName, anchor, mode);

	if (!config) {
		console.error('No configuration found for component:', componentName, 'with anchor:', anchor);
	}

	let targetVars = getTargetVariablesWithValues(config);
	let isViewMode = mode === 'view';
	let separator = targetVars?.find((v) => v.target_variable === 'separator')?.value ?? ' ';
	let customLabel = targetVars?.find((v) => v.target_variable === 'label')?.value ?? '';

	let simpleComponents: {
		path: string;
		component: any;
		value: any;
		required: boolean | undefined;
		label: string;
    description: string;
    disabled: string;
	}[] = [];

	$:simpleComponents;

	let loaded	= false;

 onMount(() => {

			console.log('🚀 ~ load horizontal component~ ',targetVars);
				
			// field left
			const field_left = targetVars?.find((v) => v.target_variable === 'Field_left');
			const description_left = targetVars?.find((v) => v.target_variable === 'descriptionLeft')?.value ?? '';
			const disabled_left = targetVars?.find((v) => v.target_variable === 'disabledLeft')?.value ?? "false";
			const defaultValueLeft =
				targetVars?.find((v) => v.target_variable === 'defaultValueLeft')?.value ?? '';
			
			console.log('🚀 ~ field_left:', field_left);
			if (field_left && field_left.value) {
				console.log('🚀 ~ field_left.value:', field_left.value);
				const p = field_left.value;

				//set path	variableleft
				pl	= p;

						let value_left = getValueByPath(p);
						if (value_left === undefined || value_left === null || value_left === '') {
								value_left = defaultValueLeft;
						}
				simpleComponents.push({
					path: p,
					component: resolveNode(p),
					value: value_left,
					required: getIsRequiredBySchemaAndPath(p),
					label: getLabelByPath(p),
								description: description_left,
								disabled: disabled_left
				});
			}

			// field middle
			const field_middle = targetVars?.find((v) => v.target_variable === 'Field_mid');
			const description_middle = targetVars?.find((v) => v.target_variable === 'descriptionMid')?.value ?? '';
			const disabled_middle = targetVars?.find((v) => v.target_variable === 'disabledMid')?.value ?? "false";
			const defaultValueMiddle =
				targetVars?.find((v) => v.target_variable === 'defaultValueMiddle')?.value ?? '';

			if (field_middle && field_middle.value) {
				console.log('🚀 ~ field_middle.value:', field_middle.value);
				const p = field_middle.value;

					//set path	variableleft
					pm	= p;

						let value_middle = getValueByPath(p);
						if (value_middle === undefined || value_middle === null || value_middle === '') {
								value_middle = defaultValueMiddle;
						}
				simpleComponents.push({
					path: p,
					component: resolveNode(p),
					value: value_middle,
					required: getIsRequiredBySchemaAndPath(p),
					label: getLabelByPath(p),
								description: description_middle,
								disabled: disabled_middle
				});
			}

			// field right
			const field_right = targetVars?.find((v) => v.target_variable === 'Field_right');
			const description_right = targetVars?.find((v) => v.target_variable === 'descriptionRight')?.value ?? '';
			const disabled_right = targetVars?.find((v) => v.target_variable === 'disabledRight')?.value ?? "false";
			const defaultValueRight =
				targetVars?.find((v) => v.target_variable === 'defaultValueRight')?.value ?? '';

			if (field_right && field_right.value) {
				console.log('🚀 ~ field_right.value:', field_right.value);

				const p = field_right.value;
				pr = p;
				let value_right = getValueByPath(p);
				if (value_right === undefined || value_right === null || value_right === '') {
					value_right = defaultValueRight;
				}

				simpleComponents.push({
					path: p,
					component: resolveNode(p),
					value: value_right,
					required: getIsRequiredBySchemaAndPath(p),
					label: getLabelByPath(p),
					description: description_right, 
					disabled: disabled_right
				});

				loaded	= true;
			}
			console.log('🚀 ~ simpleComponents:', simpleComponents);

})

metadataStore.subscribe(() => {
		
			vr = getValueByPath(pr); 
			vm = getValueByPath(pm); 
			vl = getValueByPath(pl); 
	});


</script>


{#if isViewMode}
	<div class="entry">
		<span class="key text-sm font-medium text-gray-600 dark:text-gray-300">
			{customLabel || getLabelByPath(anchor)}
		</span>
		<span class="val text-sm text-gray-900">
			{simpleComponents.map((sc) => sc.value || '').filter((v) => v !== '').join(separator) || '—'}
		</span>
	</div>
{:else}

	<div id="horizontal-alignment" class="flex flex-row justify-between w-full">

		{#if loaded}
		<!-- render left component -->
  {@const leftComponent = simpleComponents.find((sc) => sc.path === pl)}
		{#if leftComponent}
				<div class="flex-1">
						<SimpleComponent
								simpleComponent={leftComponent.component.node}
								{...leftComponent}
								on:updated
								bind:value={vl}
						/>
				</div>
		{/if}

		<!-- render middle component -->
		{@const middleComponent = simpleComponents.find((sc) => sc.path === pm)}
		{#if middleComponent}
				<div class="flex-1">
						<SimpleComponent
								simpleComponent={middleComponent.component.node}
								{...middleComponent}
								on:updated
								bind:value={vm}
						/>
				</div>
		{/if}

		<!-- render right component -->
		{@const rightComponent = simpleComponents.find((sc) => sc.path === pr)}
		{#if rightComponent}
				<div class="flex-1">
						<SimpleComponent
								simpleComponent={rightComponent.component.node}
								{...rightComponent}
								on:updated
								bind:value={vr}
						/>
				</div>
		{/if}

		{/if}
	</div>
{/if}

<style>
	.entry {
		padding-bottom: 0.35rem;
	}

	.key {
		display: inline-block;
		flex-grow: 1;
	}

	.val {
		display: inline-block;
		width: 30vw;
	}

	@media (max-width: 768px) {
		.val {
			width: 50vw;
		}
	}
</style>
