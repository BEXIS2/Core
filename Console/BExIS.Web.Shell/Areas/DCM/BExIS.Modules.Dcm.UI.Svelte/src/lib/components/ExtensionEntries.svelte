<script lang="ts">
import {type listItemType } from '@bexis2/bexis2-core-ui';
import { extensionType } from '../../models/EntityTemplate';


import ExtensionEntry from './ExtensionEntry.svelte';
	import Fa from 'svelte-fa';
	import { faAdd, faXmark } from '@fortawesome/free-solid-svg-icons';

export let extensions: listItemType[] = [];
export let referenceTypes: any[] = [];
export let selectedExtensions: extensionType[] = [];

function addFn() {

	console.log('add',	selectedExtensions, extensions, referenceTypes);
	console.log('refs',referenceTypes);
	selectedExtensions = [...selectedExtensions, new extensionType()];
}

function removeFn(i:number) {
	selectedExtensions	= selectedExtensions.filter((e) => e != selectedExtensions[i]); 
}

</script>
{#if selectedExtensions && selectedExtensions.length > 0}
<div class="card p-5 space-y-3">

		<div class="flex">
			<!--Header-->
			<div class="w-12" />
			<div class="w-1/4 h3">Extension Template</div>
			<div class="grow h3">Reference Type</div>
		</div>
		<hr />
  {#each selectedExtensions as extension, i}
		<div class="flex items-center">
			<div class=" w-12 inline-block align-bottom">
				<button type="button" class="chip variant-filled-error" on:click={() => removeFn(i)}
					><Fa icon={faXmark} /></button
				>
			</div>
			<div class="grow">
				<ExtensionEntry bind:extensions bind:referenceTypes bind:extension />
			</div>
		</div>
 {/each} 


</div>
{/if}
<div class="inline-block align-bottom py-2">
	<button type="button" class="chip variant-filled-primary" on:click={addFn}
		><Fa icon={faAdd} /></button>
</div>
