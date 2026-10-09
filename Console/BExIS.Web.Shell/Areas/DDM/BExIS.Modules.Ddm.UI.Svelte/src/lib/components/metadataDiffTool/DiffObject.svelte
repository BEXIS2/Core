<script lang="ts">
	import DiffNode from './DiffNode.svelte';
	import DiffPrimitive from './DiffPrimitive.svelte';
	import { hasDiff } from './diffUtils';

	export let level: number = 0;
	export let levelNames: string[] = [];
	export let value1: any;
	export let value2: any;
	export let useSimpleFormat: boolean = false;
	export let hideUnchanged: boolean = true;

	let name = levelNames.at(-1);

	let keys = Object.keys({ ...value1, ...value2 });

	let attributes = keys.filter((k) => k.startsWith('@'));
	let textKey = keys.includes('#text') ? '#text' : null;
	let others = keys.filter((k) => !k.startsWith('@') && k !== '#text');

	$: visibleAttributes = hideUnchanged
		? attributes.filter((k) => hasDiff(value1?.[k], value2?.[k]))
		: attributes;
	$: hidden = hideUnchanged && !hasDiff(value1, value2);
</script>

{#if !hidden}
<div class="flex flex-col gap-1 rounded border border-surface-500 px-2 py-1">
	<div class="group">
		<div class="mb-1 flex flex-wrap items-center justify-between gap-x-2 gap-y-1">
			{#if name}
				<span class="mr-2 font-semibold">{name}</span>
			{/if}
			{#if visibleAttributes.length > 0}
				<div
					class="flex gap-3 rounded px-1 py-0.5 text-xs opacity-30 grayscale group-hover:opacity-100 group-hover:grayscale-0"
				>
					{#each visibleAttributes as k}
						<div>
							<strong>{k}:</strong>
							<DiffPrimitive value1={value1[k]} value2={value2[k]} useSimpleFormat={useSimpleFormat} {hideUnchanged} />
						</div>
					{/each}
				</div>
			{/if}
			<span class="font-mono text-xs text-surface-800 opacity-30 group-hover:opacity-100">
				{levelNames.join(' > ')}
			</span>
		</div>

		{#if textKey}
			<DiffPrimitive value1={value1[textKey]} value2={value2[textKey]} useSimpleFormat={useSimpleFormat} {hideUnchanged} />
		{/if}
	</div>

	{#each others as k}
		<DiffNode
			level={level + 1}
			levelNames={[...levelNames, k]}
			value1={value1[k]}
			value2={value2[k]}
			useSimpleFormat={useSimpleFormat}
			{hideUnchanged}
		/>
	{/each}
</div>
{/if}
