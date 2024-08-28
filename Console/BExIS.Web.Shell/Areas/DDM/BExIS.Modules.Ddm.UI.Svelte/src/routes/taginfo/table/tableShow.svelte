<script lang="ts">
	import { SlideToggle } from '@skeletonlabs/skeleton';
 import {tagInfoModelStore} from '../stores';
	import type { TagInfoEditModel } from '../types';
	import { get } from 'svelte/store';

	export let value: boolean;
	export let row: any;

	let currentRow:TagInfoEditModel;

 $:currentRow	= get(tagInfoModelStore).find(x=>x.versionId==row.original.versionId);


</script>


<div class="flex h-full items-center justify-center">
 <div>	
		{#if currentRow && currentRow.tagId>0 }
			<SlideToggle name="{""+currentRow.versionId}" class="" bind:checked={currentRow.show} size="sm" />
		{/if}
	</div>
</div>

