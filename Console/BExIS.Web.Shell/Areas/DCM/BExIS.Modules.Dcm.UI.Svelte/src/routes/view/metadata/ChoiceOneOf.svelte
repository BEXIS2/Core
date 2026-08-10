<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponent.svelte';
	import { getValueByPath, hasValueAtPath } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { activeStore, hideStore } from '$lib/components/utils/metadata/stores';
	import { slide } from 'svelte/transition';
	import Header from './MetadataComponentHeader.svelte';
	import { getViewChoiceOptions, resolveViewModeSelection } from './choiceOneOfUtils';

	export let choiceComponent: any;
	export let path: string;

	let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
	let choices = getViewChoiceOptions(choiceComponent, path);
	$: resolvedSelection = resolveViewModeSelection(choiceComponent, path, hasValueAtPath);
	$: selectedChoice = resolvedSelection ? choiceComponent?.properties?.[resolvedSelection] : null;
	$: selectedPath = resolvedSelection ? `${path}.${resolvedSelection}` : '';

	function getBranchValue(branchPath: string) {
		return getValueByPath(branchPath);
	}



</script>

<div class="grid grid-cols-1 gap-0 m-2">
	<Header {path} p={path} />

	{#if !$hideStore.includes(path) && $activeStore.includes(path)}
		<div in:slide out:slide class="px-5" id={path}>
			{#if choiceComponent.oneOf && choices.length > 0}
				<div class="mb-3 text-sm font-semibold text-gray-700">
					{label}: {resolvedSelection || 'No selection'}
				</div>
				{#each choices as item}
					<div class="flex items-center gap-2 py-1">
						<span class="text-sm text-gray-600">{item.display}</span>
						{#if item.key === resolvedSelection}
							<span class="badge variant-filled-primary">Selected</span>
						{/if}
					</div>
				{/each}
			{/if}

			{#if selectedChoice && selectedPath}
				{#if selectedChoice.type === 'object' && selectedChoice.properties && !selectedChoice.properties['#text']}
					<div class="grid grid-cols-1 gap-0 m-2">
						{#if !$hideStore.includes(selectedPath) && $activeStore.includes(path)}
							<div in:slide out:slide class="card px-5" id={selectedPath}>
								<ComplexComponent
									complexComponent={selectedChoice}
									path={selectedPath}
									required={choiceComponent.required && choiceComponent.required.includes(resolvedSelection)}
								/>
							</div>
						{/if}
					</div>
				{:else if selectedChoice.type === 'object' && selectedChoice.properties && selectedChoice.properties['#text']}
					<div class="px-5">
						<SimpleComponent
							simpleComponent={selectedChoice.properties['#text']}
							path={selectedPath}
							required={choiceComponent.required && choiceComponent.required.includes(resolvedSelection)}
							value={getBranchValue(selectedPath)}
							label={resolvedSelection}
						/>
					</div>
				{/if}
			{/if}
		</div>
	{/if}
</div>



