<script lang="ts">
    import Fa from 'svelte-fa';
	import { faEye, faPlus } from '@fortawesome/free-solid-svg-icons';
	import { type BasicDatasetInfo } from './data';

	export let row: BasicDatasetInfo;
	export let dispatchFn;

	const eventDispatchFn = (type: string) => {
		return dispatchFn({ type, row });
	};

    	const buttons = [
		{
			icon: faPlus,
			color: 'variant-filled-primary',
			type: 'BEGIN'
		},
		{
			icon: faEye,
			color: 'variant-filled-secondary',
			type: 'CONTINUE'
		},
	];
</script>

<div class="flex gap-2 w-min">
	{#if row.hasMatchingProgress}
		<button
			class={`btn btn-sm btn-icon rounded-md variant-filled-secondary`}
			on:click|preventDefault={() => eventDispatchFn('CONTINUE')}
		>
			<Fa icon={faEye} />
		</button>
	{:else}
		<button
			class={`btn btn-sm btn-icon rounded-md variant-filled-primary`}
			on:click|preventDefault={() => eventDispatchFn('BEGIN')}
		>
			<Fa icon={faPlus} />
		</button>
	{/if}
</div>