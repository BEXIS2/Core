<script lang="ts">
	export let name: string = "";
	export let py:number = 2;
</script>

<div class="grid lg:grid-cols-10 md:grid-cols-10 py-{py} gap-5 ">

	{#if name =="DataType"}
	<div class="col-span-2">
			<slot name="property" />
		</div>
		<div class="col-span-2">
			<slot name="displaypattern" />
		</div>
		<div class="col-span-3">
			<slot name="unit" />
		</div>
	{:else if name =="Title"}
		<div class="col-span-5">
			<slot name="property" />
		</div>
		<div class="col-span-3 border-l-2 border-b-2 italic text-slate-400 pl-2 pb-2 flex ">
			<slot name="template" />
	</div>
	{:else}
		<div class="col-span-7">
			<slot name="property" />
		</div>
	{/if}


	<div class="col-span-3">
		<slot name="description" />
	</div>
</div>

<style>
	:global(.myitem) {
		white-space: normal !important;
	}
	</style>