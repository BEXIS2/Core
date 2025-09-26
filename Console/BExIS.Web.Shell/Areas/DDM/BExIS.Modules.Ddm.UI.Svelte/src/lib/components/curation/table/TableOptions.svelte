<script lang="ts">
	import { faArrowUpRightFromSquare } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';

	export let row;
	export let dispatchFn;

	const eventDispatchFn = () => {
		const datasetId = row.datasetId;
		return dispatchFn({ datasetId });
	};
</script>

<button
	class="variant-filled-primary btn px-2 py-0.5 text-sm"
	on:click|preventDefault={eventDispatchFn}
>
	<Fa icon={faArrowUpRightFromSquare} class="mr-1 inline-block" />
	Open Curation
</button>
