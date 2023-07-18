<script lang="ts">
import {MultiSelect} from '@bexis2/bexis2-core-ui';

import { onMount } from 'svelte';

import type { StructureSuggestionModel } from '$models/StructureSuggestion';

import type { asciiFileReaderInfoType, fileInfoType } from "@bexis2/bexis2-core-ui";

import {load} from '$services/StructureSuggestionCaller';
import FileReaderSelectionModal from './FileReaderSelectionModal.svelte';
import FileReader from './FileReader.svelte';
import { Accordion, AccordionItem } from '@skeletonlabs/skeleton';

import Fa from 'svelte-fa'
import { faCheck,faXmark } from '@fortawesome/free-solid-svg-icons'

export let id;
export let readableFiles:fileInfoType[] = []; 
export let asciiFileReaderInfo:asciiFileReaderInfoType;

let target;
$:target;
let model: StructureSuggestionModel | null;
$:model;
let list:string[] = []; 
$:list, update(readableFiles);

let loading = false;
let open = false;

let style = asciiFileReaderInfo?"success":"warning";

async function selectFile(e)
{
	console.log("file reader select file", e.detail.value);
	
	if(e.detail.value) {
				open = true;
    model = await load(id, e.detail.value, 0);
				target = null;
  }

}

function update(files)
{
	 loading=true;
		list = files.map(f=>f.name);
		target = null;
		loading=false;
}

// after closing the selection window reset values
function close()
{
		model = null;
}

</script>

<div class="card shadow-sm border-{style}-600 border-solid border">

<Accordion>
	<AccordionItem >
		<svelte:fragment slot="summary">
			<span class="variant-filled-surface text-{style}-500"><Fa icon={faCheck}></Fa></span>
		</svelte:fragment>
		<svelte:fragment slot="lead">File Reader</svelte:fragment>
		<svelte:fragment slot="content">
				<FileReader {...asciiFileReaderInfo} />


				<MultiSelect
						id="fileselection"
						title=""
						bind:target={target}
						source={list}
						on:change={selectFile}
						isMulti={false}
						complexTarget={true}
						{loading}
						placeholder="please select a file to set/update the reader information"
					/>
		
				{#if model}
					<FileReaderSelectionModal {model} on:close={close}/>
				{/if}
		</svelte:fragment>
	</AccordionItem>
</Accordion>
</div>
