<script lang="ts">
	import { ReadEntryModel } from '$models/settingModels';
	import {
		TextInput,
		CodeEditor,
		TextArea,
		DateInput,
		NumberInput,
		MultiSelect
	} from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { onMount } from 'svelte';
	import Fa from 'svelte-fa';
	import { faAdd, faTrash } from '@fortawesome/free-solid-svg-icons';

	export let entry: ReadEntryModel;
	let JSONValue: string;
	let initialJSONValue: string;

	if (entry.type === 'JSON') {
		initialJSONValue = JSON.stringify(entry.value, null, 2);
		JSONValue = initialJSONValue;
	}

	$: onChange(JSONValue);

	// TODO: add comments
	function onChange(value: string) {
		if (typeof value !== 'undefined') {
			try {
				console.log(value);
				entry.value = JSON.parse(value);
			} catch {
				// add icon to indicate that something went wrong.
				console.log('error');
			}
		}
	}

	function removeItem(index) {
		if (Object.values(entry.value).length > 1) {
			entry.value.splice(index, 1);
			entry.value = entry.value;
		}
	}

	function addItem() {
		entry.value = [
			...entry.value,
			new ReadEntryModel({
				key: entry.value[0].key,
				type: entry.value[0].type,
				value: '...',
				description: '...'
			})
		];
	}
</script>

<div class="pb-10">
	{#if entry.options && entry.options.length >= 1}
		<MultiSelect
			id={entry.key}
			title={entry.key}
			source={entry.options}
			bind:target={entry.value}
			isMulti={false}
		/>
	{:else if entry.type.toLowerCase() === 'string'}
		<TextInput label={entry.key} bind:value={entry.value} on:input />
	{:else if entry.type.toLowerCase() === 'integer'}
		<NumberInput label={entry.key} bind:value={entry.value} on:input />
	{:else if entry.type.toLowerCase() === 'boolean'}
		<SlideToggle active="bg-primary-500" name="slider-label" size="sm" bind:checked={entry.value}
			>{entry.key}</SlideToggle
		>
	{:else if entry.type.toLowerCase() === 'json'}
		<CodeEditor
			initialValue={initialJSONValue}
			actions={false}
			language="json"
			toggle={false}
			bind:value={JSONValue}
			on:save={() => (entry.value = JSON.parse(JSONValue))}
		/>
	{:else if entry.type === 'EntryList'}
		<div class="my-3">
			<span class="h3">{entry.key}</span>
			{#each Object.values(entry.value) as e, index}
				<div class="flex card p-2">
					<div class="grow">
						<svelte:self entry={e} />
					</div>
					<div>
						<button class="btn variant-filled-error flex-none" on:click={() => removeItem(index)}
							><Fa icon={faTrash} /></button
						>
					</div>
				</div>
			{/each}

			<button class="btn variant-filled-primary" on:click={addItem}>
				<Fa icon={faAdd} />
			</button>

			<TextInput label="Description" bind:value={entry.description} on:input />
		</div>
	{/if}
</div>