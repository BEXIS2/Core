<script lang="ts">
	import Fa from 'svelte-fa';
	import { faPeopleGroup, faPen, faTrash } from '@fortawesome/free-solid-svg-icons';
	import type { ReadGroupModel } from "../../routes/groups/types";
	import type { ComponentType, SvelteComponent } from 'svelte';

	export let row: ReadGroupModel;
	export let dispatchFn: (event: { type: string; row: ReadGroupModel }) => void;
</script>

<div class="flex gap-2">
	<button
		class="chip rounded-md variant-filled-primary shadow-md"
		on:click|preventDefault={() => dispatchFn({ type: 'UPDATE', row })}
	>
		<Fa icon={faPen} />
	</button>
	<button
		class="chip rounded-md variant-filled-primary shadow-md"
		on:click|preventDefault={() => dispatchFn({ type: 'DELETE', row })}
	>
		<Fa icon={faTrash} />
	</button>
</div>