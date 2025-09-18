<script lang="ts">
	import { writable } from 'svelte/store';
	import type { CurationTemplateModel } from './types';
	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';

	export let title: string;
	export let templates: CurationTemplateModel[];
	export let addFunction: (template: CurationTemplateModel) => void;

	const pickedTemplate = writable<CurationTemplateModel | null>(null);

	pickedTemplate.subscribe(($pickedTemplate) => {
		if (!$pickedTemplate) return;
		addFunction($pickedTemplate);
		pickedTemplate.set(null);
	});
</script>

<div class="relative flex items-center gap-2">
	<Fa icon={faPlus} class="text-surface-600" />
	<select
		bind:value={$pickedTemplate}
		class="select w-full"
		title={`Select a template to append it to the ${title.toLowerCase()}`}
		disabled={templates.length === 0}
	>
		<option value={null} disabled selected>
			{#if templates.length === 0}
				No templates available
			{:else}
				Select a template to append it to the {title.toLowerCase()}
			{/if}
		</option>
		{#each templates as template}
			<option value={template}>{template.name}</option>
		{/each}
	</select>
</div>
