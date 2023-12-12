<script lang="ts">
	import { ReadEntryModel } from '$models/settingModels';
	import {
		TextInput,
		CodeEditor,
		NumberInput,
		MultiSelect,
		helpStore
	} from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faAdd, faTrash } from '@fortawesome/free-solid-svg-icons';

	export let entry: ReadEntryModel;
	export let isChild = false;
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
		<TextInput id={entry.key} label="{entry.title} (key: {entry.key})" bind:value={entry.value} on:input help={true} />
	{:else if entry.type.toLowerCase().includes('int')}
		<NumberInput id={entry.key} label="{entry.title} (key: {entry.key})" bind:value={entry.value} on:input help={true} />
	{:else if entry.type.toLowerCase() === 'boolean'}
		<SlideToggle active="bg-primary-500" name="slider-label" size="sm" bind:checked={entry.value}
			>{entry.title} (key: {entry.key})</SlideToggle>
	{:else if entry.type.toLowerCase() === 'json'}
		<CodeEditor 
			title="{entry.title} (key: {entry.key})"
			id={entry.key}
			initialValue={initialJSONValue}
			actions={false}
			language="json"
			toggle={false}
			bind:value={JSONValue}
			on:save={() => (entry.value = JSON.parse(JSONValue))}
		/>
	{:else if entry.type === 'EntryList'}
		<div class="my-3" id={entry.key} on:mouseover={() => { helpStore.show(entry.key); }}>
			<span class="h3">{entry.title} (key: {entry.key})</span>
			{#each Object.values(entry.value) as e, index}
				<div class="flex card p-2">
					<div class="grow">
						<svelte:self entry={e} isChild={true}/>
					</div>
					<div>
						<button class="btn variant-filled-error flex-none" on:click={() => removeItem(index)}
							><Fa icon={faTrash} /></button>
					</div>
				</div>
			{/each}

			<button class="btn variant-filled-primary" on:click={addItem}>
				<Fa icon={faAdd} />
			</button>
		</div>
	{/if}

	{#if isChild}
		<TextInput label="Description" bind:value={entry.description} on:input />
	{/if}
</div>
