<script lang="ts">
	import { onMount } from 'svelte';
	import {
		updateMetadataStore,
		getFullConfig,
		getTargetVariablesWithValues,
		resolveNode,
		updateValidationState,
		registerValidationItem,
		getMetadata,
		validateCustomCondition
	} from '../../utils/metadata/metadataComponentUtils';
	import { InputContainer } from '@bexis2/bexis2-core-ui';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';
	import { validationStore } from '$lib/components/utils/metadata/stores';

	let res = suite.get();
	let componentName: string = 'ror_v1.0.0';

	export let anchor: string;
	export let path: string = '';
	export let mode: 'edit' | 'view' = 'edit';

	let config = getFullConfig(componentName, anchor, mode);
	let targetVars = getTargetVariablesWithValues(config);

	let modeName = config?.mode?.mode_name ?? '';
	let isViewMode = mode === 'view';

	let ror_field_path = targetVars?.find((v) => v.target_variable === 'ror_field')?.value
		?? targetVars?.find((v) => v.target_variable === 'displayRor')?.value
		?? '';
	if (ror_field_path && ror_field_path == anchor.split('.').slice(0, -1).join('.')) {
		ror_field_path = anchor;
	}
	let { value, ref, label, description, required } = getMetadata(ror_field_path);
	let validationRegistered = false;
	let validationReady = false;

	$: validationItem = $validationStore?.simpleTypeValidationItems?.find(
		(i) => i.path === ror_field_path
	);

	let RorApiUrl =
		targetVars?.find((v) => v.target_variable === 'RorApiUrl')?.value || 'https://api.ror.org/';
	if (RorApiUrl && !RorApiUrl.endsWith('/')) RorApiUrl += '/';

	let debounceStr = targetVars?.find((v) => v.target_variable === 'debounce')?.value ?? '400';
	let debounceMs = parseInt(debounceStr) || 400;

	let descriptionCustom = targetVars?.find((v) => v.target_variable === 'description')?.value ?? '';
	if (descriptionCustom && descriptionCustom.trim() !== '') {
		description = descriptionCustom;
	}

	type RorResult = {
		id: string;
		name: string;
		aliases: string[];
		country?: string;
		city?: string;
	};
	let searchQuery = value || '';
	let searchResults: RorResult[] = [];
	let isLoading = false;
	let showResults = false;
	let selectedIndex = -1;
	let searchTimer: ReturnType<typeof setTimeout> | null = null;

	onMount(async () => {
		if (isViewMode) {
			return;
		}

		const { node: schemaNode } = resolveNode(ror_field_path);
		registerValidationItem(ror_field_path, label, required, schemaNode, true);
		validationRegistered = true;
		syncRorValue();
	});

	function onSearchInput(e: Event) {
		const input = e.target as HTMLInputElement;
		searchQuery = input.value;

		if (searchTimer) clearTimeout(searchTimer);

		if (searchQuery.trim().length < 2) {
			searchResults = [];
			showResults = false;
			if (searchQuery.trim() === '') {
				value = '';
				ref = '';
				syncRorValue();
			}
			return;
		}

		searchTimer = setTimeout(() => {
			searchOrganizations(searchQuery.trim());
		}, debounceMs);
	}

	async function searchOrganizations(query: string) {
		console.log('ROR search for:', query, 'at', RorApiUrl);
		isLoading = true;
		showResults = true;
		try {
			const response = await fetch(
				`${RorApiUrl}organizations?query=${encodeURIComponent(query)}`
			);
			console.log('ROR response status:', response.status);
			if (!response.ok) {
				console.warn('ROR API returned non-OK status');
				searchResults = [];
				return;
			}
			const data = await response.json();
			console.log('ROR results count:', data?.number_of_results ?? 0);
			const items = data?.items ?? [];
			searchResults = items.map((item: any) => {
				const names: any[] = Array.isArray(item.names) ? item.names : [];
				const displayName = names.find((n: any) => n.types?.includes('ror_display'))?.value ?? '';
				const allAliases = names
					.filter((n: any) => !n.types?.includes('ror_display'))
					.map((n: any) => n.value)
					.filter((v: string) => v && v !== displayName);
				return {
					id: item.id,
					name: displayName || names[0]?.value || item.name || '',
					aliases: allAliases,
					country: item?.locations?.[0]?.geonames_detail?.country_name ?? '',
					city: item?.locations?.[0]?.geonames_detail?.name ?? ''
				};
			});
			selectedIndex = -1;
		} catch (error) {
			console.error('Error searching ROR organizations:', error);
			searchResults = [];
		} finally {
			isLoading = false;
		}
	}

	function selectRor(result: RorResult) {
		value = result.name;
		ref = result.id;
		searchQuery = result.name;
		showResults = false;
		searchResults = [];
		syncRorValue();
	}

	function onKeydown(e: KeyboardEvent) {
		if (!showResults || searchResults.length === 0) return;

		if (e.key === 'ArrowDown') {
			e.preventDefault();
			selectedIndex = Math.min(selectedIndex + 1, searchResults.length - 1);
		} else if (e.key === 'ArrowUp') {
			e.preventDefault();
			selectedIndex = Math.max(selectedIndex - 1, 0);
		} else if (e.key === 'Enter' && selectedIndex >= 0) {
			e.preventDefault();
			selectRor(searchResults[selectedIndex]);
		} else if (e.key === 'Escape') {
			showResults = false;
		}
	}

	function onBlur() {
		setTimeout(() => {
			showResults = false;
		}, 200);
	}

	function onFocus() {
		if (searchResults.length > 0) {
			showResults = true;
		}
	}

	function updateValue(rorValue: any, _path: string) {
		res = suite(_path);
		updateValidationState(_path, res);

		const isNotEmpty = rorValue != null && String(rorValue).trim() !== '';
		if (required && !isNotEmpty) {
			validateCustomCondition(_path, false, 'Please select an organization from ROR.');
		}
	}

	function syncRorValue() {
		if (!validationRegistered) return;

		updateMetadataStore(
			ror_field_path,
			value != undefined && value != null ? value.toString() : '',
			false,
			ref != undefined && ref != null ? ref.toString() : ''
		);
		updateValue(value, ror_field_path);
		validationReady = true;
	}

	$: commonProps = {
		id: path,
		label: label,
		required,
		invalid: validationReady && validationItem ? !validationItem.isValid : false,
		valid: validationReady && validationItem ? validationItem.isValid : false,
		feedback:
			validationItem && validationItem.errorMessage ? validationItem.errorMessage.split('\n') : [],
		description: description,
		showDescription: false,
		showIcon: false,
		disabled: false
	};
