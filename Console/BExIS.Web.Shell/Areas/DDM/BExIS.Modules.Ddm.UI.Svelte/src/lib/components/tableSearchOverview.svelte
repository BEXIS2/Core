<script lang="ts">
	export let value: string;
</script>

<div>
	<span title="{value}">
        {#if value.length > 200}
            {value.slice(0, 200) + '...'}
        {:else}
            {value}
        {/if}
		
	</span>
</div>