<script lang="ts">
	import { onMount, createEventDispatcher } from 'svelte';
	import {
		getFullConfig,
		getTargetVariablesWithValues,
		getValueByPath,
		updateMetadataStore,
		resolveNode,
		registerValidationItem,
		getMetadata,
		updateValidationState,
		validateCustomCondition
	} from '../../utils/metadata/metadataComponentUtils';
	import { DateInput, InputContainer } from '@bexis2/bexis2-core-ui';
	import suite from '$lib/components/utils/metadata/ComponentSuite';
	import { validationStore } from '$lib/components/utils/metadata/stores';

	const dispatch = createEventDispatcher();
	let res = suite.get();
	let componentName: string = 'date_range_picker_v1.0.0';

	export let anchor: string;
	export let path: string = '';
	export let mode: 'edit' | 'view' = 'edit';

	let config = getFullConfig(componentName, anchor, mode);
	if (!config) {
		console.error('No configuration found for component:', componentName, 'with anchor:', anchor);
	}
	let targetVars = getTargetVariablesWithValues(config);

	let modeName = config?.mode?.mode_name ?? '';
	let isViewMode = mode === 'view';

	let start_date_path = targetVars?.find((v) => v.target_variable === 'startDate')?.value ?? '';
	let end_date_path = targetVars?.find((v) => v.target_variable === 'endDate')?.value ?? '';

	const cleanPath = (p: string) => p ? p.replace(/^\$\.?/, '') : p;
	start_date_path = cleanPath(start_date_path);
	end_date_path = cleanPath(end_date_path);

	let startValue = start_date_path ? getValueByPath(start_date_path) ?? '' : '';
	let endValue = end_date_path ? getValueByPath(end_date_path) ?? '' : '';

	let { value: _v, ref: _r, label, description, required } = getMetadata(start_date_path || anchor);

	let descriptionCustom = targetVars?.find((v) => v.target_variable === 'description')?.value ?? '';
	if (descriptionCustom && descriptionCustom.trim() !== '') {
		description = descriptionCustom;
	}

	let disabled =
		(targetVars?.find((v) => v.target_variable == 'disable')?.value ?? false) === 'true';

	let validationRegistered = false;
	let validationReady = false;
	let rangeError: string = '';
	let endDateError: string = '';

	$: validationItemStart = $validationStore?.simpleTypeValidationItems?.find(
		(i) => i.path === start_date_path
	);

	$: validationItemEnd = $validationStore?.simpleTypeValidationItems?.find(
		(i) => i.path === end_date_path
	);

	onMount(async () => {
		if (isViewMode) return;

		if (start_date_path) {
			const { node: schemaNode } = resolveNode(start_date_path);
			registerValidationItem(start_date_path, label, required, schemaNode, true);
			validationRegistered = true;
		}

		if (end_date_path) {
			const { node: schemaNode } = resolveNode(end_date_path);
			registerValidationItem(end_date_path, label, required, schemaNode, true);
		}

		validationReady = true;
		validateRange();
	});

	function validateRange(): boolean {
		let isValid = true;
		rangeError = '';
		endDateError = '';

		// Check if start date is required but empty
		if (required && start_date_path) {
			const isStartEmpty = startValue == null || String(startValue).trim() === '';
			if (isStartEmpty) {
				rangeError = 'Please select a start date.';
				isValid = false;
			}
		}

		// Check if end date is required but empty
		if (required && end_date_path) {
			const isEndEmpty = endValue == null || String(endValue).trim() === '';
			if (isEndEmpty) {
				endDateError = 'Please select an end date.';
				isValid = false;
			}
		}

		// Check if both dates exist and start is after end
		if (startValue && endValue && !rangeError && !endDateError) {
			const start = new Date(startValue);
			const end = new Date(endValue);
			if (start > end) {
				rangeError = 'Start date must not be after the end date.';
				endDateError = 'End date must not be before the start date.';
				isValid = false;
			}
		}

		// Update validation for both fields
		if (start_date_path) {
			validateCustomCondition(start_date_path, !rangeError, rangeError);
		}
		if (end_date_path) {
			validateCustomCondition(end_date_path, !endDateError, endDateError);
		}

		return isValid;
	}

	function onStartChange(e: Event) {
		const input = e.target as HTMLInputElement;
		startValue = input.value || '';

		if (start_date_path) {
			updateMetadataStore(start_date_path, startValue, false, '');
		}

		// Validate the entire range when start date changes
		validateRange();

		// Update validation state for start field
		if (validationRegistered && start_date_path) {
			res = suite(start_date_path);
			updateValidationState(start_date_path, res);
		}

		// Update validation state for end field if it exists and has range error
		if (end_date_path && endDateError) {
			res = suite(end_date_path);
			updateValidationState(end_date_path, res);
		}

		dispatch('change');
	}

	function onEndChange(e: Event) {
		const input = e.target as HTMLInputElement;
		endValue = input.value || '';

		if (end_date_path) {
			updateMetadataStore(end_date_path, endValue, false, '');
		}

		// Validate the entire range when end date changes
		validateRange();

		// Update validation state for end field
		if (validationRegistered && end_date_path) {
			res = suite(end_date_path);
			updateValidationState(end_date_path, res);
		}

		// Update validation state for start field if it exists and has range error
		if (start_date_path && rangeError) {
			res = suite(start_date_path);
			updateValidationState(start_date_path, res);
		}

		dispatch('change');
	}

	$: startInvalid = validationReady && validationItemStart ? !validationItemStart.isValid : !!rangeError;
	$: endInvalid = validationReady && validationItemEnd ? !validationItemEnd.isValid : !!endDateError;
	
	$: startFeedback = rangeError
		? [rangeError]
		: validationItemStart && validationItemStart.errorMessage
			? validationItemStart.errorMessage.split('\n')
			: [];
	
	$: endFeedback = endDateError
		? [endDateError]
		: validationItemEnd && validationItemEnd.errorMessage
			? validationItemEnd.errorMessage.split('\n')
			: [];
</script>

{#if isViewMode}
	<div class="entry">
		<span class="key text-sm font-medium text-gray-600 dark:text-gray-300">{label}</span>
		<span class="val text-sm text-gray-900">
			{#if startValue || endValue}
				{startValue}{#if startValue && endValue} – {endValue}{/if}
			{:else}
				<span class="text-gray-500 dark:text-gray-400">—</span>
			{/if}
		</span>
	</div>
{:else}
		<div class="drp-row">
			<div class="drp-field">
				<DateInput
					id={`${path}-start`}
					label="Start"
					bind:value={startValue}
					invalid={startInvalid}
					valid={validationReady && validationItemStart ? validationItemStart.isValid && !rangeError : false}
					{required}
					{disabled}
					feedback={startFeedback}
					on:input={onStartChange}
					on:change={onStartChange}
					on:showDescription
					on:hideDescription
				/>
			</div>
			<div class="drp-field">
				<DateInput
					id={`${path}-end`}
					label="End"
					bind:value={endValue}
					invalid={endInvalid}
					valid={validationReady && validationItemEnd ? validationItemEnd.isValid && !endDateError : false}
					{required}
					{disabled}
					feedback={endFeedback}
					on:input={onEndChange}
					on:change={onEndChange}
					on:showDescription
					on:hideDescription
				/>
			</div>
		</div>
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
	.drp-row {
		display: flex;
		gap: 1rem;
		width: 100%;
	}
	.drp-field {
		flex: 1;
	}

	@media (max-width: 768px) {
		.val {
			width: 50vw;
		}
	}
</style>