</script>

{#if isViewMode}
	<div class="entry">
		<span class="key text-sm font-medium text-gray-500">{label}</span>
		<span class="val text-sm text-gray-900 font-semibold">
			{#if value}
				{#if ref}
					<a href={ref} target="_blank" rel="noopener noreferrer" class="ror-link">
						<span>{value}</span>
					</a>
				{:else}
					{value}
				{/if}
			{:else}
				<span class="text-gray-400">—</span>
			{/if}
		</span>
	</div>
{:else}
	<InputContainer {...commonProps} on:showDescription on:hideDescription>
		<div class="ror-search-container">
			<input
				type="text"
				class="ror-input input variant-form-material {commonProps.valid ? 'input-success' : ''} {commonProps.invalid ? 'input-error' : ''}"
				placeholder="Search for an organization..."
				bind:value={searchQuery}
				on:input={onSearchInput}
				on:keydown={onKeydown}
				on:blur={onBlur}
				on:focus={onFocus}
			/>
			{#if isLoading}
				<div class="ror-loading">
					<span class="ror-spinner"></span>
					<span>Searching...</span>
				</div>
			{/if}
			{#if showResults && searchResults.length > 0}
				<ul class="ror-results">
					{#each searchResults as result, i}
						<li
							class="ror-result-item"
							class:selected={i === selectedIndex}
							on:mousedown={() => selectRor(result)}
							on:mouseenter={() => (selectedIndex = i)}
							role="option"
							tabindex="-1"
						>
							<div class="ror-result-name">{result.name}</div>
							{#if result.aliases.length > 0}
								<div class="ror-result-aliases">
									{#each result.aliases as alias}
										<span class="ror-alias">{alias}</span>
									{/each}
								</div>
							{/if}
							<div class="ror-result-meta">
								{#if result.city || result.country}
									{[result.city, result.country].filter(Boolean).join(', ')}
								{/if}
								<span class="ror-id">{result.id}</span>
							</div>
						</li>
					{/each}
				</ul>
			{/if}
			{#if showResults && !isLoading && searchResults.length === 0 && searchQuery.trim().length >= 2}
				<div class="ror-no-results">No organizations found for "{searchQuery}"</div>
			{/if}
			{#if ref}
				<div class="ror-selected-id">
					Selected ROR ID: <a href={ref} target="_blank" rel="noopener noreferrer">{ref}</a>
				</div>
			{/if}
		</div>
	</InputContainer>
{/if}

<style>
	.entry {
		display: flex;
		flex-direction: row;
	}

	.key {
		display: inline-block;
		flex-grow: 1;
	}

	.val {
		display: inline-block;
		width: 30vw;
		font-weight: bold;
	}

	.ror-link {
		color: rgb(37 99 235);
	}

	.ror-link:hover {
		text-decoration: underline;
	}

	.ror-search-container {
		position: relative;
		width: 100%;
	}

	.ror-input {
		width: 100%;
		padding: 0.5rem 0.75rem;
		border-radius: 4px;
	}

	.ror-loading {
		position: absolute;
		right: 0.75rem;
		top: 0.5rem;
		display: flex;
		align-items: center;
		gap: 0.35rem;
		font-size: 0.75rem;
		color: #888;
	}

	.ror-spinner {
		display: inline-block;
		width: 0.85rem;
		height: 0.85rem;
		border: 2px solid #ccc;
		border-top-color: #007acc;
		border-radius: 50%;
		animation: ror-spin 0.6s linear infinite;
	}

	@keyframes ror-spin {
		to {
			transform: rotate(360deg);
		}
	}

	.ror-results {
		position: absolute;
		z-index: 1000;
		left: 0;
		right: 0;
		max-height: 18rem;
		overflow-y: auto;
		list-style: none;
		margin: 2px 0 0 0;
		padding: 0;
		border: 1px solid #ddd;
		border-radius: 4px;
		background: white;
		box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
	}

	.ror-result-item {
		padding: 0.5rem 0.75rem;
		cursor: pointer;
		border-bottom: 1px solid #f0f0f0;
		transition: background-color 0.15s;
	}

	.ror-result-item:last-child {
		border-bottom: none;
	}

	.ror-result-item.selected,
	.ror-result-item:hover {
		background: #f0f7ff;
	}

	.ror-result-name {
		font-size: 0.85rem;
		font-weight: 500;
		color: #333;
	}

	.ror-result-aliases {
		display: flex;
		flex-wrap: wrap;
		gap: 0.25rem;
		margin-top: 2px;
	}

	.ror-alias {
		font-size: 0.72rem;
		color: #666;
		background: #f0f0f0;
		padding: 1px 5px;
		border-radius: 3px;
	}

	.ror-result-meta {
		display: flex;
		justify-content: space-between;
		gap: 0.5rem;
		font-size: 0.75rem;
		color: #888;
		margin-top: 2px;
	}

	.ror-id {
		font-family: monospace;
	}

	.ror-no-results {
		position: absolute;
		z-index: 1000;
		left: 0;
		right: 0;
		padding: 0.75rem;
		border: 1px solid #ddd;
		border-radius: 4px;
		background: white;
		font-size: 0.85rem;
		color: #999;
		text-align: center;
		box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
	}

	.ror-selected-id {
		margin-top: 0.25rem;
		font-size: 0.75rem;
		color: #888;
	}

	.ror-selected-id a {
		color: rgb(37 99 235);
		font-family: monospace;
	}

	.ror-selected-id a:hover {
		text-decoration: underline;
	}
</style>
