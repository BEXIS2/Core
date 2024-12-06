<script lang="ts">
	import { faEye, faArrowAltCircleRight, faRepeat, faTrash } from '@fortawesome/free-solid-svg-icons';
	import { helpStore } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';


	export let row: any;
	export let dispatchFn: any;

	let hasRequest = row.status === 'Registered';

</script>

<tableOption>
	<div class="w-18 flex gap-1" id={row.id}>
			<!-- svelte-ignore a11y-mouse-events-have-key-events -->
			<button
				type="button"
				class="chip variant-filled-primary shadow-md"
				title="show, {row.title}"
				id="show-{row.id}"
				on:mouseover={() => {
					helpStore.show('show');
				}}
				on:click|preventDefault={() =>
					dispatchFn({
						type: { action: 'show', id: row.datasetId, version:row.datasetVersionNr, publicationId: row.publicationId }
					})}
			>
				<Fa icon={faEye} />
			</button>
			<!-- svelte-ignore a11y-mouse-events-have-key-events -->
			<button
				type="button"
				class="chip variant-filled-warning shadow-md"
				title="request, {row.name}"
				id="request-{row.id}"
				disabled={hasRequest}
				on:mouseover={() => {
					helpStore.show('request');
				}}
				on:click|preventDefault={() =>
					dispatchFn({
						type: { action: 'request', id: row.datasetId, version:row.datasetVersionNr, publicationId: row.publicationId }
					})}
			>
				<Fa icon={faArrowAltCircleRight} /></button
			>
  
			{#if hasRequest}
			<button
			type="button"
			class="chip variant-filled-warning shadow-md"
			title="update, {row.name}"
			id="update-{row.id}" on:click={()=>alert("not implemented")}	>
			<Fa icon={faRepeat} /></button			>
			<button
			type="button"
			class="chip variant-filled-error shadow-md"
			title="delete, {row.name}"
			id="delete-{row.id}"	on:click={()=>alert("not implemented")}>
			<Fa icon={faTrash} /></button			>


			{/if}


	</div>
</tableOption>
