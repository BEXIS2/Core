<script>
	export let hooks;

	let visible = false;
</script>

<label>
	<input type="checkbox" bind:checked={visible} />
	show debug information
</label>

{#if hooks && visible}
	<hr />
	<b>debug infos:</b>
	{#each hooks as h}
		<p>{h.name}|{h.displayName}| Status : {h.status}</p>
	{/each}
{/if}
