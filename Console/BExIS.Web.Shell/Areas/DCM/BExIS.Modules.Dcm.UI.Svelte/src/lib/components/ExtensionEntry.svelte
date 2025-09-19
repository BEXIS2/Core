<script lang="ts">
 import {DropdownKVP,  MultiSelect,  type listItemType } from '@bexis2/bexis2-core-ui';
 import type { extensionType } from '../../models/EntityTemplate';
 
 import { SlideToggle } from '@skeletonlabs/skeleton';
 
 export let extensions: listItemType[] = [];
 export let referenceTypes: any[] = [];
 export let extension: extensionType;
 
 const referenceTypeList:string[] = referenceTypes.map((r) => r.key.toString());

 let description = referenceTypes.find((r) => r.key === extension.referenceType)?.fil || '';
 $:description;
 function onChangeFn() {
		console.log(extension);
	}
  
 </script>

<div class="flex gap-2 items-center">

	<div class="w-1/4">

        <DropdownKVP
		id="extension"
		bind:target={extension.templateId}
		source={extensions}
		on:change={onChangeFn}/>
	</div>

	<div class="grow">

        <MultiSelect
			id="referenceType"
			source={referenceTypeList}
			isMulti={false}
			clearable={false}
			bind:target={extension.referenceType}
			on:change={(e) => onChangeFn(e)}
		/>
	</div>
    <div>     
        <SlideToggle 
        active="bg-primary-500"
        name="unique"
        bind:checked={extension.unique}
       >Unique</SlideToggle>
    </div>
 
   
</div>
