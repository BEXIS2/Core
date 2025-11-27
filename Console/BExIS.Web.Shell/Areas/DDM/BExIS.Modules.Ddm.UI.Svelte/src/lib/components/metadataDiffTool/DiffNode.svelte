<script lang="ts">
	import DiffPrimitive from './DiffPrimitive.svelte';
	import DiffObject from './DiffObject.svelte';
	import DiffArray from './DiffArray.svelte';

	export let level: number = 0;
	export let levelNames: string[] = [];
	export let value1: any;
	export let value2: any;

	function isObject(val: unknown) {
		return val && typeof val === 'object' && !Array.isArray(val);
	}

	function isArray(val: unknown) {
		return Array.isArray(val);
	}

	function isPrimitive(val: unknown) {
		return (
			val === null ||
			typeof val === 'string' ||
			typeof val === 'number' ||
			typeof val === 'boolean' ||
			typeof val === 'undefined'
		);
	}
</script>

{#if isPrimitive(value1) || isPrimitive(value2)}
	<DiffPrimitive
		value1={isPrimitive(value1) ? value1 : undefined}
		value2={isPrimitive(value2) ? value2 : undefined}
	/>
{:else if isArray(value1) || isArray(value2)}
	<DiffArray
		{levelNames}
		value1={isArray(value1) ? value1 : []}
		value2={isArray(value2) ? value2 : []}
		level={level + 1}
	/>
{:else if isObject(value1) || isObject(value2)}
	<DiffObject
		{levelNames}
		value1={isObject(value1) ? value1 : {}}
		value2={isObject(value2) ? value2 : {}}
		level={level + 1}
	/>
{:else}
	<span class="rounded bg-warning-200 p-1 text-warning-800">
		Unsupported type change from {typeof value1} to {typeof value2}
	</span>
{/if}
