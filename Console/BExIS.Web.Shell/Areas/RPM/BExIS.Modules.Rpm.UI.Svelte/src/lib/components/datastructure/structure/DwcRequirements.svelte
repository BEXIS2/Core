<script lang="ts">
	import { MultiSelect } from '@bexis2/bexis2-core-ui';
	import { getDWCList } from '../services';
	import { onMount } from 'svelte';
	import { get } from 'svelte/store';
	import type { dwcExtention, VariableInstanceModel } from '../types';
	import { showDarwinCoreValidationStore } from '../store';

	export let variables: VariableInstanceModel[] = [];
	$: variables, validateDWCfn();

	let dwcExtensions: dwcExtention[] = [];
	let dwcSelection: dwcExtention | null = null;
	let notSet: string[] = [];
  let set: string[] = [];

	const isActive = get(showDarwinCoreValidationStore);

	onMount(async () => {
		// dwc	extensions
		dwcExtensions = await getDWCList();
		console.log('🚀 ~ start ~ dwcExtensions:', dwcExtensions);
	});

	function validateDWCfn() {
		if (!dwcSelection) return;

		// get all set meanings
		const allMeanings = variables.map((v) => v.meanings.map((m) => m.text)).flat();
		// get all not set required fields
		const notSetFields = dwcSelection.requiredFields.filter(
			(field) => !allMeanings.includes(field)
		);
		const setFields = dwcSelection.requiredFields.filter((field) => allMeanings.includes(field));

		notSet = [...notSetFields];
    set = [...setFields];
    console.log('🚀 ~ validateDWCfn ~ notSet:', notSet);
    console.log('🚀 ~ validateDWCfn ~ set:', set);

		if (notSet.length === 0) {
			notSet = [];
		}
	}
</script>
{#if dwcExtensions && isActive}

<div class="flex items-end gap-2 mt-4 mb-2 bg-gray-50 p-2 rounded-md border border-gray-200">
		<MultiSelect
			id="check_dwc"
			title="Check Darwin Core Requirements"
			source={dwcExtensions}
			bind:target={dwcSelection}
			itemId="name"
			itemLabel="name"
			complexSource={true}
			complexTarget={true}
			isMulti={false}
			on:change={() => validateDWCfn()}
			on:clear={() => {
				notSet = [];
				dwcSelection = null;
			}}
		/>
		{#if notSet}
      {#if set.length > 0}
        {#each set as field, i}
          <div><span class="chip variant-filled-success">{field}</span></div>
        {/each}
      {/if}
			{#if notSet.length > 0}
				{#each notSet as field, i}
					<div><span class="chip variant-filled-warning">{field}</span></div>
				{/each}
			{:else if dwcSelection}
				<div><span class="chip variant-filled-success">All required fields are set.</span></div>
			{/if}
		{/if}
</div>
	{/if}

