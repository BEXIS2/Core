<script lang="ts">
	import Fa from 'svelte-fa';
	import { faSave } from '@fortawesome/free-solid-svg-icons';
	import {tagInfoModelStore } from '../stores';	
	import {get} from 'svelte/store';

	export let row;
	export let dispatchFn;

	let first:boolean = isFirstRow(row);

	function isFirstRow(row){
		const tagInfoModel = get(tagInfoModelStore)
		const lastRow = tagInfoModel[0];
		return lastRow.tagId === row.tagId;
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
	

		{#if first}
			<button
				class="btn btn-sm variant-filled-primary"
				on:click|preventDefault={() => eventDispatchFn("MINOR")}
				>minor</button>
				<button
				class="btn btn-sm variant-filled-primary"
				on:click|preventDefault={() => eventDispatchFn("MAJOR")}
				>major</button>
		{/if}
		


</div>