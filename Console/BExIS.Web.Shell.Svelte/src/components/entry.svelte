<script lang="ts">
	import type { ReadEntryModel } from '$models/settingModels';
	import { TextInput, TextArea, DateInput, NumberInput } from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';

	export let entry: ReadEntryModel;
	console.log(entry);
</script>

{#if entry.type === 'String'}
	<TextInput label={entry.key} bind:value={entry.value} on:input />
{:else if entry.type === 'Boolean'}
	<SlideToggle name="slider-label" size="sm" bind:checked={entry.value}>({entry.value})</SlideToggle>
{:else if entry.type === 'JSON'}
	<TextArea></TextArea>
{:else if entry.type === 'EntryList'}
	<div>
		<span>{entry.key}</span>
		{#each Object.values(entry.value) as e}
			<svelte:self entry={e} />
		{/each}
	</div>
{/if}
