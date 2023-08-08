<script lang="ts">
	import type { missingValueType } from '$models/StructureSuggestion';

	export let type;
	export let missingValues: missingValueType[];
</script>

<div class="space-y-2">
	{#if type === 'string' && missingValues}
		<div>
			<b>String detected: </b> If not correct, please check if placeholder for missing values provided
			correctly.
		</div>
		<div>
			<b>Missing values: </b>
			{#if missingValues && missingValues.length > 0}
				{missingValues.map((m) => m.displayName).join(', ')}
			{/if}
		</div>
	{/if}
</div>
