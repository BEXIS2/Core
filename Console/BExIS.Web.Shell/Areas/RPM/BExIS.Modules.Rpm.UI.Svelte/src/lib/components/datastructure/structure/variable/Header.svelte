<script lang="ts">
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { Alert } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faAngleUp, faAngleDown } from '@fortawesome/free-solid-svg-icons';
	import Status from './Status.svelte';
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

<div class="flex-none grow flex gap-2 text-left">
	{#if isKey && isOptional}
		<div>
			<Alert cssClass="variant-filled-warning" deleteBtn={false}>
				If optional variables are part of the primary key, it can lead to problems during the import
				and updating of the dataset.
			</Alert>
		</div>
	{/if}
	{#if isOptional}
		<div>
			<Alert cssClass="variant-filled-warning" deleteBtn={false}>
				Please consider defining missing values instead of leaving the field optional.
			</Alert>
		</div>
	{/if}
</div>

<div class="flex flex-col">
	<div id={index.toString()} class="flex">
		<div class="flex grow gap-2">
			<div
				class="cursor-pointer"
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
				<div>Mark as part of primary key</div>
				<SlideToggle
					size="sm"
					name="isKey"
					title={isKey ? 'Part of primary key' : 'Not part of primary key'}
					bind:checked={isKey}
					active="bg-primary-500"
					disabled={blockDataRelevant && !changeablePrimaryKey}
				></SlideToggle>
			</div>
			<div class="flex gap-2 pb-2 justify-end">
				<div>Value can be optional</div>
				<SlideToggle
					size="sm"
					name="isOptional"
					title={isOptional ? 'Value can be optional' : 'Value is required'}
					active="bg-primary-500"
					bind:checked={isOptional}
					disabled={blockDataRelevant}
				></SlideToggle>
			</div>
		</div>
	</div>
</div>
