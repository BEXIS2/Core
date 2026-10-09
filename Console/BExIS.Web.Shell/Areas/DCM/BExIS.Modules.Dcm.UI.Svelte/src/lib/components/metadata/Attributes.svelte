<script lang="ts">
	import { onMount } from "svelte";
	import { getAttributeValue, getSchemaAttributes, getSchemaAttributeTypes, updateAttribute } from "../utils/metadata/metadataComponentUtils";
	import { metadataStore } from "../utils/metadata/stores";
	import Attribute from "./Attribute.svelte";


	export let component: any;
 export let path: string;

 $: schemaAttr = getSchemaAttributes(component)
	$: schemaAttrTypes = getSchemaAttributeTypes(component);
	$: attrValues = schemaAttr.reduce((acc: Record<string, any>, attr: string) => {
		acc[attr] = getAttributeValue(path, attr);
		return acc;
	}, {});

</script>

{#if schemaAttr.length > 0}
		<div class="flex flex-col gap-1 mt-1 pl-2 mb-2 w-1/2 border-l-2 border-surface-200 dark:border-surface-700">
			{#each Object.entries(schemaAttrTypes) as [key, value]}
			 	<Attribute value= {attrValues[key]} type={value.type} {path} {key}/>
			{/each}
		</div>
	{/if}