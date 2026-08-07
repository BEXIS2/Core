<script lang="ts">

 import SimpleComponent from "$lib/components/metadata/simpleComponent.svelte";
import { getFullConfig, getIsRequiredBySchemaAndPath, getLabelByPath, getTargetVariablesWithValues, getValueByPath, resolveNode } from "$lib/components/utils/metadata/metadataComponentUtils";

 export let anchor: string;
	export let path: string = '';

	let componentName: string = 'horizontalAlignment_v1.0.0';

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
   label:string}[] = [];


  // field left
  const field_left = targetVars?.find((v) => v.target_variable === 'Field_left');
  console.log("🚀 ~ field_left:", field_left)
  if(field_left && field_left.value){
    console.log("🚀 ~ field_left.value:", field_left.value)
   const p = field_left.value;
   simpleComponents.push({
   path: p, 
   component: resolveNode(p), 
   value: getValueByPath(p),
   required:getIsRequiredBySchemaAndPath(p),
   label:getLabelByPath(p)
   });
  }

  // field middle
  const field_middle = targetVars?.find((v) => v.target_variable === 'Field_mid');
  if(field_middle && field_middle.value){
    console.log("🚀 ~ field_middle.value:", field_middle.value)
   const p = field_middle.value;
   simpleComponents.push({
   path: p, 
   component: resolveNode(p), 
   value: getValueByPath(p),
   required:getIsRequiredBySchemaAndPath(p),
   label:getLabelByPath(p)
   });
  }

  // field right
  const field_right = targetVars?.find((v) => v.target_variable === 'Field_right');
  if(field_right && field_right.value){
    console.log("🚀 ~ field_right.value:", field_right.value)
   const p = field_right.value;
   simpleComponents.push({
   path: p, 
   component: resolveNode(p), 
   value: getValueByPath(p),
   required:getIsRequiredBySchemaAndPath(p),
   label:getLabelByPath(p)
   });
  }
  console.log("🚀 ~ simpleComponents:", simpleComponents)

</script>

<div id="horizontal-alignment" class="flex flex-row justify-between w-full">
  {#each simpleComponents as simpleComponent, index (simpleComponent.path)}
    <div class="flex-1">
      {#if simpleComponent}
			    <SimpleComponent simpleComponent = {simpleComponent.component.node}  {...simpleComponent} on:reload />
      {/if}
    </div>
  {/each}
</div>


