<script lang="ts">
	import { MappingComponentConfig } from "$lib/components/utils/metadata/models";
	import { MultiSelect } from "@bexis2/bexis2-core-ui";
	import { createEventDispatcher, onMount } from 'svelte';
 import suite from '$lib/components/utils/metadata/simpleComponentSuite';

	import { getByPath, getParentPath, getPartyIdByPath, removeJsonPathIndices, updateMetadataStore, ValidationStoreSetSimpleTypeValid } from "$lib/components/utils/metadata/metadataComponentUtils";

	import { getMappingComponentConfig } from "$lib/components/utils/metadata/mappingHelper";
	import { GetPartyValue } from "../services/apiCalls";
	import { systemMappingsStore } from "$lib/components/utils/metadata/stores";
 
const dispatch = createEventDispatcher();

 export let path: string;
 export let value:any;
	export let partyId:number;
 export let label: string;
 export let required: boolean = false;
 export let isMulti: boolean = false;
 export let mappingComponentConfig: MappingComponentConfig;
 export let description: string = '';




 let partyMappingObject: any = null;
 let pathWithoutIndices: string = '';
 let selectorValue: any = null;
 let list:any

 // load form result object
	let res = suite.get();

	//$: updateMetadataStore(path, value, isMulti);

 onMount(() => {
 
   if(!mappingComponentConfig){ 
    mappingComponentConfig=getMappingComponentConfig(path, value);
   }

		

   partyMappingObject = mappingComponentConfig?.partyMappingObject;
   pathWithoutIndices = removeJsonPathIndices(path);
   list = mappingComponentConfig?.partyMappingObject?.list ?? [];

			if(value)
			{
				 if(mappingComponentConfig.partyMappingObject.complexity){

						// get party id from parent
						console.log("🚀 ~ onMount ~ path:", path)
						const parentPath = getParentPath(path);
						partyId = getPartyIdByPath(parentPath);
  
				 }
				 else {
						// get party id from this
						partyId = getPartyIdByPath(path);
					}
					selectorValue = list.find((item: any) => item.partyId == partyId);

			}
			
 
   // initial check
			setTimeout(async () => {
				if(value == undefined || value == null || value == '') {
					//res = suite(value, '');
				}
				else {
					res = suite(value, path);
				}
			}, 10);
 })


	//handle mapping change of party mapping with selector
	// we need to update the value with the new selected party and also trigger the validation for this field because maybe there are some validation rules on the party id
	async function onUpdateParty(e: any){
			
				// console.log("xyz Update Party",value, e.detail);
				const partyid = e.detail.partyId;
				const newValue	= e.detail.value;

    // selectorValue.value = newValue;
    // selectorValue.partyId = partyid;
			// add some delay so the entityTemplate is updated
			// otherwise the values are old
			setTimeout(async () => {
				 // update selected value
						// if mapping is simple, set party id 
						if(!partyMappingObject.complexity){
								//value['@partyid'] = e.detail.selectedItem.partyId;
								// console.log("🚀 ~ onUpdateParty ~ simple mapping, add party id to value:", e.detail)
								updateMetadataStore(path, newValue, isMulti, undefined, partyid);

						}
						else	if(partyMappingObject.complexity){
							
							// update new value
							updateMetadataStore(path, newValue, isMulti, undefined, undefined);

							const parentPath = getParentPath(path);
							const parentPathWithoutIndices = removeJsonPathIndices(parentPath);
							
						// if mapping is complex
						// get all partymappings where parent path is the same as the changed one
							$systemMappingsStore.partyMappings.filter((mapping: any) => mapping.parentPath == parentPathWithoutIndices && mapping.path !== pathWithoutIndices).forEach(async (mapping: any) => {
								// updateMetadataStore(mapping.path, value,	isMulti, undefined, e.detail.partyId);
								const childvalue = await GetPartyValue(partyid, mapping.linkElementId);
							

								const childPathWithIndex = parentPath+"."+mapping.path.split('.').slice(-1)[0];

								// update child value with new party value
								updateMetadataStore(childPathWithIndex, childvalue, isMulti, undefined, undefined);

								// update parent with pary	id if not already set
								console.log("🚀 ~ onUpdateParty ~ parentPath:", parentPath)
								updateMetadataStore(parentPath, null, false, undefined, partyid);

								// update because of validation
								updateValue(childvalue, childPathWithIndex)
								
        console.log("🚀 ~ onUpdateParty ~ dispatch reload for path:", selectorValue)
						}	)					
					}

						// trigger reload parent	component to update all child components with new values
						dispatch("reload");

				}, 10)
	}
 

function updateValue(value: any, _path:string){
		
    
    console.log("🚀 ~ updateValue ~ value:", value)
		
  // check changed field
			res = suite(value, _path);
			//console.log("🚀 ~ onChangeHandler ~ res:", res)
			let errorMessage = '';
			if(res.hasErrors(_path)){
					errorMessage = res.getErrors(_path).join('.  ');
					//console.log("🚀 ~ onChangeHandler ~ errorMessage:", errorMessage)
			}
			// update validationstore
			ValidationStoreSetSimpleTypeValid(_path, res.isValid(_path), errorMessage);

	}

</script>

<MultiSelect
						id="{path}"
						title="{label}"
						required={required}
						source={list}
						complexSource={true}
						complexTarget={true}
						itemId="partyId"
						itemLabel="value"
						bind:target={selectorValue}
						isMulti={false}
						clearable={required	? false : true} 
						on:change={onUpdateParty}
						invalid={res.hasErrors(path)}
						feedback={res.getErrors(path)}	 
						description={description}
					/>
