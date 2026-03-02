<script lang="ts">
	import { onMount } from 'svelte';
	import {
		CheckboxKVPList,
	} from '@bexis2/bexis2-core-ui';
	import { RadioGroup, RadioItem } from '@skeletonlabs/skeleton';
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponent.svelte';
	import { removeFromMetadataStore, toggleShow, updateMetadataStore } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { hideStore } from '$lib/components/utils/metadata/stores';
	import { faChevronUp, faChevronDown } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { slide } from 'svelte/transition';
	import Header from './MetadataComponentHeader.svelte';

	export let choiceComponent: any;
	export let path: string;


	let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
	let choices: {key:string, value:string}[] = getChoices(choiceComponent);
	let target;

	$:{
		console.log("target", target);
		changeFn(target);
	}
	
	function getChoices(cComponent: any): {key:string, value:string}[] {
		console.log("🚀 ~ getChoices ~ cComponent:", cComponent)
		let c: {key:string, value:string}[] = [];

		if (cComponent != undefined || cComponent != null)
		{
			let items: any[] = [];
   if (cComponent.oneOf !=null && cComponent.oneOf != undefined && cComponent.oneOf.length > 0) {
				items = cComponent.oneOf;
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

	function changeFn(t) {
		console.log("changeFn",t, target);
		if (choiceComponent.oneOf != null && choiceComponent.oneOf != undefined && choiceComponent.oneOf.length > 0) {
			removeFromMetadataStore(path);
		}
	}

</script>

<div class="grid grid-cols-1 gap-0 m-2">
		<Header {path} />
	{#if !$hideStore.includes(path)}
	<div in:slide out:slide class="card px-5 py-4" id={path}>
		{#if choiceComponent.oneOf}
			<RadioGroup bind:value={target} on:change={changeFn}>
			{#each choices as item}
				<RadioItem bind:group={target} name="justify" title={item.key} label={item.key} value={item.value}> {item.key}</RadioItem>
			{/each}
			</RadioGroup>
		{/if}

		{#if target && target.length > 0} 
			{#if choiceComponent.oneOf}
				{#if choiceComponent.properties[target].type === 'object' && choiceComponent.properties[target].properties && !choiceComponent.properties[target].properties['#text']}
	
				<div class="grid grid-cols-1 gap-0 m-2">
					<Header path = {path + '.' + target} />
					
					{#if !$hideStore.includes(path + '.' + target)}
					<div in:slide out:slide class="card px-5 py-4" id={path + '.' + target}>
					{#key target}
					<ComplexComponent
						complexComponent={choiceComponent.properties[target]}
						path={path + '.' + target}
						required={choiceComponent.required && choiceComponent.required.includes(target)}
					/>
					{/key}
					</div> 
					{/if}
				</div>
				{:else if choiceComponent.properties[path + '.' + target].type === 'object' && choiceComponent.properties[path + '.' + target].properties['#text']}
					<div class="px-5 py-2">
						<SimpleComponent
							simpleComponent={choiceComponent.properties[target].properties['#text']}
							path={path + '.' + target}
							required={choiceComponent.required && choiceComponent.required.includes(target)}
							value={null}
							label={target}
						/>
					</div>
				{/if}
			{/if}
{/if}
</div>
{/if}
</div>



