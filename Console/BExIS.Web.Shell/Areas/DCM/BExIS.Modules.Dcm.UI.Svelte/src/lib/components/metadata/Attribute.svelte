<script lang="ts">
	import { updateAttribute } from "../utils/metadata/metadataComponentUtils";
	import { metadataStore } from "../utils/metadata/stores";


 export let value;
 export let type;
 export let path;
 export let key;

 function onChangeFn( e: any) {

		updateAttribute(path, key, value);
		console.log("attributes - update - metadata", $metadataStore)

	}

</script>

<div class="flex items-center gap-2">
					<span class="text-xs text-surface-600 dark:text-surface-300 w-20 shrink-0 font-medium">{key.replace('@', '')}</span>
    
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
						value={value}
						on:input={(e)=> onChangeFn(e)}
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
						type="string"
						class="input variant-form-material text-xs py-1 flex-1"
						bind:value={value}
						on:input={(e)=> onChangeFn(e)}
					/>
				{/if}

  </div>