<script lang="ts">
	import type { MeaningModel } from '$lib/components/meaning/types';

	export let value: boolean;
	export let row: MeaningModel;
</script>

<input type="checkbox" id={row.id.toString()} checked={value} disabled />
