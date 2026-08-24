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
		validateCustomCondition,
		getValueByPath,
		getRefByPath,
		getParentPath,
		removeJsonPathIndices,
		getPartyIdByPath
	} from '../../utils/metadata/metadataComponentUtils';
	import { InputContainer, MultiSelect } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faCircleCheck, faCircleQuestion, faXmark } from '@fortawesome/free-solid-svg-icons';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';
	import { validationStore, metadataStore, systemMappingsStore } from '$lib/components/utils/metadata/stores';
	import { getMappingComponentConfig } from '$lib/components/utils/metadata/mappingHelper';
	import { GetPartyValue } from '../../../../services/MetadataCaller';

	let res = suite.get();
	let componentName: string = 'orcid_v1.0.0';

	export let anchor: string;
	export let path: string = '';
	export let mode: 'edit' | 'view' = 'edit';

	let config = getFullConfig(componentName, anchor, mode);
	let targetVars = getTargetVariablesWithValues(config);

	let modeName = config?.mode?.mode_name ?? '';
	let isSearchMode = modeName === 'Search';
	let isValidateMode = modeName === 'Validate and Fill';

	let OrcidApiUrl =
		targetVars?.find((v) => v.target_variable === 'OrcidApiUrl')?.value || 'https://pub.orcid.org/v3.0/';
	if (OrcidApiUrl && !OrcidApiUrl.endsWith('/')) OrcidApiUrl += '/';

	let debounceStr = targetVars?.find((v) => v.target_variable === 'debounce')?.value ?? '400';
	let debounceMs = parseInt(debounceStr) || 400;

	let descriptionCustom = targetVars?.find((v) => v.target_variable === 'description')?.value ?? '';

	// --- Search mode ---
	let orcid_field_path = targetVars?.find((v) => v.target_variable === 'orcid_field')?.value
		?? targetVars?.find((v) => v.target_variable === 'displayOrcid')?.value
		?? '';
	if (orcid_field_path && orcid_field_path == anchor.split('.').slice(0, -1).join('.')) {
		orcid_field_path = anchor;
	}
	let { value, ref, label, description, required } = getMetadata(orcid_field_path);
	if (descriptionCustom && descriptionCustom.trim() !== '') {
		description = descriptionCustom;
	}

	// Party mapping support
	let mappingComponentConfig = getMappingComponentConfig(orcid_field_path, value);
	$: canLinkToParty = mappingComponentConfig?.isMappedToParty ?? false;
	$: partyMappingObject = mappingComponentConfig?.partyMappingObject ?? null;
	$: isComplexMapping = canLinkToParty && partyMappingObject?.complexity === true;
	$: partyCheckPath = isComplexMapping ? getParentPath(orcid_field_path) : orcid_field_path;
	$: storeData = $metadataStore;
	$: partyNode = storeData && canLinkToParty ? partyCheckPath.split('.').reduce((acc: any, part: string) => acc && acc[part], storeData) : null;
	$: currentPartyId = canLinkToParty ? (partyNode ? partyNode['@partyid'] : null) : null;
	$: hasPartyId = canLinkToParty && currentPartyId != null && Number(currentPartyId) > 0;

	// Party autocomplete state (used when canLinkToParty is true)
	let selectorValue: any = null;
	let partyList: any[] = [];
	let showOrcidSearch = false;

	$: partyList = partyMappingObject?.list ?? [];

	// Party autocomplete handler (same logic as PartySelector, plus ORCID validation)
	async function onUpdateParty(e: any) {
		const detail = e?.detail ?? {};
		const partyid = detail.partyId ?? 0;
		const newValue = detail.value ?? '';

		// close ORCID search if open
		showOrcidSearch = false;

		setTimeout(async () => {
			if (!isComplexMapping) {
				updateMetadataStore(orcid_field_path, newValue, false, undefined, partyid);
				value = newValue;
				ref = '';
				syncOrcidValue();
			} else {
				updateMetadataStore(orcid_field_path, newValue, false, undefined, undefined);
				value = newValue;
				syncOrcidValue();

				const parentPath = getParentPath(orcid_field_path);
				const parentPathWithoutIndices = removeJsonPathIndices(parentPath);
				updateMetadataStore(parentPath, null, false, undefined, partyid);

				$systemMappingsStore.partyMappings
					.filter((mapping: any) =>
						mapping.parentPath == parentPathWithoutIndices && mapping.path !== removeJsonPathIndices(orcid_field_path)
					)
					.forEach(async (mapping: any) => {
						const childvalue = await GetPartyValue(partyid, mapping.linkElementId);
						const childPathWithIndex = parentPath + '.' + mapping.path.split('.').slice(-1)[0];
						updateMetadataStore(childPathWithIndex, childvalue, false, undefined, undefined);
					});
			}

			// After party is set, automatically search ORCID to validate/find the ORCID ID
			if (newValue && newValue.trim().length >= 2) {
				await searchOrcidForParty(newValue.trim());
			}
		}, 100);
	}

	// Search ORCID for the selected party name and auto-select if exactly one match is found
	async function searchOrcidForParty(name: string) {
		isLoading = true;
		showResults = true;
		searchQuery = name;
		try {
			const words = name.split(/\s+/).filter(Boolean);
			const given = words[0] || '';
			const family = words.length > 1 ? words[words.length - 1] : '';
			let orcidQuery: string;
			if (given && family) {
				orcidQuery = `given-names:${encodeURIComponent(given)}+AND+family-name:${encodeURIComponent(family)}`;
			} else {
				orcidQuery = words.map(w => `given-names:${encodeURIComponent(w)}+OR+family-name:${encodeURIComponent(w)}`).join('+OR+');
			}
			const searchUrl = `${OrcidApiUrl}search/?q=${orcidQuery}&rows=10`;

			const orcidIds = await fetchOrcidSearch(searchUrl);
			searchResults = [];
			for (const orcidId of orcidIds) {
				const person = await fetchOrcidPerson(orcidId);
				searchResults.push({
					orcidId,
					orcidUri: `https://orcid.org/${orcidId}`,
					givenNames: person.givenNames,
					familyName: person.familyName,
					creditName: person.creditName
				});
			}

			// Auto-select if there's an exact name match
			const exactMatch = searchResults.find(r => {
				const fullName = `${r.givenNames} ${r.familyName}`.trim().toLowerCase();
				const creditLower = r.creditName?.toLowerCase() ?? '';
				return fullName === name.toLowerCase() || creditLower === name.toLowerCase();
			});

			if (exactMatch) {
				// Auto-select the matching ORCID record
				ref = exactMatch.orcidUri;
				syncOrcidValue();
				showResults = false;
				searchResults = [];
			}
			// If no exact match but results exist, keep them visible for manual selection
		} catch (error) {
			console.error('Error searching ORCID for party:', error);
			searchResults = [];
		} finally {
			isLoading = false;
		}
	}

	let validationRegistered = false;
	let validationReady = false;

	$: validationItem = $validationStore?.simpleTypeValidationItems?.find(
		(i) => i.path === orcid_field_path
	);

	type OrcidResult = {
		orcidId: string;
		orcidUri: string;
		givenNames: string;
		familyName: string;
		creditName: string;
	};

	let searchQuery = value || '';
	let searchResults: OrcidResult[] = [];
	let isLoading = false;
	let showResults = false;
	let selectedIndex = -1;
	let searchTimer: ReturnType<typeof setTimeout> | null = null;

	// --- Validate mode ---
	let given_name_path = targetVars?.find((v) => v.target_variable === 'given_name')?.value ?? '';
	let family_name_path = targetVars?.find((v) => v.target_variable === 'family_name')?.value ?? '';
	let orcid_id_path = targetVars?.find((v) => v.target_variable === 'orcid_id')?.value ?? '';

	// strip leading $ or $. from paths
	const cleanPath = (p: string) => p ? p.replace(/^\$\.?/, '') : p;
	given_name_path = cleanPath(given_name_path);
	family_name_path = cleanPath(family_name_path);
	orcid_id_path = cleanPath(orcid_id_path);

	console.log('ORCID validate paths:', { given_name_path, family_name_path, orcid_id_path });

	let givenNameValue = given_name_path ? getValueByPath(given_name_path) ?? '' : '';
	let familyNameValue = family_name_path ? getValueByPath(family_name_path) ?? '' : '';
	let validateResults: OrcidResult[] = [];
	let isValidateLoading = false;
	let showValidateResults = false;
	let selectedValidateIndex = -1;
	let validateTimer: ReturnType<typeof setTimeout> | null = null;

	$: {
		if (isValidateMode) {
			givenNameValue = given_name_path ? getValueByPath(given_name_path) ?? '' : '';
			familyNameValue = family_name_path ? getValueByPath(family_name_path) ?? '' : '';
		}
	}

	onMount(async () => {
		if (isSearchMode) {
			const { node: schemaNode } = resolveNode(orcid_field_path);
			registerValidationItem(orcid_field_path, label, required, schemaNode, true);
			validationRegistered = true;
			syncOrcidValue();
		}

		// Initialize party selector value from current party (like PartySelector does)
		if (canLinkToParty && partyMappingObject) {
			if (value) {
				if (partyMappingObject.complexity) {
					const parentPath = getParentPath(orcid_field_path);
					currentPartyId = getPartyIdByPath(parentPath);
				} else {
					currentPartyId = getPartyIdByPath(orcid_field_path);
				}
				const pid = currentPartyId ? Number(currentPartyId) : 0;
				if (pid > 0 && partyList.length > 0) {
					selectorValue = partyList.find((item: any) => Number(item.partyId) === pid) ?? null;
				}
			}
		}

		if (isValidateMode) {
			// auto-search when both names are available
			if (givenNameValue && familyNameValue) {
				setTimeout(() => searchOrcidByName(givenNameValue, familyNameValue, true), 500);
			}
		}
	});

	// --- Search functions ---
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
				syncOrcidValue();
			}
			return;
		}

		searchTimer = setTimeout(() => {
			searchOrcidByText(searchQuery.trim());
		}, debounceMs);
	}

	async function searchOrcidByText(query: string) {
		console.log('ORCID search for:', query);
		isLoading = true;
		showResults = true;
		try {
			const words = query.split(/\s+/).filter(Boolean);
			const queryParts = words.map(w =>
				`given-names:${encodeURIComponent(w)}+OR+family-name:${encodeURIComponent(w)}`
			);
			const orcidQuery = queryParts.length > 1 ? `(${queryParts.join('+OR+')})` : queryParts[0];
			const searchUrl = `${OrcidApiUrl}search/?q=${orcidQuery}&rows=10`;
			console.log('ORCID search URL:', searchUrl);

			const orcidIds = await fetchOrcidSearch(searchUrl);
			console.log('ORCID found IDs:', orcidIds.length);

			searchResults = [];
			for (const orcidId of orcidIds) {
				const person = await fetchOrcidPerson(orcidId);
				searchResults.push({
					orcidId: orcidId,
					orcidUri: `https://orcid.org/${orcidId}`,
					givenNames: person.givenNames,
					familyName: person.familyName,
					creditName: person.creditName
				});
			}
			selectedIndex = -1;
		} catch (error) {
			console.error('Error searching ORCID:', error);
			searchResults = [];
		} finally {
			isLoading = false;
		}
	}

	async function searchOrcidByName(given: string, family: string, isAuto = false) {
		if (!given.trim() || !family.trim()) return;

		console.log('ORCID validate search for:', given, family);
		isValidateLoading = true;
		showValidateResults = true;
		try {
			const searchUrl = `${OrcidApiUrl}search/?q=given-names:${encodeURIComponent(given.trim())}+AND+family-name:${encodeURIComponent(family.trim())}&rows=10`;
			console.log('ORCID validate URL:', searchUrl);

			const orcidIds = await fetchOrcidSearch(searchUrl);
			console.log('ORCID validate found IDs:', orcidIds.length);

			validateResults = [];
			for (const orcidId of orcidIds) {
				const person = await fetchOrcidPerson(orcidId);
				validateResults.push({
					orcidId: orcidId,
					orcidUri: `https://orcid.org/${orcidId}`,
					givenNames: person.givenNames,
					familyName: person.familyName,
					creditName: person.creditName
				});
			}
			selectedValidateIndex = -1;
		} catch (error) {
			console.error('Error validating ORCID:', error);
			validateResults = [];
		} finally {
			isValidateLoading = false;
		}
	}

	async function fetchOrcidSearch(searchUrl: string): Promise<string[]> {
		const response = await fetch(searchUrl, {
			headers: { 'Accept': 'application/json' }
		});
		console.log('ORCID search response status:', response.status);
		if (!response.ok) {
			console.warn('ORCID API returned non-OK status:', response.status);
			return [];
		}

		const contentType = response.headers.get('content-type') || '';
		if (contentType.includes('application/json')) {
			const data = await response.json();
			const results = data?.result ?? [];
			return results
				.map((r: any) => r?.['orcid-identifier']?.path)
				.filter((id: any) => id);
		}

		// fallback: parse XML
		const xmlText = await response.text();
		const parser = new DOMParser();
		const doc = parser.parseFromString(xmlText, 'application/xml');
		const paths = doc.getElementsByTagNameNS('*', 'path');
		const ids: string[] = [];
		for (let i = 0; i < paths.length; i++) {
			const val = paths[i].textContent;
			if (val && /^\d{4}-\d{4}-\d{4}-\d{3}[\dX]$/.test(val)) {
				ids.push(val);
			}
		}
		return ids;
	}

	async function fetchOrcidPerson(orcidId: string): Promise<{ givenNames: string; familyName: string; creditName: string }> {
		try {
			const response = await fetch(`${OrcidApiUrl}${orcidId}/person`, {
				headers: { 'Accept': 'application/json' }
			});
			if (!response.ok) return { givenNames: '', familyName: '', creditName: '' };

			const contentType = response.headers.get('content-type') || '';
			if (contentType.includes('application/json')) {
				const data = await response.json();
				return {
					givenNames: data?.name?.['given-names']?.value ?? '',
					familyName: data?.name?.['family-name']?.value ?? '',
					creditName: data?.name?.['credit-name']?.value ?? ''
				};
			}

			// fallback: parse XML
			const xmlText = await response.text();
			const parser = new DOMParser();
			const doc = parser.parseFromString(xmlText, 'application/xml');
			const getText = (localName: string) =>
				doc.getElementsByTagNameNS('*', localName)?.[0]?.textContent ?? '';
			return {
				givenNames: getText('given-names'),
				familyName: getText('family-name'),
				creditName: getText('credit-name')
			};
		} catch {
			return { givenNames: '', familyName: '', creditName: '' };
		}
	}

	function selectOrcid(result: OrcidResult) {
		if (isSearchMode) {
			const displayName = result.creditName || `${result.givenNames} ${result.familyName}`.trim();
			value = displayName;
			ref = result.orcidUri;
			searchQuery = displayName;
			showResults = false;
			searchResults = [];
			syncOrcidValue();

			// If this field is mapped to a party, update the party id
			if (canLinkToParty && partyMappingObject) {
				const partyid = partyMappingObject.list?.find((item: any) =>
					item.value === displayName || item.value === result.orcidId
				)?.partyId ?? 0;

				if (!isComplexMapping) {
					updateMetadataStore(orcid_field_path, displayName, false, result.orcidUri, partyid);
				} else {
					const parentPath = getParentPath(orcid_field_path);
					const parentPathWithoutIndices = removeJsonPathIndices(parentPath);
					updateMetadataStore(parentPath, null, false, undefined, partyid);

					// update sibling fields linked to the same party
					$systemMappingsStore.partyMappings
						.filter((mapping: any) =>
							mapping.parentPath == parentPathWithoutIndices && mapping.path !== removeJsonPathIndices(orcid_field_path)
						)
						.forEach(async (mapping: any) => {
							const childvalue = await GetPartyValue(partyid, mapping.linkElementId);
							const childPathWithIndex = parentPath + '.' + mapping.path.split('.').slice(-1)[0];
							updateMetadataStore(childPathWithIndex, childvalue, false, undefined, undefined);
						});
				}
			}
		}
	}

	function selectValidateOrcid(result: OrcidResult) {
		console.log('ORCID selectValidateOrcid:', result);
		console.log('ORCID output path:', orcid_id_path);

		if (orcid_id_path) {
			console.log('Writing ORCID ID to:', orcid_id_path, '=', result.orcidId);
			updateMetadataStore(orcid_id_path, result.orcidId, false, result.orcidUri);
		} else {
			console.warn('ORCID: orcid_id_path is empty — output variable not connected');
		}

		// If the orcid field is mapped to a party, update the party id
		if (canLinkToParty && partyMappingObject) {
			const displayName = result.creditName || `${result.givenNames} ${result.familyName}`.trim();
			const partyid = partyMappingObject.list?.find((item: any) =>
				item.value === displayName || item.value === result.orcidId
			)?.partyId ?? 0;

			if (!isComplexMapping) {
				updateMetadataStore(orcid_field_path, displayName, false, result.orcidUri, partyid);
			} else {
				const parentPath = getParentPath(orcid_field_path);
				updateMetadataStore(parentPath, null, false, undefined, partyid);
			}
		}

		selectedCreditName = result.creditName || `${result.givenNames} ${result.familyName}`.trim();
		showValidateResults = false;
		validateResults = [];
	}

	function onKeydown(e: KeyboardEvent, results: OrcidResult[], isValidate: boolean) {
		const idx = isValidate ? selectedValidateIndex : selectedIndex;
		const show = isValidate ? showValidateResults : showResults;
		if (!show || results.length === 0) return;

		if (e.key === 'ArrowDown') {
			e.preventDefault();
			if (isValidate) selectedValidateIndex = Math.min(selectedValidateIndex + 1, results.length - 1);
			else selectedIndex = Math.min(selectedIndex + 1, results.length - 1);
		} else if (e.key === 'ArrowUp') {
			e.preventDefault();
			if (isValidate) selectedValidateIndex = Math.max(selectedValidateIndex - 1, 0);
			else selectedIndex = Math.max(selectedIndex - 1, 0);
		} else if (e.key === 'Enter' && idx >= 0) {
			e.preventDefault();
			if (isValidate) selectValidateOrcid(results[idx]);
			else selectOrcid(results[idx]);
		} else if (e.key === 'Escape') {
			if (isValidate) showValidateResults = false;
			else showResults = false;
		}
	}

	function onBlur(isValidate: boolean) {
		setTimeout(() => {
			if (isValidate) showValidateResults = false;
			else showResults = false;
		}, 200);
	}

	function onFocus(isValidate: boolean) {
		const results = isValidate ? validateResults : searchResults;
		if (results.length > 0) {
			if (isValidate) showValidateResults = true;
			else showResults = true;
		}
	}

	function onValidateSearchClick() {
		if (validateTimer) clearTimeout(validateTimer);
		validateTimer = setTimeout(() => {
			searchOrcidByName(givenNameValue, familyNameValue);
		}, 100);
	}

	function onValidateInput() {
		if (validateTimer) clearTimeout(validateTimer);
		if (!givenNameValue.trim() || !familyNameValue.trim()) {
			validateResults = [];
			showValidateResults = false;
			return;
		}
		validateTimer = setTimeout(() => {
			searchOrcidByName(givenNameValue, familyNameValue);
		}, debounceMs);
	}

	function updateOrcidValue(rorValue: any, _path: string) {
		res = suite(_path);
		updateValidationState(_path, res);
		const isNotEmpty = rorValue != null && String(rorValue).trim() !== '';
		if (required && !isNotEmpty) {
			validateCustomCondition(_path, false, 'Please select a person from ORCID.');
		}
	}

	function syncOrcidValue() {
		if (!validationRegistered) return;
		updateMetadataStore(
			orcid_field_path,
			value != undefined && value != null ? value.toString() : '',
			false,
			ref != undefined && ref != null ? ref.toString() : ''
		);
		updateOrcidValue(value, orcid_field_path);
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

	let selectedCreditName: string = '';

	$: currentOrcidId = (() => {
		$metadataStore;
		return orcid_id_path ? getValueByPath(orcid_id_path) : '';
	})();
	$: currentOrcidRef = (() => {
		$metadataStore;
		return orcid_id_path ? getRefByPath(orcid_id_path) : '';
	})();
</script>

{#if isSearchMode}
	<InputContainer {...commonProps} on:showDescription on:hideDescription>
		{#if canLinkToParty}
			<!-- Party-linked mode: show party autocomplete as primary, ORCID search as secondary -->
			<div class="flex items-start gap-2">
				<div class="grow">
					<MultiSelect
						{...commonProps}
						title={label}
						source={partyList}
						complexSource={true}
						complexTarget={true}
						itemId="partyId"
						itemLabel="value"
						bind:target={selectorValue}
						isMulti={false}
						clearable={true}
						searchable={false}
						on:change={onUpdateParty}
						on:clear={onUpdateParty}
					/>
				</div>
				<div class="pt-7 shrink-0 flex items-center gap-1">
					<button
						class="orcid-search-btn {showOrcidSearch ? 'orcid-search-btn-active' : ''}"
						title="Search at ORCID"
						on:click={() => (showOrcidSearch = !showOrcidSearch)}>
						{#if showOrcidSearch}
							<Fa icon={faXmark} class="text-xs" />
						{:else}
							<svg class="orcid-icon" viewBox="0 0 256 256" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
								<rect width="256" height="256" rx="32" fill="#A6CE39"/>
								<path d="M78.8 78.8h21.4v107.5H78.8V78.8z" fill="#fff"/>
								<circle cx="89.5" cy="53.5" r="13.5" fill="#fff"/>
								<path d="M139.5 78.8v8.6c6.7-6.7 16.4-10.7 28.1-10.7 28.5 0 49.6 21.1 49.6 49.6s-21.1 49.6-49.6 49.6c-11.7 0-21.4-4-28.1-10.7v55.1h-21.4V78.8h21.4zm23.9 79.4c15.7 0 27.2-12.3 27.2-28.9s-11.5-28.9-27.2-28.9-27.2 12.3-27.2 28.9 11.5 28.9 27.2 28.9z" fill="#fff"/>
							</svg>
						{/if}
					</button>
					{#if hasPartyId}
						<Fa icon={faCircleCheck} class="text-success-500" title="This field is linked to a party." />
					{:else}
						<Fa icon={faCircleQuestion} class="text-warning-500" title="This field can be linked to a party but has no party assigned yet." />
					{/if}
				</div>
			</div>

			{#if showOrcidSearch}
				<div class="orcid-search-container mt-2">
					<input
						type="text"
						class="orcid-input input variant-form-material"
						placeholder="Search for a person by name at ORCID..."
						bind:value={searchQuery}
						on:input={onSearchInput}
						on:keydown={(e) => onKeydown(e, searchResults, false)}
						on:blur={() => onBlur(false)}
						on:focus={() => onFocus(false)}
					/>
					{#if isLoading}
						<div class="orcid-loading">
							<span class="orcid-spinner"></span>
							<span>Searching...</span>
						</div>
					{/if}
					{#if showResults && searchResults.length > 0}
						<ul class="orcid-results">
							{#each searchResults as result, i}
								<li
									class="orcid-result-item"
									class:selected={i === selectedIndex}
									on:mousedown={() => selectOrcid(result)}
									on:mouseenter={() => (selectedIndex = i)}
									role="option"
									tabindex="-1"
								>
									<div class="orcid-result-name">
										{result.creditName || `${result.givenNames} ${result.familyName}`.trim() || result.orcidId}
									</div>
									<div class="orcid-result-meta">
										<span class="orcid-id">{result.orcidId}</span>
									</div>
								</li>
							{/each}
						</ul>
					{/if}
					{#if showResults && !isLoading && searchResults.length === 0 && searchQuery.trim().length >= 2}
						<div class="orcid-no-results">No ORCID records found for "{searchQuery}"</div>
					{/if}
				</div>
			{/if}

			{#if ref && !showOrcidSearch}
				<div class="orcid-selected-id">
					ORCID: <a href={ref} target="_blank" rel="noopener noreferrer">{ref}</a>
				</div>
			{/if}
		{:else}
			<!-- Non-party mode: original ORCID search -->
			<div class="flex items-start gap-2">
				<div class="orcid-search-container grow">
					<input
						type="text"
						class="orcid-input input variant-form-material {commonProps.valid ? 'input-success' : ''} {commonProps.invalid ? 'input-error' : ''}"
						placeholder="Search for a person by name..."
						bind:value={searchQuery}
						on:input={onSearchInput}
						on:keydown={(e) => onKeydown(e, searchResults, false)}
						on:blur={() => onBlur(false)}
						on:focus={() => onFocus(false)}
					/>
					{#if isLoading}
						<div class="orcid-loading">
							<span class="orcid-spinner"></span>
							<span>Searching...</span>
						</div>
					{/if}
					{#if showResults && searchResults.length > 0}
						<ul class="orcid-results">
							{#each searchResults as result, i}
								<li
									class="orcid-result-item"
									class:selected={i === selectedIndex}
									on:mousedown={() => selectOrcid(result)}
									on:mouseenter={() => (selectedIndex = i)}
									role="option"
									tabindex="-1"
								>
									<div class="orcid-result-name">
										{result.creditName || `${result.givenNames} ${result.familyName}`.trim() || result.orcidId}
									</div>
									<div class="orcid-result-meta">
										{#if result.givenNames || result.familyName}
											{#if result.creditName}
												<span>{result.givenNames} {result.familyName}</span>
											{/if}
										{/if}
										<span class="orcid-id">{result.orcidId}</span>
									</div>
								</li>
							{/each}
						</ul>
					{/if}
					{#if showResults && !isLoading && searchResults.length === 0 && searchQuery.trim().length >= 2}
						<div class="orcid-no-results">No ORCID records found for "{searchQuery}"</div>
					{/if}
					{#if ref}
						<div class="orcid-selected-id">
							Selected ORCID: <a href={ref} target="_blank" rel="noopener noreferrer">{ref}</a>
						</div>
					{/if}
				</div>
			</div>
		{/if}
	</InputContainer>
{:else if isValidateMode}
	<InputContainer {...commonProps} on:showDescription on:hideDescription>
		<div class="orcid-validate-container">
			<div class="orcid-validate-row">
				<div class="orcid-validate-field">
					<label class="orcid-validate-label">Given Name</label>
					<input
						type="text"
						class="orcid-input input variant-form-material"
						placeholder="Given name"
						bind:value={givenNameValue}
						on:input={onValidateInput}
					/>
				</div>
				<div class="orcid-validate-field">
					<label class="orcid-validate-label">Family Name</label>
					<input
						type="text"
						class="orcid-input input variant-form-material"
						placeholder="Family name"
						bind:value={familyNameValue}
						on:input={onValidateInput}
					/>
				</div>
			<button
				class="orcid-search-button"
				on:click={onValidateSearchClick}
				disabled={!givenNameValue.trim() || !familyNameValue.trim() || isValidateLoading}
			>
				{#if isValidateLoading}
					<span class="orcid-spinner orcid-spinner-sm"></span>
				{:else}
					<svg class="orcid-icon-sm" viewBox="0 0 256 256" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
						<rect width="256" height="256" rx="32" fill="#A6CE39"/>
						<path d="M78.8 78.8h21.4v107.5H78.8V78.8z" fill="#fff"/>
						<circle cx="89.5" cy="53.5" r="13.5" fill="#fff"/>
						<path d="M139.5 78.8v8.6c6.7-6.7 16.4-10.7 28.1-10.7 28.5 0 49.6 21.1 49.6 49.6s-21.1 49.6-49.6 49.6c-11.7 0-21.4-4-28.1-10.7v55.1h-21.4V78.8h21.4zm23.9 79.4c15.7 0 27.2-12.3 27.2-28.9s-11.5-28.9-27.2-28.9-27.2 12.3-27.2 28.9 11.5 28.9 27.2 28.9z" fill="#fff"/>
					</svg>
					Search
				{/if}
			</button>
			</div>

			{#if showValidateResults && validateResults.length > 0}
				<ul class="orcid-results">
					{#each validateResults as result, i}
						<li
							class="orcid-result-item"
							class:selected={i === selectedValidateIndex}
							on:mousedown={() => selectValidateOrcid(result)}
							on:mouseenter={() => (selectedValidateIndex = i)}
							role="option"
							tabindex="-1"
						>
							<div class="orcid-result-name">
								{result.creditName || `${result.givenNames} ${result.familyName}`.trim() || result.orcidId}
							</div>
							<div class="orcid-result-meta">
								{#if result.givenNames || result.familyName}
									{#if result.creditName}
										<span>{result.givenNames} {result.familyName}</span>
									{/if}
								{/if}
								<span class="orcid-id">{result.orcidId}</span>
							</div>
						</li>
					{/each}
				</ul>
			{/if}
			{#if showValidateResults && !isValidateLoading && validateResults.length === 0 && givenNameValue.trim() && familyNameValue.trim()}
				<div class="orcid-no-results">No ORCID records found for "{givenNameValue} {familyNameValue}"</div>
			{/if}

			{#if currentOrcidId || selectedCreditName}
				<div class="orcid-filled">
					{#if currentOrcidId}
						<div class="orcid-filled-row">
							<span class="orcid-filled-label">ORCID iD:</span>
							<a href={currentOrcidRef || `https://orcid.org/${currentOrcidId}`} target="_blank" rel="noopener noreferrer">{currentOrcidId}</a>
						</div>
					{/if}
					{#if selectedCreditName}
						<div class="orcid-filled-row">
							<span class="orcid-filled-label">Credit Name:</span>
							<span>{selectedCreditName}</span>
						</div>
					{/if}
				</div>
			{/if}
		</div>
	</InputContainer>
{/if}

<style>
	.orcid-search-btn {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		width: 1.5rem;
		height: 1.5rem;
		border: 1px solid #d4d4d4;
		border-radius: 4px;
		background: #fff;
		cursor: pointer;
		padding: 2px;
		transition: all 0.15s;
		flex-shrink: 0;
	}
	.orcid-search-btn:hover {
		border-color: #a6ce39;
		box-shadow: 0 0 4px rgba(166, 206, 57, 0.4);
	}
	.orcid-search-btn-active {
		border-color: #ef4444;
		background: #fef2f2;
	}
	.orcid-icon {
		width: 100%;
		height: 100%;
	}

	.orcid-search-container,
	.orcid-validate-container {
		position: relative;
		width: 100%;
	}

	.orcid-input {
		width: 100%;
		padding: 0.5rem 0.75rem;
		border-radius: 4px;
	}

	.orcid-loading {
		position: absolute;
		right: 0.75rem;
		top: 0.5rem;
		display: flex;
		align-items: center;
		gap: 0.35rem;
		font-size: 0.75rem;
		color: #888;
	}

	.orcid-spinner {
		display: inline-block;
		width: 0.85rem;
		height: 0.85rem;
		border: 2px solid #ccc;
		border-top-color: #a6ce39;
		border-radius: 50%;
		animation: orcid-spin 0.6s linear infinite;
	}
	.orcid-spinner-sm {
		width: 0.75rem;
		height: 0.75rem;
	}
	@keyframes orcid-spin {
		to {
			transform: rotate(360deg);
		}
	}

	.orcid-results {
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
	.orcid-result-item {
		padding: 0.5rem 0.75rem;
		cursor: pointer;
		border-bottom: 1px solid #f0f0f0;
		transition: background-color 0.15s;
	}
	.orcid-result-item:last-child {
		border-bottom: none;
	}
	.orcid-result-item.selected,
	.orcid-result-item:hover {
		background: #f0f7ee;
	}
	.orcid-result-name {
		font-size: 0.85rem;
		font-weight: 500;
		color: #333;
	}
	.orcid-result-meta {
		display: flex;
		justify-content: space-between;
		gap: 0.5rem;
		font-size: 0.75rem;
		color: #888;
		margin-top: 2px;
	}
	.orcid-id {
		font-family: monospace;
	}
	.orcid-no-results {
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
	.orcid-selected-id {
		margin-top: 0.25rem;
		font-size: 0.75rem;
		color: #888;
	}
	.orcid-selected-id a {
		color: rgb(37 99 235);
		font-family: monospace;
	}
	.orcid-selected-id a:hover {
		text-decoration: underline;
	}

	.orcid-validate-row {
		display: flex;
		gap: 0.5rem;
		align-items: flex-end;
	}
	.orcid-validate-field {
		flex: 1;
	}
	.orcid-validate-label {
		display: block;
		font-size: 0.75rem;
		font-weight: 500;
		color: #666;
		margin-bottom: 0.25rem;
	}
	.orcid-search-button {
		display: inline-flex;
		align-items: center;
		gap: 0.35rem;
		padding: 0.5rem 1rem;
		background: #a6ce39;
		color: white;
		border: none;
		border-radius: 4px;
		cursor: pointer;
		font-size: 0.85rem;
		font-weight: 500;
		white-space: nowrap;
		transition: background-color 0.2s;
	}
	.orcid-icon-sm {
		width: 1rem;
		height: 1rem;
		flex-shrink: 0;
	}
	.orcid-search-button:hover:not(:disabled) {
		background: #94b833;
	}
	.orcid-search-button:disabled {
		background: #ccc;
		cursor: not-allowed;
	}

	.orcid-filled {
		margin-top: 0.5rem;
		padding: 0.5rem 0.75rem;
		background: #f0f7ee;
		border: 1px solid #c8e6a0;
		border-radius: 4px;
	}
	.orcid-filled-row {
		display: flex;
		gap: 0.5rem;
		font-size: 0.8rem;
		margin-bottom: 0.15rem;
	}
	.orcid-filled-row:last-child {
		margin-bottom: 0;
	}
	.orcid-filled-label {
		font-weight: 500;
		color: #555;
		min-width: 5rem;
	}
	.orcid-filled-row a {
		color: rgb(37 99 235);
		font-family: monospace;
	}
	.orcid-filled-row a:hover {
		text-decoration: underline;
	}
</style>
