<script lang="ts">
	export let id;
	export let version;
	export let title = '';
	export let labels:any[]	= [];
	console.log('header labels', labels);
	console.log('header labels 2', );
	console.log('header labels 2',labels[Object.keys(labels)[0]] );

	const labelKeys = Object.keys(labels);


</script>

<div class="flex pb-5">	
	<div class="flex-grow">
		<h2 class="h2">{title}</h2>
	</div>
	<div>

		{#if labelKeys	&& labelKeys.length > 0}
				{#each labelKeys as key}
					<span class="badge variant-filled-primary mr-2">{key}</span>
				{/each}
		{/if}
		
	</div>
</div>
