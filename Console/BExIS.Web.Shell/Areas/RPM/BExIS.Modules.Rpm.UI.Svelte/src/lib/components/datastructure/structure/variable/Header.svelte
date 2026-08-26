<script lang="ts">
	import Fa from 'svelte-fa';
	import { faAngleUp, faAngleDown } from '@fortawesome/free-solid-svg-icons';
	import { changeablePrimaryKeyStore } from '../../store';
	import { get } from 'svelte/store';

	export let index = 0;
	export let name: string;
	export let isKey: boolean;
	export let isOptional: boolean;
	export let isValid: boolean;
	export let expand: boolean;
	export let blockDataRelevant: boolean;

	let changeablePrimaryKey: boolean = get(changeablePrimaryKeyStore);
</script>

	{#if isKey && isOptional}
		<div class="flex items-center gap-1 variant-ghost-warning warning border-l-4 border-warning-500 p-2 text-warning-800 dark:text-warning-200 pb-1" role="status"><span class="sr-only">Info:</span>
			If optional variables are part of the primary key, it can lead to problems during the import and updating of the dataset.		
		</div>
	{/if}
	{#if isOptional}
			<div class="flex items-center gap-1 variant-ghost-warning warning border-l-4 border-warning-500 p-2 text-warning-800 dark:text-warning-200 pb-1" role="status"><span class="sr-only">Info:</span>
			Please consider defining missing values instead of leaving the field optional.
		</div>
	{/if}


<div class="flex flex-col pt-2">
	<div id={index.toString()} class="flex">
		<div class="flex grow gap-2">
			<div
				class="cursor-pointer"
				role="button"
				tabindex="0"
				on:click={() => (expand = !expand)}
				on:keypress={() => (expand = !expand)}
			>
				{#if expand}<Fa icon={faAngleUp} />
				{:else}
					<Fa icon={faAngleDown} />
				{/if}
			</div>

			<div class="grow">
				<slot />
			</div>
		</div>

		<div class="flex-none flex-col text-right w-1/4">
			<div class="flex gap-2 pb-2 justify-end">
				<div>Primary key</div>
				<input class="checkbox" type="checkbox" bind:checked={isKey} disabled={blockDataRelevant && !changeablePrimaryKey} title="{isKey ? 'Variable is part of primary key' : 'Variable is not part of primary key'}"/>
			</div>
			<div class="flex gap-2 pb-2 justify-end">
				<div>Optional</div>
				<input class="checkbox" type="checkbox" bind:checked={isOptional} disabled={blockDataRelevant} title="{isOptional ? 'Variable allows empty values' : 'Variable does not allow empty values'}"/>
			</div>
		</div>
	</div>
</div>
