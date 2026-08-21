<script lang="ts">
	import { onMount } from 'svelte';
	import {
		updateMetadataStore,
		getFullConfig,
		getTargetVariablesWithValues,
		ValidationStoreSetSimpleTypeValid,
		resolveNode,
		updateValidationState,
		registerValidationItem,
		getMetadata,
		validateCustomCondition
	} from '../../utils/metadata/metadataComponentUtils';
	import * as ts4nfdiWidgets from '@ts4nfdi/terminology-service-suite-js';
	import { InputContainer } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faExternalLinkAlt } from '@fortawesome/free-solid-svg-icons';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';
	import { validationStore } from '$lib/components/utils/metadata/stores';

	let res = suite.get();
	let componentName: string = 'terminology_v1.0.0';

	export let anchor: string;
	export let path: string = '';
	export let mode: 'edit' | 'view' = 'edit';
	console.log('🚀 ~ anchor:', anchor, 'path:', path);

	let config = getFullConfig(componentName, anchor, mode);
	let targetVars = getTargetVariablesWithValues(config);
	console.log('Target Variables with Values:', targetVars, config);

	let modeName = config?.mode?.mode_name ?? '';
	let isViewMode = mode === 'view';
	let showDescriptionInView = isViewMode && modeName === 'Linked Terminology Display with Description';
	console.log(showDescriptionInView, isViewMode, mode, "mode");
	let term_field_path = targetVars?.find((v) => v.target_variable === 'term_field')?.value
		?? targetVars?.find((v) => v.target_variable === 'displayTerm')?.value
		?? '';
	if (term_field_path && term_field_path == anchor.split('.').slice(0, -1).join('.')) {
		term_field_path = anchor;
	}
	let { value, ref, label, description, required } = getMetadata(term_field_path);
	let validationRegistered = false;
	let validationReady = false;
	let viewDescription: string | null = null;

	$: validationItem = $validationStore?.simpleTypeValidationItems?.find(
		(i) => i.path === term_field_path
	);

	console.log(targetVars);
	let parameter = targetVars?.find((v) => v.target_variable === 'parameter')?.value ?? '';
	// console.log('Parameter for terminology widget:', parameter);
	let allowCustomTerms =
		targetVars?.find((v) => v.target_variable === 'allowCustomTerms')?.value ?? false;
	let rawUrl = targetVars?.find((v) => v.target_variable === 'TerminologyServiceUrl')?.value;
	let TerminologyServiceUrl =
		rawUrl && rawUrl.trim() !== '' ? rawUrl : 'https://semanticlookup.zbmed.de/api/';

	// check for custom description from target variables, if not use default description
	let descriptionCustom = targetVars?.find((v) => v.target_variable === 'description')?.value ?? '';
	if (descriptionCustom && descriptionCustom.trim() !== '') {
		description = descriptionCustom;
	}

	let containerElement: HTMLDivElement;
	let data: Array<{ iri: string; label: string }> = [];
	let preselectedItems: Array<{ iri: string; label: string }> = [];
	if (value && ref) preselectedItems = [{ iri: ref.toString(), label: value.toString() }];
	console.log('Preselected Items:', preselectedItems, value, ref);

	let isValidTerm: boolean = true;
	let isRunningService: boolean = true;
	onMount(async () => {
		if (isViewMode) {
			if (showDescriptionInView && ref) {
				try {
					viewDescription = await getDescriptionFromAPI(ref.toString());
				} catch (e) {
					console.error('Error fetching description for view mode:', e);
					viewDescription = 'No description available';
				}
			}
			return;
		}

		const { node: schemaNode } = resolveNode(term_field_path);
		// The terminology component has custom validation rules, including for optional fields.
		registerValidationItem(term_field_path, label, required, schemaNode, true);
		validationRegistered = true;
		syncTermValue();

		// check if terminology service is running
		isRunningService = await checkService(TerminologyServiceUrl);

		// check if preselected term is valid / exists in current terminology service
		if (ref && isRunningService) {
			isValidTerm = await validatePreselectedTerm(ref.toString());
		}

		// init service if it is running
		if (containerElement && isRunningService) {
			// console.log('Rendering TS4NFDI AutocompleteWidget...');

			try {
				ts4nfdiWidgets.createAutocomplete(
					{
						api: TerminologyServiceUrl,
						selectionChangedEvent: (props) => {
							value = props.map((item) => item.label).toString();
							ref = props.map((item) => item.iri).toString();
							data = props;
							syncTermValue();
						},
						parameter: parameter,
						placeholder: 'Select a term within pre-selected ontologies ..',
						singleSelection: true,
						preselected: preselectedItems,
						className: 'tswidget-input',
						allowCustomTerms: allowCustomTerms
					},
					containerElement
				);

				// console.log('TS4NFDI AutocompleteWidget rendered.');
			} catch (error) {
				console.error('Error creating autocomplete widget:', error);
			}
		} else {
			console.warn(
				'Autocomplete widget will not be rendered. Container element is missing or term is invalid.'
			);
		}
	});

	// Keep validation state when component is temporarily unmounted (e.g., collapsed section).

	async function validatePreselectedTerm(refValue: string): Promise<boolean> {
		try {
			const response = await fetch(
				TerminologyServiceUrl + `terms?iri=${encodeURIComponent(refValue)}`
			);
			if (!response.ok) {
				console.warn('Terminology service returned non-OK status, keeping existing value.');
				return false;
			}
			const data = await response.json();
			const terms = data?._embedded?.terms;
			if (!terms || terms.length === 0) {
				console.warn('Term not found in terminology service.');
				return false;
			}
			return true;
		} catch (error) {
			console.error('Error validating preselected term, keeping existing value:', error);
			return false; // network error or other issue like it does not exist, keep existing value
		}
	}

	// Check if the terminology service is running by making a simple request to the base URL.
	async function checkService(url: string): Promise<boolean> {
		try {
			const response = await fetch(url);
			return true;
		} catch (error) {
			console.error('Error checking terminology service:', error);
			return false;
		}
	}

	// Update the value in the metadata store and validate it.
	function updateValue(value: any, _path: string) {
		res = suite(_path);
		updateValidationState(_path, res);

		const isNotEmpty = value != null && String(value).trim() !== '';
		// console.log('🚀 ~ updateValue ~ path:', _path, 'value:', value, 'isNotEmpty:', isNotEmpty);
		if (required && !isNotEmpty) {
			validateCustomCondition(_path, false, 'Please select a term from the terminology service.');
		} 
	}

	// Sync the term value and reference to the metadata store and update validation state.
	function syncTermValue() {
		if (!validationRegistered) return;

		updateMetadataStore(
			term_field_path,
			value != undefined && value != null ? value.toString() : '',
			false,
			ref != undefined && ref != null ? ref.toString() : ''
		);
		updateValue(value, term_field_path);
		validationReady = true;
	}

	// Fetch the description of a term from the terminology service API using its IRI.
	async function getDescriptionFromAPI(ref: string): Promise<string> {
		
		// Return the description as a string
		// https://semanticlookup.zbmed.de/ols/api/terms?iri=http:%2F%2Fpurl.obolibrary.org%2Fobo%2FNCBITaxon_146500
		const response = await fetch(TerminologyServiceUrl + `terms?iri=${encodeURIComponent(ref)}`);
		const data = await response.json();
		console.log('🚀 ~ getDescriptionFromAPI ~ data:', data._embedded.terms[0].description[0]);
		if (!data?._embedded?.terms?.length) {
			console.warn('Term not found in terminology service.');
			return 'No description available';
		}
		let description = data._embedded.terms[0].description[0];
		if (!description) {
			// console.warn('Term not found in terminology service.');
			return 'No description available';
		}
		return description;
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
		<span class="val text-sm text-gray-900">
			{#if value}
				{#if ref}
					<a href={ref} target="_blank" rel="noopener noreferrer" class="term-link">
						<span>{value}</span>
						<Fa icon={faExternalLinkAlt} class="term-link-icon" />
					</a>
				{:else}
					{value}
				{/if}
				{#if showDescriptionInView && viewDescription !== null}
					<span class="term-desc">{viewDescription}</span>
				{/if}
			{:else}
				<span class="text-gray-400">—</span>
			{/if}
		</span>
	</div>
{:else}
	<InputContainer {...commonProps} on:showDescription on:hideDescription>
		{#if !isRunningService}
			<div class="text-error-500 text-sm mt-1">
				The terminology service is currently unavailable. Please try again later or report the issue
				to the data manager.
			</div>
			<div>Service URL: {TerminologyServiceUrl}</div>
		{/if}
		{#if isRunningService && !isValidTerm}
			<div class="text-error-500 text-sm mt-1">
				The previously selected term is no longer available in the terminology service.
			</div>
			<div>Term: {value} (IRI: {ref})</div>
			<div>Service URL: {TerminologyServiceUrl}</div>
		{:else if isRunningService}
			<div
				bind:this={containerElement}
				class="tswidget-host input variant-form-material {commonProps.valid
					? 'input-success'
					: ''} {commonProps.invalid ? 'input-error' : ''} {commonProps.disabled
					? 'opacity-60 pointer-events-none'
					: ''}"
			></div>
		{/if}
		{#if data}
			<ul>
				{#each data as item}
					{#await getDescriptionFromAPI(item.iri) then description}
						<li title={description.toString()} class="text-xs text-gray-500 mt-1">
							(<a href={item.iri} target="_blank" rel="noopener noreferrer">
								{item.iri}
							</a>)
						</li>
					{/await}
				{/each}
			</ul>
		{/if}
	</InputContainer>
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

	.term-link {
		display: inline-flex;
		align-items: center;
		gap: 0.25rem;
		color: rgb(37 99 235);
	}

	.term-link:hover {
		text-decoration: underline;
	}

	.term-link-icon {
		font-size: 0.7rem;
		opacity: 0.6;
	}

	.term-desc {
		display: block;
		font-size: 0.75rem;
		font-weight: normal;
		color: rgb(107 114 128);
		margin-top: 0.25rem;
	}

	:global(.tswidget-host) {
		display: flex;
		align-items: center;
		width: 100%;
		min-height: 2.625rem;
	}
	:global(.tswidget-host .tswidget-input) {
		width: 100%;
	}
	:global(.tswidget-host .tswidget-input:focus-within) {
		outline: none !important;
		box-shadow: none !important;
		border-color: inherit !important;
	}
	:global(.tswidget-host:focus-within) {
		box-shadow: none !important;
		outline: none !important;
		border-color: inherit !important;
	}
	:global(.tswidget-host .tswidget-input input) {
		width: 100%;
		height: 100%;
		background: transparent;
		border: 0;
		outline: none;
		padding: 0 0.75rem;
		font: inherit;
		color: inherit;
	}
	:global(.tswidget-host .tswidget-input input:focus),
	:global(.tswidget-host .tswidget-input input:focus-visible) {
		outline: none !important;
		box-shadow: none !important;
		border-color: transparent !important;
	}
	:global(.tswidget-host .tswidget-input *:focus),
	:global(.tswidget-host .tswidget-input *:focus-visible) {
		outline: none !important;
		box-shadow: none !important;
		border-color: inherit !important;
		--tw-ring-color: transparent !important;
		--tw-ring-offset-shadow: 0 0 #0000 !important;
		--tw-ring-shadow: 0 0 #0000 !important;
		--euiFormControlStateColor: transparent !important;
		--euiFormControlStateHoverColor: transparent !important;
		--euiFormControlStateWidth: 0 !important;
	}

	:global(.css-hakdsy-euiComboBoxInputWrapper-plainText) {
		outline: none !important;
	}

	:global(.euiComboBox),
	:global(.euiComboBox__inputWrap),
	:global(.euiComboBox__input) {
		outline: none !important;
		box-shadow: none !important;
		border-color: transparent !important;
	}

	:global(.tswidget-host .tswidget-input input::placeholder) {
		opacity: 0.6;
	}

	:global([class*='euiComboBox__inputWrap']) {
		outline: none !important;
		box-shadow: none !important;
	}

	:global([class*='euiComboBox__inputWrap']:focus-within) {
		outline: none !important;
		box-shadow: none !important;
	}

	@media (max-width: 768px) {
		.val {
			width: 50vw;
		}
	}
</style>
