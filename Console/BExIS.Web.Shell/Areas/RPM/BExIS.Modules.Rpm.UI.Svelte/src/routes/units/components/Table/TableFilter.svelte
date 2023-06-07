<script lang="ts">
	import Fa from 'svelte-fa/src/fa.svelte';
	import { faFilter } from '@fortawesome/free-solid-svg-icons';
	import { popup } from '@skeletonlabs/skeleton';
	import type { PopupSettings } from '@skeletonlabs/skeleton';

	export let filterValue;
	export let values;

	let firstOption;
	let firstValue;
	let secondOption;
	let secondValue;

	const options = {
		number: [
			{
				value: 'isequal',
				label: 'Is equal to'
			},
			{
				value: 'isgreaterorequal',
				label: 'Is greater than or equal to'
			},
			{
				value: 'isgreater',
				label: 'Is greater than'
			},
			{
				value: 'islessorequal',
				label: 'Is less than or equal to'
			},
			{
				value: 'isless',
				label: 'Is less than'
			},
			{
				value: 'isnotequal',
				label: 'Is not equal to'
			}
		],
		string: [
			{
				value: 'isequal',
				label: 'Is equal to'
			},
			{
				value: 'isnotequal',
				label: 'Is not equal to'
			},
			{
				value: 'starts',
				label: 'Starts with'
			},
			{
				value: 'contains',
				label: 'Contains'
			},
			{
				value: 'notcontains',
				label: 'Does not contain'
			},
			{
				value: 'ends',
				label: 'Ends with'
			}
		]
	};

	const popupID = +new Date();
	const popupFeatured: PopupSettings = {
		event: 'click',
		target: `${popupID}`,
		placement: 'bottom-start'
	};

	const type = typeof $values[0];
</script>

<form class="">
	<button class="btn variant-filled-primary w-max p-2" type="button" use:popup={popupFeatured}>
		<Fa icon={faFilter} size="12" />
	</button>

	<div data-popup={`${popupID}`}>
		<div class="card p-3 absolute grid gap-2 shadow-lg z-10 w-min">
			<button
				class="btn variant-filled-primary btn-sm"
				type="submit"
				on:click={() => {
					firstOption = 'isequal';
					firstValue = undefined;
					secondOption = 'isequal';
					secondValue = undefined;

					$filterValue = [firstOption, firstValue, secondOption, secondValue];
				}}>Clear Filter</button
			>

			<label for="" class="label normal-case text-sm">Show rows with value that</label>
			<div class="grid gap-2 w-full">
				<select
					class="select border border-primary-500 text-sm p-1"
					aria-label="Show rows with value that"
					bind:value={firstOption}
					on:click|stopPropagation
				>
					{#each options[type] as option (option)}
						<option value={option.value}>{option.label}</option>
					{/each}
				</select>
				{#if type === 'number'}
					<input
						type="number"
						class="input p-1 border border-primary-500"
						bind:value={firstValue}
						on:click|stopPropagation
					/>
				{:else}
					<input
						type="text"
						class="input p-1 border border-primary-500"
						bind:value={firstValue}
						on:click|stopPropagation
					/>
				{/if}
			</div>
			<label for="" class="label normal-case">And</label>
			<div class="grid gap-2 w-max">
				<select
					class="select border border-primary-500 text-sm p-1"
					aria-label="Show rows with value that"
					bind:value={secondOption}
					on:click|stopPropagation
				>
					{#each options[type] as option (option)}
						<option value={option.value}>{option.label}</option>
					{/each}
				</select>
				{#if type === 'number'}
					<input
						type="number"
						class="input p-1 border border-primary-500"
						bind:value={secondValue}
						on:click|stopPropagation
					/>
				{:else}
					<input
						type="text"
						class="input p-1 border border-primary-500"
						bind:value={secondValue}
						on:click|stopPropagation
					/>
				{/if}
			</div>
			<button
				class="btn variant-filled-primary btn-sm"
				type="submit"
				on:click={() => {
					$filterValue = [firstOption, firstValue, secondOption, secondValue];
				}}>Submit</button
			>
		</div>
	</div>
</form>
