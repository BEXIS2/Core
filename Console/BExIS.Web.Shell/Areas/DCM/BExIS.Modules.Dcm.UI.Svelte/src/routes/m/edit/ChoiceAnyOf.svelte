<script lang="ts">
	import { CheckboxKVPList } from '@bexis2/bexis2-core-ui';
	import {  setInactive, getNodeByPath, removeFromMetadataStore } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { onMount } from 'svelte';
		import { activeStore, hideStore, metadataStore, validationStore } from '$lib/components/utils/metadata/stores';
 import Header from './MetadataComponentHeader.svelte';
	import SimpleComponent from '$lib/components/metadata/simpleComponent.svelte';
 import ComplexComponent from './complexComponentWrapper.svelte';
	import { slide } from 'svelte/transition';



export let choiceComponent: any;
export let path: string;
export let required: boolean = false;

let choices: {key:string, value:string}[] = getChoices(choiceComponent);
let target:string[]=[];
let pathaddtion = ".0."
let init = true;

$:{
		//console.log("target", target, choices, choiceComponent, $metadataStore);
		changeFn(target);
	}

 onMount( async ()=>{
		load()
	});

function load()
{
	 let parent = getNodeByPath(path);

		// go throw each choice and check if the element exist in the metadata
		// if yes add to target
		choices.forEach(option =>{
			const p = path+pathaddtion+option.key
			const c = getNodeByPath(p);
		
			if(c){
					target.push(option.key)
			}

		})

		init = false;
}

function changeFn(t){
	let parent = getNodeByPath(path);
	//console.log("🚀 ~ changeFn ~ parent:", parent)

	if(target && !init)
	{
		console.log(choices)

		choices.forEach(element => {
				if(!target.includes(element.key))
				{
					const childpath = path+pathaddtion+element.key;
					setInactive(childpath);
					removeFromValidationStore(childpath);
					removeFromMetadataStore(childpath)
					//console.log("remove"+childpath)
				}
		});


		target.forEach(tKey => {
				//console.log("🚀 ~ changeFn ~ choiceComponent:", choiceComponent)

				// const c = schemaToJson(choiceComponent?.items?.properties?.[tKey])
				// if(parent.length ==0)
				// {
				// 		parent = [{ [tKey]: c }];
				// }
				// else
				// {
				// 		parent = [{ [tKey]: c }];
				// }

				//console.log("🚀 ~ changeFn ~ parent:", parent, $metadataStore, JSON.stringify($metadataStore) )
		});
	}
}

function removeFromValidationStore(path: string) {
		validationStore.update((store) => {
			return {
				...store,
				simpleTypeValidationItems: store.simpleTypeValidationItems.filter(
					(item) => !item.path.startsWith(path)
				),
				complexTypeValidationItems: store.complexTypeValidationItems.filter(
					(item) => !item.path.startsWith(path)
				)
			};
		});
	}


// react on selection changes: create/remove metadata nodes for chosen options
// $: if (target !== undefined) {
// 	try {
// 		const newTarget: string[] = Array.isArray(target) ? [...target] : [];
// 		const added = newTarget.filter(t => !previousTarget.includes(t));
// 		const removed = previousTarget.filter(t => !newTarget.includes(t));

// 		// remove deselected nodes from metadata
// 		for (const r of removed) {
// 			removeFromMetadataStore(path + '.' + r);
// 		}

// 		// create default nodes for newly selected choices based on schema
// 		for (const a of added) {
// 			const nodeSchema = choiceComponent?.items?.properties?.[a];
// 			const defaultValue = schemaToJson(nodeSchema);
// 				const tpath = path + '.' + a;
// 			if (nodeSchema) {
				
// 				console.log("🚀 ~ tpath:", tpath)
// 				//updateMetadataStore(tpath, defaultValue, true);
// 				// setActive(tpath);
// 				// activateShow(tpath);
// 				//insertAtPath(path,'')

// 			} else {
// 				removeFromMetadataStore(path);
// 				setInactive(defaultValue);
// 			}
// 		}

// 		previousTarget = newTarget;
// 	} catch (err) {
// 		console.error('Error syncing ChoiceAnyOf target to metadataStore', err);
// 	}
// }

// initialize `target` from existing metadata (if present)
onMount(() => {

	// const existing = getAtPath(path);
	// if (existing && typeof existing === 'object') {
	// 	const keys = Object.keys(existing).filter(k => !k.startsWith('@') && k !== '#text');
	// 	if (keys.length > 0) {
	// 		target = keys;
	// 		previousTarget = [...keys];
	// 	}
	// }
});
	

function getChoices(cComponent: any): {key:string, value:string}[] {
	 //console.log("🚀 ~ anyoF getChoices ~ cComponent:", cComponent)
	
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


<div class="card grid grid-cols-1 gap-0">

 	<Header {path} {required}/>

  {#if !$hideStore.includes(path)  && $activeStore.includes(path)}
    <div in:slide out:slide class="card px-5 py-4" id={path}>
     {#if choiceComponent.items && choiceComponent.items.anyOf}
					<div class="pl-1 pb-2">
      <CheckboxKVPList
        title=""
        id={path}
        key="key"
        source={choices}
        bind:target
        feedback={[]}
								vertical={false}
       />
					</div>
					

					{#if target && target.length > 0}
					{#each choices as c}
					{#if target.includes(c.key)}
					 {@const item = c.key}
						{#if choiceComponent.items.properties[item].type === 'object' && choiceComponent.items.properties[item].properties && !choiceComponent.items.properties[item].properties['#text']}
							<div class="grid grid-cols-1 gap-0 pl-1">
	
								<Header path = {path + pathaddtion + item}  description={choiceComponent.items.properties[item].description} />

								{#if !$hideStore.includes(path + '.' + item) }
								<div in:slide out:slide class="card px-5 py-4" id={path + pathaddtion + item}>
								<ComplexComponent
									complexComponent={choiceComponent.items.properties[item]}
									path={path + pathaddtion + item}
									required={choiceComponent.required && choiceComponent.required.includes(item)}
									on:updated
								/>
								</div>

								{/if}
							</div>

							{:else if choiceComponent.items.properties[item].type === 'object' && choiceComponent.items.properties[item].properties['#text']}
								<div class="px-5 py-1">
									<SimpleComponent
										simpleComponent={choiceComponent.items.properties[item].properties['#text']}
										path={path + pathaddtion + item}
										required={choiceComponent.items.required && choiceComponent.items.required.includes(item)}
										value={null}
										label={item}
										on:updated
									/>
								</div>
						
								{/if}
							{/if}
						{/each}
					{/if}
					

     {/if}
    </div>
  {/if}
</div>



