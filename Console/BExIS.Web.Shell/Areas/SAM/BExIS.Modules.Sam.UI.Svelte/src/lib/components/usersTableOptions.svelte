<script lang="ts">
	import Fa from 'svelte-fa';
	import { faPen, faTrash } from '@fortawesome/free-solid-svg-icons';
	import type { ReadUserModel } from '../../routes/users/types';

	export let row: ReadUserModel;
	export let dispatchFn: (event: { type: string; row: ReadUserModel }) => void;
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
