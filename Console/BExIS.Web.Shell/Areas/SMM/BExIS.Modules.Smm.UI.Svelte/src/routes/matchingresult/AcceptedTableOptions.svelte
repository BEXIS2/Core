<script lang="ts">
    import Fa from 'svelte-fa';
	import { faEye, faPen, faTrash, faCheck } from '@fortawesome/free-solid-svg-icons';

	export let row;
	export let dispatchFn;

	const eventDispatchFn = (type: string) => {
		return dispatchFn({ type, row });
	};

    	const buttons = [
		{
			icon: faTrash,
			color: 'variant-filled-error',
			type: 'REMOVE'
		}
	];
</script>

<div class="flex gap-2 w-min">
	{#each buttons as button}
		<button
			class={`btn btn-sm btn-icon rounded-md ${button.color}`}
			on:click|preventDefault={() => eventDispatchFn(button.type)}
		>
			<Fa icon={button.icon} />
		</button>
	{/each}
</div>