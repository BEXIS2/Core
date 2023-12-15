<script lang="ts">
	import { MultiSelect, type listItemType} from '@bexis2/bexis2-core-ui';
	import type { externalLinkType, meaningEntryType } from '$lib/components/meaning/types';

	export let entry: meaningEntryType;
 export let releationList:listItemType[]=[];
 export let othersList:listItemType[]=[];

 function onChangeFn()
 {
   console.log(entry);
   
 }

</script>

<div class="flex gap-2 items-center">
	<!--Releation-->

 <div class="w-1/4">
  <MultiSelect 
  id="relation" 
  itemId="id"
  itemLabel="text"
  itemGroup="group"
  source={releationList} 
  complexSource={true}
		complexTarget={true}
		isMulti={false}
  clearable={true}
  bind:target={entry.mappingRelation} 
  on:change={(e) => onChangeFn(e)}
  />
 </div>
	<!--others-->
 <div class="grow">
  <MultiSelect
   id="others"
   bind:source={othersList}
   itemId="id"
   itemLabel="text"
   itemGroup="group"
   complexSource={true}
   complexTarget={true}
   isMulti={true}
   clearable={true}
   bind:target={entry.mappedLinks}
   on:change={(e) => onChangeFn(e)}
   placeholder="-- Please select --"
  />
 </div>
</div>
