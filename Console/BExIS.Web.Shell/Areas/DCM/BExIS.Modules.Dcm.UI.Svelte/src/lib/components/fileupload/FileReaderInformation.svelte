<script lang="ts">
import {MultiSelect} from '@bexis2/bexis2-core-ui';

import { onMount } from 'svelte';

import type { StructureSuggestionModel } from '$models/StructureSuggestion';

import type { asciiFileReaderInfoType, fileInfoType } from "@bexis2/bexis2-core-ui";

import {load} from '$services/StructureSuggestionCaller';
import FileReaderSelectionModal from './FileReaderSelectionModal.svelte';
import FileReader from './FileReader.svelte';


import Fa from 'svelte-fa'
import { faCheck } from '@fortawesome/free-solid-svg-icons'


export let id;
export let readableFiles:fileInfoType[] = []; 
export let asciiFileReaderInfo:asciiFileReaderInfoType;

let target;
	$: target;

let model: StructureSuggestionModel;
$:model;
let list:string[] = []; 


onMount(async () => {

list = readableFiles.map(m=>m.name);

 console.log("SHOW HEADER",readableFiles)
});

async function selectFile(e)
{
  if(target) {
    model = await load(id, target, 0);
  }
}

</script>

<div class="card p-5">
	<div class="flex gap-1">
		<h4 class="h4">Filereader Informations
		</h4>
			{#if asciiFileReaderInfo} 
			<span class="badge-icon variant-filled-surface text-success-500"><Fa icon={faCheck}></Fa></span>
			{/if}
	
	</div>

	<FileReader {...asciiFileReaderInfo} />

{#if !asciiFileReaderInfo}

<MultiSelect
		id="fileselection"
		title="Select a File"
		bind:target
		source={list}
		on:change={selectFile}
		isMulti={false}
		placeholder="please select a file to suggest the reader informations"
	/>

{#if model}
 <FileReaderSelectionModal {model} />
{/if}
{/if}

</div>