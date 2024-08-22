<script lang="ts">
	import Fa from 'svelte-fa';
	import { faSave } from '@fortawesome/free-solid-svg-icons';

	export let row;
	export let dispatchFn;

	const eventDispatchFn = (type: string) => {
		return dispatchFn({ type, row });
	};

	const buttons = [
		{
			icon: faSave,
			color: 'variant-filled-primary',
			type: 'SAVE'
		}
	];
</script>

<div class="flex gap-2 w-min p-2">
	{#each buttons as button}
		<button
			class="btn btn-sm variant-filled-primary ${button.color}"
			on:click|preventDefault={() => eventDispatchFn(button.type)}
		>
			<Fa icon={button.icon} />
		</button>
	{/each}
</div>