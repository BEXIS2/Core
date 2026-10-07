<script lang="ts">
	import { updateAttribute } from "../utils/metadata/metadataComponentUtils";
	import { convertDisplayName } from "../utils/metadata/metadataShared";
	

 export let value;
 export let type;
 export let path;
 export let key;


	// if(value=='' && (type=='number' || type=='integer'))
	// {
	// 		value = 0
	// 		updateAttribute(path, key, value);
	// }

 function onChangeFn( e: any) {
		updateAttribute(path, key, value);
		//console.log("attributes - update - metadata",path, key, value, $metadataStore)
	}

</script>

<div class="flex items-center gap-2">
					<span class="text-xs text-surface-800 dark:text-surface-300 w-20 shrink-0 font-medium">{convertDisplayName(key.replace('@', ''), false)}</span>
				{#if type=='boolean'}
					<input
						type="checkbox" 
						class="checkbox"
						bind:checked={value}
						on:change={(e)=> onChangeFn(e)}
					/>
				{:else if type=='number' || type=='integer'}
					<input
						type="number"
						class="input variant-form-material text-xs py-1 "
						bind:value={value}
						on:change={(e)=> onChangeFn(e)}
					/>
     {:else if type =='date'}
					<input
						type="date"
						class="date variant-form-material text-xs py-1 flex-1"
						bind:value={value}
						on:input={(e)=> onChangeFn(e)}
					/>
     {:else if type =='time'}
					<input
						type="time"
						class="time variant-form-material text-xs py-1 flex-1"
						bind:value={value}
						on:input={(e)=> onChangeFn(e)}
					/>
     {:else}
					<input
						type="text"
						class="input variant-form-material dark:bg-zinc-700 bg-zinc-50 placeholder:text-gray-400"
						bind:value={value}
						on:input={(e)=> onChangeFn(e)}
					/>
				{/if}

  </div>