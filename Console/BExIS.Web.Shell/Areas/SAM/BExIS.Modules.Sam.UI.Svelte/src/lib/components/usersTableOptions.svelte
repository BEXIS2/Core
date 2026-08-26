
<script lang="ts">
	import Fa from 'svelte-fa';
	import { faPeopleGroup, faPen, faTrash } from '@fortawesome/free-solid-svg-icons';

	export let row;
	export let dispatchFn;

	const eventDispatchFn = (type: string) => {
		return dispatchFn({ type, row });
	};

	const buttons = [
		{
			icon: faPen,
			color: 'variant-filled-primary shadow-md',
			type: 'UPDATE'
		},
		{
			icon: faTrash,
			color: 'variant-filled-primary shadow-md',
			type: 'DELETE'
		}
	];
</script>

<div class="flex gap-2 w-min">
	{#each buttons as button}
		<button
			class={`chip rounded-md ${button.color}`}
			on:click|preventDefault={() => eventDispatchFn(button.type)}
		>
			<Fa icon={button.icon} />
		</button>
	{/each}
</div>
