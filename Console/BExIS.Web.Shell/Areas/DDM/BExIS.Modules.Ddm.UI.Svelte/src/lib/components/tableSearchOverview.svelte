<script lang="ts">
	export let value: string;

    let title: string = "";
    if (value.length > 180) {
        title = value.slice(0, 180) + '...';
    }
</script>

<div>
	{#if value.length > 180}<span {title}>
			{value.slice(0, 180) + '...'}
		</span>
	{:else}
		<span>
			{value}
		</span>
	{/if}
</div>
