<script lang="ts">
	import type { ExternalLink } from "../types";

	export let value: ExternalLink[];
</script>

{#each value as link}
{#if link != undefined && link.Name != undefined}
	<a href="{link.URI}">{link.Name}</a>
{/if}
{/each}