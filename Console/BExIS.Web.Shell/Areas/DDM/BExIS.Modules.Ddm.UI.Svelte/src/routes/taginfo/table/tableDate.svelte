<script lang="ts">

	import type { TagInfoModel } from '../types';

	export let value: string;
	export let row: TagInfoModel;
	let emtpy:boolean = true;

 emtpy = isEmpty(value)

	function isEmpty(value: string): boolean {
		if(value === null || value === undefined || value === '' ||	value === '0001-01-01T00:00:00') {
			return true;
		}

		return false;
	}
	
</script>
{#if !emtpy}
	{new Date(value).toLocaleString("de-DE")}
{/if}

