<script lang="ts">
	import Fa from 'svelte-fa';
	import { faSave } from '@fortawesome/free-solid-svg-icons';
	import {withMinorStore } from '../stores';	
	import {get} from 'svelte/store';
	import type { TagInfoEditModel } from '../types';

	export let row:TagInfoEditModel;
	export let dispatchFn;

	let noTag:boolean = hasNoTag();
 let withMinor = get(withMinorStore);

	function hasNoTag(){

		return  row.tagId == 0;
	}


	const eventDispatchFn = (type: string) => {
		return dispatchFn({ type, row });
	};

	const buttons = [
		{
			icon: faSave,
			color: 'variant-filled-primary',
			type: 'SAVE'
		},
		{
			icon: faSave,
			color: 'variant-filled-secondary',
			type: 'MINOR'
		},
		{
			icon: faSave,
			color: 'variant-filled-secondary',
			type: 'MAJOR'
		}
	];
</script>

<div class="flex gap-2 w-min p-2">



			<button
				class="btn btn-sm variant-filled-primary "
				on:click|preventDefault={() => eventDispatchFn("SAVE")}
			>
				<Fa icon={faSave} />
			</button>
	

		{#if noTag}
		 {#if withMinor}
			<button
				class="btn btn-sm variant-filled-primary"
				on:click|preventDefault={() => eventDispatchFn("MINOR")}
				>minor</button>
				{/if}
				<button
				class="btn btn-sm variant-filled-primary"
				on:click|preventDefault={() => eventDispatchFn("MAJOR")}
				>major</button>
		{/if}
		


</div>