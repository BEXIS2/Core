<script lang="ts">
	import type { externalLinkType } from "../types";

	export let value: externalLinkType[];
</script>

<div>
{#each value as link}

{#if link != undefined && link.name != undefined}
	<li><a href="{link.uri}" target="blank">{link.name}</a></li>
{/if}

{/each}
</div>