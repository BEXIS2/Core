<script lang="ts">
    import Fa from 'svelte-fa';
	import { faEye, faPen, faTrash, faCheck } from '@fortawesome/free-solid-svg-icons';
    import type { StepEntry } from '$lib/types/types';

	export let row: StepEntry;
	export let dispatchFn;

	const eventDispatchFn = (type: string) => {
		return dispatchFn({ type, row });
	};

</script>

<div class="flex gap-2 w-min">
	{#if row.done}
        <button
			class={`btn btn-sm btn-icon rounded-md variant-filled-primary`}
			on:click|preventDefault={() => eventDispatchFn('READ')}
		>
			<Fa icon={faEye} />
		</button>
    {:else}
        <button
			class={`btn btn-sm btn-icon rounded-md variant-filled-secondary`}
			on:click|preventDefault={() => eventDispatchFn('READ')}
		>
			<Fa icon={faEye} />
		</button>
    {/if}
</div>