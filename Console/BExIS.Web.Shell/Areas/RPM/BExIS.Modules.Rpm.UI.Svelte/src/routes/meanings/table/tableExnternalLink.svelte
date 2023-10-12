<script lang="ts">
	import type { externalLinkType } from "../types";

	export let value: externalLinkType[];
</script>

{#each value as link}
{#if link != undefined && link.name != undefined}
	<a href="{link.uRI}">{link.name}</a>
{/if}
{/each}