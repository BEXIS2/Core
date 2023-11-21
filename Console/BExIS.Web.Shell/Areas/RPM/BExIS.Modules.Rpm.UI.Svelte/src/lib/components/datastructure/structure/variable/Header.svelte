<script lang="ts">
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { Alert, TextInput } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faCheck, faAngleUp, faAngleDown } from '@fortawesome/free-solid-svg-icons';

	export let index;
	export let name: string;
	export let isKey: boolean;
	export let isOptional: boolean;
	export let isValid: boolean;
	export let expand: boolean;
</script>

<div id={name} class="flex gap-5">
	<div class="grow flex gap-2">
		<div class="cursor-pointer" on:click={() => (expand = !expand)}>
			<!--		<div class="cursor-pointer"  on:click={() => expand = !expand}> -->
			{#if expand}<Fa icon={faAngleUp} />
			{:else}
				<Fa icon={faAngleDown} />
			{/if}
		</div>

		<div>
			<slot />
		</div>

		<div class="text-success-500">
			{#if isValid}<Fa icon={faCheck} />{/if}
		</div>
	</div>

	<div class="flex-none max-w-5xl flex gap-2 text-right">
		{#if isKey && isOptional}
			<div>
				<Alert cssClass="variant-filled-warning" deleteBtn={false}>
					If variables are defined as part of the primary key and are optional, there may be
					complications when uploading.
				</Alert>
			</div>
		{/if}
		{#if isOptional}
			<div>
				<Alert cssClass="variant-filled-warning" deleteBtn={false}>
					Please consider briefly if it would be possible to define missing values instead of
					leaving the field optional.
				</Alert>
			</div>
		{/if}
	</div>
	<div class="flex-none flex-col text-right">
		<div>
			<SlideToggle size="sm" name="isKey" bind:checked={isKey} active="bg-primary-500"
				>Mark as part of primary keys</SlideToggle
			>
		</div>
		<div>
			<SlideToggle size="sm" name="isOptional" active="bg-primary-500" bind:checked={isOptional}
				>Value can be optional</SlideToggle
			>
		</div>
	</div>
</div>
