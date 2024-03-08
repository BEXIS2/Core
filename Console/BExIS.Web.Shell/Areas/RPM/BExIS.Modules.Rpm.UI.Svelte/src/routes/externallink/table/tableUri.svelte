<script lang="ts">
	import type { externalLinkType } from '$lib/components/meaning/types';
	export let value: string;
</script>

<div>
	<a href={value} target="blank">{value}</a>
</div>
