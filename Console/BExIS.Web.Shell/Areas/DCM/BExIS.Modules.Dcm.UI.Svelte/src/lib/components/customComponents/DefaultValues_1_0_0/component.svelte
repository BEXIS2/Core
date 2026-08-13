<script lang="ts">

 import SimpleComponent from "$lib/components/metadata/simpleComponent.svelte";
import { getFullConfig, getIsRequiredBySchemaAndPath, getLabelByPath, getTargetVariablesWithValues, getValueByPath, resolveNode } from "$lib/components/utils/metadata/metadataComponentUtils";

 export let anchor: string;
	export let path: string = '';

	let componentName: string = 'defaultValues_v1.0.0';

 // get config
	let config = getFullConfig(componentName, anchor);

 if (!config) {
		console.error('No configuration found for component:', componentName, 'with anchor:', anchor);
	}

  let targetVars = getTargetVariablesWithValues(config);
  
  let simpleComponents: {
   path: string, 
   component: any, 
   value: any, 
   required:boolean|undefined,
   label:string,
   description:string,
   disabled:string
 }[] = [];



  // field left
  const field = targetVars?.find((v) => v.target_variable === 'Field');
  let description = targetVars?.find((v) => v.target_variable === 'description')?.value ?? '';
  let disabled = targetVars?.find((v) => v.target_variable === 'disabled')?.value ?? "false";
  let defaultValue = targetVars?.find((v) => v.target_variable === 'defaultValue')?.value ?? '';
  console.log("🚀 ~ field:", field)
  if(field && field.value){
    console.log("🚀 ~ field.value:", field.value)
   const p = field.value;
   let value = getValueByPath(p);
    if (value === undefined || value === null || value === '') {
      value = defaultValue;
    }
   simpleComponents.push({
   path: p, 
   component: resolveNode(p), 
   value: value,
   required:getIsRequiredBySchemaAndPath(p),
   label:getLabelByPath(p),
   description:description,
    disabled:disabled
   });
  }

</script>

<div id="horizontal-alignment" class="flex flex-row justify-between w-full">
  {#each simpleComponents as simpleComponent, index (simpleComponent.path)}
    <div class="flex-1">
      {#if simpleComponent}
			    <SimpleComponent simpleComponent = {simpleComponent.component.node}  {...simpleComponent} />
      {/if}
    </div>
  {/each}
</div>


