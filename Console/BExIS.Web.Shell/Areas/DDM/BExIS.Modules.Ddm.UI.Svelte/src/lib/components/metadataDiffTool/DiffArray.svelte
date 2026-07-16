<script lang="ts">
	import DiffNode from './DiffNode.svelte';

	export let level: number = 0;
	export let levelNames: string[] = [];
	export let value1: any;
	export let value2: any;
	export let useSimpleFormat: boolean = false;

	let lastLevelName = levelNames.at(-1);
</script>


<div class="rounded border border-surface-500 p-2 py-1">
	{#each value1 as v, i}
		<DiffNode
			level={level + 1}
			levelNames={[...levelNames, `[${i}] ${lastLevelName}`]}
			value1={v}
			value2={value2[i]}
			useSimpleFormat={useSimpleFormat}
		/>
	{/each}
	{#if value2.length > value1.length}
		{#each value2.slice(value1.length) as v, i}
			<DiffNode
				level={level + 1}
				levelNames={[...levelNames, `[${i}] ${lastLevelName}`]}
				value1={undefined}
				value2={v}
				useSimpleFormat={useSimpleFormat}
			/>
		{/each}
	{/if}
</div>
