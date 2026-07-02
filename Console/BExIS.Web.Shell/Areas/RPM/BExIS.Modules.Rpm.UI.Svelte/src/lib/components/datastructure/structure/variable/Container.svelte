<script lang="ts">
	export let name: string = '';
</script>

<div class="flex gap-3">
	{#if name == 'Title'}
		<div class="grow">
			<slot name="property" />
		</div>
		<div class="border-l-2 italic text-slate-400 pl-2 flex mr-4">
			<slot name="template" />
		</div>
	{:else if name == 'DataType'}
		<div class="grow basis-0">
			<slot name="property" />
		</div>
		<div class="grow [&:has(>div:empty)]:hidden">
			<slot name="displaypattern" class="empty:hidden"/>
		</div>
		<div class="grow basis-0">
			<slot name="unit" />
		</div>
		<div class="w-1/4">
			<slot name="description" />
		</div>
	{:else if name == 'MeaningsAndConstraints'}
		<div class="grow basis-0">
			<slot name="constraints" />
		</div>
		<div class="grow basis-0">
			<slot name="meanings" />
		</div>
		<div class="w-1/4">
			<slot name="description" />
		</div>
	{:else}
		<div class="grow">
			<slot name="property" />
		</div>
		<div class="w-1/4">
			<slot name="description" />
		</div>
	{/if}
</div>

<style>
	:global(.myitem) {
		white-space: normal !important;
	}
</style>
