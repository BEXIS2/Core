<script lang="ts">
	import { onMount } from 'svelte';
	import {
		CheckboxKVPList,
	} from '@bexis2/bexis2-core-ui';
	import { RadioGroup, RadioItem } from '@skeletonlabs/skeleton';
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponent.svelte';
	import { toggleShow } from '../../utils';
	import { hideStore } from '../../stores';
	import { faChevronUp, faChevronDown } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { slide } from 'svelte/transition';

	export let choiceComponent: any;
	export let path: string;

	let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
	let choices: {key:string, value:string}[] = getChoices(choiceComponent);
	let target;

	function getChoices(cComponent: any): {key:string, value:string}[] {
		let c: {key:string, value:string}[] = [];

		if (cComponent != undefined || cComponent != null)
		{
			let items: any[] = [];

			if (cComponent.anyOf !=null && cComponent.anyOf != undefined && cComponent.anyOf.length > 0) {
				items = cComponent.anyOf;
			}
			else if (cComponent.oneOf !=null && cComponent.oneOf != undefined && cComponent.oneOf.length > 0) {
				items = cComponent.oneOf;
			}
			else if (cComponent.allOf !=null && cComponent.allOf != undefined && cComponent.allOf.length > 0) {
				items = cComponent.allOf;	
			}
			
			items.forEach((item) => {
				c.push({
					key: item['$ref'].split('/')[item['$ref'].split('/').length - 1],
					value: item['$ref'].split('/')[item['$ref'].split('/').length - 1]
				});
				if (cComponent.allOf !=null && cComponent.allOf != undefined && cComponent.allOf.length > 0) target.push(item['$ref'].split('/')[item['$ref'].split('/').length - 1]);
			});
		}
		return c;
	}	
</script>

<div class="grid grid-cols-1 gap-0 m-2">
	<div class="card bg-primary-300 dark:bg-primary-800 px-5 py-2 grid grid-cols-2">
		<div class="text-left w-4/5">						
			<h3 class="h3">{label}</h3>
		</div>
		<div class="text-right">
			{#if !$hideStore.includes(path)}
				<button
					class="h-9 w-10 text-right"
					title="Open or close {label}"
					on:click={() => toggleShow(path)}><Fa icon={faChevronUp} /></button
				>
			{:else}
				<button
					class="h-9 w-10 text-right"
					title="Open or close {label}"
					on:click={() => toggleShow(path)}><Fa icon={faChevronDown} /></button
				>
			{/if}
		</div>
	</div>
	{#if !$hideStore.includes(path)}
	<div in:slide out:slide class="card px-5 py-4" id={path}>
		{#if choiceComponent.anyOf}
			<CheckboxKVPList
					title=""
					id={path}
					description="Select one or more options"
					key="id"
					source={choices}
					bind:target
				/>
		{:else if choiceComponent.oneOf}
			<RadioGroup bind:value={target}>
			{#each choices as item}
				<RadioItem bind:group={target} name="justify" title={item.key} label={item.key} value={item.value}>{item.key}</RadioItem>
			{/each}
			</RadioGroup>
			{:else if choiceComponent.allOf}
			{#each choices as item}
				<div>
					{item.key}
				</div>
			{/each}
		{/if}

		{#if target && target.length > 0}
			{#if choiceComponent.anyOf || choiceComponent.allOf}
			{#each target as item}
				{#if choiceComponent.properties[item].type === 'object' && choiceComponent.properties[item].properties && !choiceComponent.properties[item].properties['#text']}
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
						complexComponent={choiceComponent.properties[item]}
						path={path + '.' + item}
						required={choiceComponent.required && choiceComponent.required.includes(item)}
					/>
					</div>
					{/if}
				</div>
				{:else if choiceComponent.properties[item].type === 'object' && choiceComponent.properties[item].properties['#text']}
					<div class="px-5 py-2">
						<SimpleComponent
							simpleComponent={choiceComponent.properties[item].properties['#text']}
							path={path + '.' + item}
							required={choiceComponent.required && choiceComponent.required.includes(item)}
							value={null}
							label={item}
						/>
					</div>
				{/if}
			{/each}
			{:else if choiceComponent.oneOf}
				{#if choiceComponent.properties[target].type === 'object' && choiceComponent.properties[target].properties && !choiceComponent.properties[target].properties['#text']}
				<div class="grid grid-cols-1 gap-0 m-2">
					<div class="card bg-primary-300 dark:bg-primary-800 px-5 py-2 grid grid-cols-2">
						<div class="text-left w-4/5">						
							<h3 class="h3">{target}</h3>
						</div>
						<div class="text-right">
							{#if !$hideStore.includes(target)}
								<button
									class="h-9 w-10 text-right"
									title="Open or close {target}"
									on:click={() => toggleShow(target)}><Fa icon={faChevronUp} /></button
								>
							{:else}
								<button
									class="h-9 w-10 text-right"
									title="Open or close {target}"
									on:click={() => toggleShow(target)}><Fa icon={faChevronDown} /></button
								>
							{/if}
						</div>
					</div>
					{#if !$hideStore.includes(target)}
					<div in:slide out:slide class="card px-5 py-4" id={target}>
					<ComplexComponent
						complexComponent={choiceComponent.properties[target]}
						path={target}
						required={choiceComponent.required && choiceComponent.required.includes(target)}
					/>
					</div>
					{/if}
				</div>
				{:else if choiceComponent.properties[target].type === 'object' && choiceComponent.properties[target].properties['#text']}
					<div class="px-5 py-2">
						<SimpleComponent
							simpleComponent={choiceComponent.properties[target].properties['#text']}
							path={path}
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
