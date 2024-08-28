<script lang="ts">
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import type { TagInfoModel } from '../types';

	
	export let value: boolean;
	export let row: any;
	export let dispatchFn;

let currentRow:TagInfoModel = row.original;

</script>

<div class="flex h-full items-center justify-center">
	{#if currentRow.tagId>0 }
		<SlideToggle name="{currentRow.versionId}"  class="" bind:checked={currentRow.publish} size="sm"/>
	{/if}	

</div>
