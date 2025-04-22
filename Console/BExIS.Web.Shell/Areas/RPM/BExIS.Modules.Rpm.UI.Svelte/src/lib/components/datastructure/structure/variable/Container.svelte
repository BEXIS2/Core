<script lang="ts">
	export let name: string = '';
</script>

<div class="flex gap-5">
	{#if name == 'Title'}
		<div class="grow">
			<slot name="property" />
		</div>
		<div class="border-l-2 border-b-2 italic text-slate-400 pl-2 pb-2 flex mr-8">
			<slot name="template" />
		</div>
	{:else if name == 'DataType'}
		<div class="grow">
			<slot name="property" />
		</div>
		<div class="grow">
			<slot name="displaypattern" />
		</div>
		<div class="grow">
			<slot name="unit" />
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
