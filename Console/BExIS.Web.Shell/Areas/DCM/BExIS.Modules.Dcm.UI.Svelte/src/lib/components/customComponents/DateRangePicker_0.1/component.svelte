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
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';
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

	$: validationItem = $validationStore?.simpleTypeValidationItems?.find(
		(i) => i.path === start_date_path
	);

	onMount(async () => {
		if (isViewMode) return;

		if (start_date_path) {
			const { node: schemaNode } = resolveNode(start_date_path);
			registerValidationItem(start_date_path, label, required, schemaNode, true);
			validationRegistered = true;
			validationReady = true;
			validateRange();
		}
	});

	function validateRange(): boolean {
		if (startValue && endValue) {
			const start = new Date(startValue);
			const end = new Date(endValue);
			if (start > end) {
				rangeError = 'Start date must not be after the end date.';
				if (validationRegistered && start_date_path) {
					validateCustomCondition(start_date_path, false, rangeError);
				}
				return false;
			}
		}
		rangeError = '';
		return true;
	}

	function onStartChange(e: Event) {
		const input = e.target as HTMLInputElement;
		startValue = input.value || '';

		if (start_date_path) {
			updateMetadataStore(start_date_path, startValue, false, '');
		}

		if (!validateRange() && end_date_path) {
			validateCustomCondition(end_date_path, false, 'Start date is after end date.');
		} else if (end_date_path) {
			validateCustomCondition(end_date_path, true, '');
		}

		if (validationRegistered && start_date_path) {
			res = suite(start_date_path);
			updateValidationState(start_date_path, res);

			const isNotEmpty = startValue != null && String(startValue).trim() !== '';
			if (required && !isNotEmpty) {
				validateCustomCondition(start_date_path, false, 'Please select a start date.');
			}
		}

		dispatch('change');
	}

	function onEndChange(e: Event) {
		const input = e.target as HTMLInputElement;
		endValue = input.value || '';

		if (end_date_path) {
			updateMetadataStore(end_date_path, endValue, false, '');
		}

		if (!validateRange() && start_date_path) {
			validateCustomCondition(start_date_path, false, 'End date is before start date.');
		} else if (start_date_path && !rangeError) {
			const isNotEmpty = startValue != null && String(startValue).trim() !== '';
			if (required && !isNotEmpty) {
				validateCustomCondition(start_date_path, false, 'Please select a start date.');
			} else {
				res = suite(start_date_path);
				updateValidationState(start_date_path, res);
			}
		}

		dispatch('change');
	}

	$: startInvalid = validationReady && validationItem ? !validationItem.isValid : false;
	$: endInvalid = !!rangeError;
	$: startFeedback = rangeError
		? [rangeError]
		: validationItem && validationItem.errorMessage
			? validationItem.errorMessage.split('\n')
			: [];
</script>

{#if isViewMode}
	<div class="entry">
		<span class="key text-sm font-medium text-gray-500">{label}</span>
		<span class="val text-sm text-gray-900 font-semibold">
			{#if startValue || endValue}
				{startValue}{#if startValue && endValue} – {endValue}{/if}
			{:else}
				<span class="text-gray-400">—</span>
			{/if}
		</span>
	</div>
{:else}
	<InputContainer
		id={path}
		{label}
		feedback={startFeedback}
		{required}
		{description}
		showDescription={false}
		showIcon={false}
		on:showDescription
		on:hideDescription
	>
		<div class="drp-row">
			<div class="drp-field">
				<DateInput
					id={`${path}-start`}
					label="Start"
					bind:value={startValue}
					invalid={startInvalid}
					valid={validationReady && validationItem ? validationItem.isValid && !rangeError : false}
					{required}
					{disabled}
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
					valid={!rangeError && !!endValue}
					{disabled}
					feedback={rangeError ? [rangeError] : []}
					on:input={onEndChange}
					on:change={onEndChange}
					on:showDescription
					on:hideDescription
				/>
			</div>
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
	.drp-row {
		display: flex;
		gap: 1rem;
		width: 100%;
	}
	.drp-field {
		flex: 1;
	}
</style>
