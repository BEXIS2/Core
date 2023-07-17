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
let model: StructureSuggestionModel;
$:model;
let list:string[] = []; 
$:list, update(readableFiles);

let loading = false;

// onMount(async () => {

// });

async function selectFile(e)
{
	console.log("file reader select file", e.detail.value);
	
	if(e.detail.value) {
    model = await load(id, e.detail.value, 0);
  }
}

function update(files)
{
	 loading=true;
		list = files.map(f=>f.name);
		loading=false;
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
		bind:target={target}
		source={list}
		on:change={selectFile}
		isMulti={false}
		complexTarget={true}
		{loading}
		placeholder="please select a file to suggest the reader informations"
	/>

{#if model}
 <FileReaderSelectionModal {model} />
{/if}
{/if}

</div>