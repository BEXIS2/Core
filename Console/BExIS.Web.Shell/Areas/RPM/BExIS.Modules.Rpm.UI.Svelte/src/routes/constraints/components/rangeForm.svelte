<script lang="ts">
	import { CodeEditor, NumberInput, TextInput } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';
	import type { RangeConstraintListItem } from '../models';

	export let rangeConstraint: RangeConstraintListItem;
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
			/>
		</div>
	</div>
{/if}
