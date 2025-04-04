<script lang="ts">
	import Fa from 'svelte-fa';
	import { faFileImport } from '@fortawesome/free-solid-svg-icons';
	import { helpStore } from '@bexis2/bexis2-core-ui';

	export let row: any;
	export let dispatchFn: any;
</script>

<tableOption>
	<div class="w-10">
		<!-- svelte-ignore a11y-mouse-events-have-key-events -->
		<button
			type="button"
			class="chip variant-filled-primary shadow-md"
			title="Select Variable Template, {row.name}"
			id="select-{row.id}"
			on:click|preventDefault={() =>
				dispatchFn({
					type: { action: 'select', id: row.id }
				})}
		>
			<Fa icon={faFileImport} />
		</button>

	</div>
</tableOption>
