<script lang="ts">
	import { ReadEntryModel } from '$models/settingModels';
	import {
		TextInput,
		CodeEditor,
		NumberInput,
		MultiSelect,
		helpStore,
		Api
	} from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faAdd, faTrash, faTriangleExclamation, faCircleCheck } from '@fortawesome/free-solid-svg-icons';
	import { tick } from 'svelte';

	export let onDirty: () => void = () => {};

	export let entry: ReadEntryModel;
	export let isChild = false;
	let JSONValue: string;
	let initialJSONValue: string;

	// API-sourced values (e.g., group names)
	let apiValues: string[] = [];
	let apiLoaded = false;
	let apiLoading = false;

	// Fallback mapping: if apiSource is not set on the entry (backend not rebuilt),
	// check known setting keys that should use API-sourced dropdowns
	const knownApiSources: Record<string, string> = {
		'curatorsGroupName': 'groups',
		'formerMemberRole': 'groups',
		'OwnerPartyRelationshipType': 'relationshipTypes',
		'DataRequestNotificationRelationshipType': 'relationshipTypes'
	};

	$: effectiveApiSource = entry.apiSource || knownApiSources[entry.key] || '';
	$: hasApiSource = !!effectiveApiSource;
	$: if (hasApiSource && !apiLoaded && !apiLoading) {
		loadApiValues();
	}

	async function loadApiValues() {
		const source = effectiveApiSource;
		if (!source || apiLoaded || apiLoading) return;
		apiLoading = true;
		console.log('Loading API values for', entry.key, 'apiSource:', source);
		try {
			let url = '';
			let nameField = 'name';

			if (source === 'groups') {
				url = '/api/groups';
				nameField = 'name';
			} else if (source === 'relationshipTypes') {
				url = '/api/partyRelationshipTypes';
				nameField = 'Title';
			} else if (source.startsWith('http') || source.startsWith('/')) {
				url = source;
			}

			if (url) {
				console.log('Fetching from:', url);
				const response = await Api.get(url);
				console.log('API response for', source, ':', response.data);
				const data = response.data;
				if (Array.isArray(data)) {
					if (data.length > 0 && typeof data[0] === 'object') {
						const val = data[0][nameField] ?? data[0][nameField.toLowerCase()] ?? data[0][nameField.toUpperCase()];
						if (val) {
							apiValues = data.map((d: any) => d[nameField] ?? d[nameField.toLowerCase()] ?? d[nameField.toUpperCase()]).sort();
						}
					} else if (data.length > 0 && typeof data[0] === 'string') {
						apiValues = data.sort();
					}
				}
				console.log('Loaded API values:', apiValues);
			}
			apiLoaded = true;
		} catch (e) {
			console.error('Failed to load API values for', entry.apiSource, e);
			apiLoaded = true;
		} finally {
			apiLoading = false;
		}
	}

	$: valueMatchesApi = !hasApiSource || !apiLoaded || apiValues.length === 0 || apiValues.includes(entry.value);
	$: showApiWarning = hasApiSource && apiLoaded && apiValues.length > 0 && !apiValues.includes(entry.value) && entry.value !== '';

	async function onApiSelectChange() {
		await tick();
		onDirty();
	}

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
				title: entry.value[0].title,
				type: entry.value[0].type,
				value: '',
				description: ''
			})
		];
	}
</script>

<div class="pb-10">
	{#if entry.options && entry.options.length >= 1}
		<div id={entry.key} on:mouseover={() => helpStore.show(entry.key)}>
			<MultiSelect
				id={entry.key}
				title={entry.title + ' (key: ' + entry.key + ')'}
				source={entry.options}
				bind:target={entry.value}
				isMulti={false}
			/>
		</div>
	{:else if hasApiSource && entry.type.toLowerCase() === 'string'}
		<div id={entry.key} on:mouseover={() => helpStore.show(entry.key)}>
			<div class="flex items-end gap-2">
				<div class="grow">
					{#if apiLoading}
						<TextInput
							id={entry.key}
							label="{entry.title} (key: {entry.key})"
							value="loading..."
							disabled={true}
							help={true}
						/>
					{:else if apiValues.length > 0}
					<div class="flex flex-col gap-1">
						<label class="text-sm font-medium" for="select-{entry.key}">{entry.title} (key: {entry.key})</label>
						<select
							id="select-{entry.key}"
							class="select"
							bind:value={entry.value}
							on:change={onApiSelectChange}
						>
							{#if !apiValues.includes(entry.value)}
								<option value={entry.value}>{entry.value} (not found)</option>
							{/if}
							{#each apiValues as val}
								<option value={val}>{val}</option>
							{/each}
						</select>
					</div>
					{:else}
						<TextInput
							id={entry.key}
							placeholder={entry.key}
							label="{entry.title} (key: {entry.key})"
							bind:value={entry.value}
							on:input
							help={true}
						/>
						<span class="text-xs text-warning-600">No {entry.apiSource} found in the system.</span>
					{/if}
				</div>
				{#if !apiLoading && apiValues.length > 0}
					{#if showApiWarning}
						<div class="pb-3 flex items-center gap-1 text-warning-600" title="Current value does not match any available {entry.apiSource}.">
							<Fa icon={faTriangleExclamation} />
						</div>
					{:else if hasApiSource && apiLoaded && valueMatchesApi}
						<div class="pb-3 flex items-center gap-1 text-success-600" title="Value is a valid {entry.apiSource}">
							<Fa icon={faCircleCheck} />
						</div>
					{/if}
				{/if}
			</div>
		</div>
	{:else if entry.type.toLowerCase() === 'string'}
		<TextInput
			id={entry.key}
			placeholder={entry.key}
			label="{entry.title} (key: {entry.key})"
			bind:value={entry.value}
			on:input
			help={true}
		/>
	{:else if entry.type.toLowerCase().includes('int')}
		<NumberInput
			id={entry.key}
			label="{entry.title} (key: {entry.key})"
			bind:value={entry.value}
			on:input
			help={true}
		/>
	{:else if entry.type.toLowerCase() === 'boolean'}
		<div id={entry.key} on:mouseover={() => helpStore.show(entry.key)}>
			<SlideToggle active="bg-primary-500" name="slider-label" size="sm" bind:checked={entry.value}
				>{entry.title} (key: {entry.key})</SlideToggle
			>
		</div>
	{:else if entry.type.toLowerCase() === 'json'}
		<div id={entry.key} on:mouseover={() => helpStore.show(entry.key)}>
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
		</div>
	{:else if entry.type === 'EntryList'}
		<div class="my-3" id={entry.key} on:mouseover={() => helpStore.show(entry.key)}>
			<span class="h3">{entry.title} (key: {entry.key})</span>
			{#each Object.values(entry.value) as e, index}
				<div class="flex card p-2">
					<div class="grow">
						<svelte:self entry={e} isChild={true} {onDirty} />
					</div>
					<div>
						{#if Object.values(entry.value).length > 1}
							<button
								class="btn variant-filled-error flex-none"
								type="button"
								on:click={() => removeItem(index)}><Fa icon={faTrash} /></button
							>
						{/if}
					</div>
				</div>
			{/each}

			<button class="btn variant-filled-primary" type="button" on:click={addItem}>
				<Fa icon={faAdd} />
			</button>
		</div>
	{/if}

	{#if isChild}
		<TextInput label="Description" bind:value={entry.description} on:input />
	{/if}
</div>
