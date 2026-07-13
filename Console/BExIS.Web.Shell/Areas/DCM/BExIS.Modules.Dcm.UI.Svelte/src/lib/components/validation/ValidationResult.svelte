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
	<div class="pb-2">
		<ol class="flex flex-row items-center gap-1 w-full max-w-full overflow-hidden">
			{#each checks as check, i}
				<li class="crumb flex-1 min-w-0 flex items-center">
					<button
						class="btn variant-ghost-{check.style} py-2 px-1 w-full max-w-[11rem] flex justify-center items-center space-x-1 sm:space-x-2"
						title="{checkDisplayName[check.name]}: {check.errors.length} errors, {check.warnings
							.length} warnings"
					>
						<span class="truncate text-xs sm:text-sm">{checkDisplayName[check.name]}</span>
						<span class="inline-flex items-center flex-shrink-0">
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
					<li class="crumb-separator flex items-center flex-shrink-0" aria-hidden>&rsaquo;</li>
				{/if}
			{/each}
		</ol>
	</div>

	{#if selected}
		<div
			class="flex items-center gap-1 variant-ghost-warning warning border-l-4 border-warning-500 p-2 text-warning-800 dark:text-warning-200"
			role="status"
		>
			Please correct your data and upload again or edit the data structure accordingly. Validation
			will be performed again based on the changes. {#if selected.errors.length > 990} Only the first ~1000 errors are shown. {/if} {#if selected.warnings.length > 990} Only the first ~1000 warnings are shown. {/if}
		</div>
		<div class="card shadow-sm border-error-300 border-solid border">
			{#each selected.errors as error}
				<Message title={error.issue} count={error.count} messages={error.errors} type="error" />
			{/each}
			{#each selected.warnings as warning}
				<Message title={warning.issue} count={warning.count} messages={warning.warnings} type="warning" />
			{/each}
		</div>
	{/if}
</div>
