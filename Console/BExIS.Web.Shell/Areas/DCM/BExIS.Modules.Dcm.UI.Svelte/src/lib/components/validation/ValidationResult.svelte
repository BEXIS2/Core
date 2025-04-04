<script lang="ts">
	import type { Check } from '$models/ValidationModels';
	import { errorType, type sortedError, type sortedWarning } from '$models/Models';
	import { onMount } from 'svelte';

	import Fa from 'svelte-fa';
	import { faCheck, faXmark, faBan } from '@fortawesome/free-solid-svg-icons';
	import Message from './Message.svelte';

	export let file;
	export let sortedErrors: sortedError[];
	$: sortedErrors;

	export let sortedWarnings: sortedWarning[];
	$: sortedWarnings;

	let workflow: errorType[] = [
		errorType.Dataset,
		errorType.File,
		errorType.FileReader,
		errorType.Datastructure,
		errorType.Value,
		errorType.PrimaryKey,
		errorType.Other
	];

	let checkDisplayName = {
		['Dataset']: 'Dataset',
		['File']: 'File',
		['FileReader']: 'File Reader',
		['Datastructure']: 'Data Structure',
		['Value']: 'Value',
		['PrimaryKey']: 'Primary Key',
		['Other']: 'Other'
	};

	let checks: Check[] = [];
	$: checks;
	let selected: Check;
	$: selected;

	let errorCount = 0;
	let warningCount = 0;

	onMount(async () => {
		let faild: boolean = false;
		console.log('🚀 ~ sortedWarnings:', sortedWarnings);
		console.log('🚀 ~ sortedErrors:', sortedErrors);

		for (let index = 0; index < workflow.length; index++) {
			const type = workflow[index];
			const name = errorType[type];
			const errors = sortedErrors.filter((e) => e.type === type); // get list of sorted errors based on a type e.g. data structure or value
			const warnings = sortedWarnings.filter((e) => e.type === type); // get list of sorted warnings based on a type e.g. data structure or value
			const style = getStyle(errors.length, warnings.length, faild);

			let c: Check = { name, type, errors, warnings, style };

			errorCount += errors.length;
			warningCount += warnings.length;

			checks = [...checks, c];

			if (errors.length > 0 || warnings.length > 0) {
				faild = true;
				selected = c;
			}
		}

		//console.log('checks', checks);
	});

	function getStyle(errorCount, warningCount, faild) {
		if (errorCount > 0) return 'error';

		if (warningCount > 0) return 'warning';

		if (errorCount == 0 && faild) return 'surface';

		if (errorCount == 0 && warningCount == 0 && !faild) return 'success';

		return '';
	}
</script>

<div
	class="variant-ghost-success variant-ghost-error variant-ghost-surface variant-ghost-warning hidden"
/>

<div class="card p-5 space-y-3 mb-5">
	<div class="flex gap-1">
		<h4 class="h4">{file}</h4>
		{#if errorCount == 0 && warningCount == 0}
			<span class="text-success-500 px-1"><Fa icon={faCheck} /></span>
		{/if}
	</div>
	<div class="flex space-x-2">
		<ol class="breadcrumb">
			{#each checks as check, i}
				<li class="crumb">
					<button class="btn variant-ghost-{check.style} p-2 flex justify-center space-x-2">
						<span>{checkDisplayName[check.name]}</span>
						<span class="pt-1">
							{#if check.style == 'error'}
								<Fa icon={faXmark} />
							{/if}
							{#if check.style == 'success'}
								<Fa icon={faCheck} />
							{/if}
							{#if check.style == 'surface'}
								<Fa icon={faBan} />
							{/if}
							{#if check.style == 'warning'}
								<Fa icon={faXmark} />
							{/if}
						</span>
					</button>
				</li>
				{#if i < checks.length - 1}
					<li class="crumb-separator" aria-hidden>&rsaquo;</li>
				{/if}
			{/each}
		</ol>
	</div>

	{#if selected}
		<div class="card shadow-sm border-error-300 border-solid border">
			{#each selected.errors as error}
				<Message title={error.issue} count={error.count} messages={error.errors} />
			{/each}
			{#each selected.warnings as warning}
				<Message title={warning.issue} count={warning.count} messages={warning.warnings} />
			{/each}
		</div>
	{/if}
</div>
