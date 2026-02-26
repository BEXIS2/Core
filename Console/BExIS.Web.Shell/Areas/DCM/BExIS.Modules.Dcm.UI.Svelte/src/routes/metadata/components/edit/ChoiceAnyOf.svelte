<script lang="ts">
	import { CheckboxKVPList } from '@bexis2/bexis2-core-ui';
import { removeFromMetadataStore, toggleShow} from '../../../../lib/components/utils/metadata/metadataComponentUtils';
	import { hideStore } from '$lib/components/utils/metadata/stores';
 import Header from './MetadataComponentHeader.svelte';
	import SimpleComponent from './simpleComponent.svelte';
 import ComplexComponent from './complexComponentWrapper.svelte';
	import { slide } from 'svelte/transition';
	import Fa from 'svelte-fa';
	import { faChevronDown, faChevronUp } from '@fortawesome/free-solid-svg-icons';

export let choiceComponent: any;
export let path: string;

let choices: {key:string, value:string}[] = getChoices(choiceComponent);
let target;

$:{
		console.log("target", target);
		//changeFn(target);
	}
	

function getChoices(cComponent: any): {key:string, value:string}[] {
	console.log("🚀 ~ anyoF getChoices ~ cComponent:", cComponent)
	
		let c: {key:string, value:string}[] = [];

		if (cComponent != undefined || cComponent != null )
		{
			let items: any[] = [];
   let type = cComponent.items;

   if (type.anyOf !=null && type.anyOf != undefined && type.anyOf.length > 0) {
				items = type.anyOf;
			}

			items.forEach((e) => {

    for (let key in e.properties)
    {
      let item = e.properties[key];

      c.push({
       key: item['$ref'].split('/')[item['$ref'].split('/').length - 1],
       value: item['$ref'].split('/')[item['$ref'].split('/').length - 1]
      });
    }
  });
		}
		return c;
	}	

</script>


<div class="grid grid-cols-1 gap-0">
 <Header {path} />

  {#if !$hideStore.includes(path)}
    <div in:slide out:slide class="card px-5 py-4" id={path}>
     {#if choiceComponent.items && choiceComponent.items.anyOf}
      <CheckboxKVPList
        title=""
        id={path}
        description="Select one or more options"
        key="id"
        source={choices}
        bind:target
        feedback={[]}
       />

     {/if}
    </div>
  {/if}
</div>

{#if target && target.length > 0}

		{#each target as item}

   {#if choiceComponent.items.properties[item].type === 'object' && choiceComponent.items.properties[item].properties && !choiceComponent.items.properties[item].properties['#text']}
				<div class="grid grid-cols-1 gap-0 m-2">
					<div class="card bg-primary-300 dark:bg-primary-800 px-5 py-2 grid grid-cols-2">
						<div class="text-left w-4/5">						
							<h3 class="h3">{item}</h3>
						</div>
						<div class="text-right">
							{#if !$hideStore.includes(path + '.' + item)}
								<button
									class="h-9 w-10 text-right"
									title="Open or close {item}"
									on:click={() => toggleShow(path + '.' + item)}><Fa icon={faChevronUp} /></button
								>
							{:else}
								<button
									class="h-9 w-10 text-right"
									title="Open or close {item}"
									on:click={() => toggleShow(path + '.' + item)}><Fa icon={faChevronDown} /></button
								>
							{/if}
						</div>
					</div>

					{#if !$hideStore.includes(path + '.' + item)}
					<div in:slide out:slide class="card px-5 py-4" id={path + '.' + item}>
					<ComplexComponent
						complexComponent={choiceComponent.items.properties[item]}
						path={path + '.' + item}
						required={choiceComponent.required && choiceComponent.required.includes(item)}
					/>
					</div>
 
					{/if}
    </div>
	
				{:else if choiceComponent.items.properties[item].type === 'object' && choiceComponent.items.properties[item].properties['#text']}
					<div class="px-5 py-2">
						<SimpleComponent
							simpleComponent={choiceComponent.items.properties[item].properties['#text']}
							path={path + '.' + item}
							required={choiceComponent.items.required && choiceComponent.items.required.includes(item)}
							value={null}
							label={item}
						/>
					</div>
			
     {/if}

			{/each}
  {/if}