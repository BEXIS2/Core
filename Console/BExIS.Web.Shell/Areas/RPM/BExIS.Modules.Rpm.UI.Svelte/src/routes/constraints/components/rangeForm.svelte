<script lang="ts">
	import { CodeEditor, NumberInput, TextInput } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';
	import type { RangeConstraintListItem } from '../models';
	import { createEventDispatcher, onMount } from 'svelte';
	const dispatch = createEventDispatcher();

	import suite from './rangeForm';

	export let rangeConstraint: RangeConstraintListItem;

	// load form result object
	let res = suite.get();

	// use to actived save if form is valid
	$: disabled = !res.isValid();
	$: dispatch(String(disabled));

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		//console.log("input changed", e)
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			// check changed field
			res = suite(rangeConstraint , e);
		}, 10);
	}

	onMount(async () => {
		if (rangeConstraint.id == 0) {
			suite.reset();
		}
		else{
			setTimeout(async () => {	
				res = suite(rangeConstraint, "");
			}, 10);
		}
		

	});

	
</script>

{#if rangeConstraint}
	<div class="grid grid-cols-11 gap-5" in:slide out:slide>
		<div class="pb-3">
			<div>
				<!-- svelte-ignore a11y-label-has-associated-control -->
				<label>Negated</label>
				<input id="negated" type="checkbox" bind:checked={rangeConstraint.negated} />
			</div>
		</div>
		<div class="pb-5">
			<!-- svelte-ignore a11y-label-has-associated-control -->
			<label>Include</label>
			<input
				id="includeLowerbound"
				type="checkbox"
				bind:checked={rangeConstraint.lowerboundIncluded}
			/>
		</div>
		<div class="pb-5 col-span-4">
			<NumberInput
				id="lowerbound"
				label="Lower Bound"
				help={true}
				bind:value={rangeConstraint.lowerbound}
				on:input={onChangeHandler}
				valid={res.isValid('lowerbound')}
				invalid={res.hasErrors('lowerbound')}
				feedback={res.getErrors('lowerbound')}
			/>
		</div>
		<div class="pb-5">
			<!-- svelte-ignore a11y-label-has-associated-control -->
			<label>Include</label>
			<input
				id="includeUpperbound"
				type="checkbox"
				bind:checked={rangeConstraint.upperboundIncluded}
			/>
		</div>
		<div class="pb-5 col-span-4">
			<NumberInput
				id="upperbound"
				label="Upper Bound"
				help={true}
				bind:value={rangeConstraint.upperbound}
				on:input={onChangeHandler}
				valid={res.isValid('upperbound')}
				invalid={res.hasErrors('upperbound')}
				feedback={res.getErrors('upperbound')}
			/>
		</div>
	</div>
{/if}
