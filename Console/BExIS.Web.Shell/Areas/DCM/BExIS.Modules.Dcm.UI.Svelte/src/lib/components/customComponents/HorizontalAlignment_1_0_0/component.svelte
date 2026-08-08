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

	export let anchor: string;
	export let path: string = '';

	let componentName: string = 'horizontalAlignment_v1.0.0';

	// get config
	let config = getFullConfig(componentName, anchor);

	if (!config) {
		console.error('No configuration found for component:', componentName, 'with anchor:', anchor);
	}

	let targetVars = getTargetVariablesWithValues(config);

	let simpleComponents: {
		path: string;
		component: any;
		value: any;
		required: boolean | undefined;
		label: string;
    description: string;
    disabled: string;
	}[] = [];

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
	}
	console.log('🚀 ~ simpleComponents:', simpleComponents);
</script>

<div id="horizontal-alignment" class="flex flex-row justify-between w-full">
	{#each simpleComponents as simpleComponent, index (simpleComponent.path)}
		<div class="flex-1">
			{#if simpleComponent}
				<SimpleComponent
					simpleComponent={simpleComponent.component.node}
					{...simpleComponent}
					on:reload
				/>
			{/if}
		</div>
	{/each}
</div>
