<script lang="ts">
	import type { missingValueType } from '$';

	export let type;
	export let missingValues: missingValueType[];
</script>

<div class="space-y-2">
	{#if type === 'string' && missingValues}
		<div>
			<b>String detected: </b> If not correct, please check if the placeholder for missing values is provided correctly.
		</div>
		<div>
			<b>Missing values: </b>
			{#if missingValues && missingValues.length > 0}
				{missingValues.map((m) => m.displayName).join(', ')}
			{/if}
		</div>
	{/if}
</div>
