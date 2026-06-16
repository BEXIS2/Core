<script lang="ts">
	import ChoiceOneOf from './ChoiceOneOf.svelte';
	import ChoiceAnyOf from './ChoiceAnyOf.svelte';
	import ChoiceAllOf from './ChoiceAllOf.svelte';
	import { onMount } from 'svelte';
	import { activeStore } from '$lib/components/utils/metadata/stores';
	export let choiceComponent: any;
	export let path: string;


	let type = '';
	choiceComponent.oneOf ? 'oneOf' : choiceComponent.items.anyOf ? 'anyOf' : choiceComponent.items.allOf ? 'allOf' : null;
	console.log("🚀 ~ choiceWarpper:", choiceComponent, choiceComponent.oneOf)

	onMount(() => {
		
		if(choiceComponent.oneOf)
		{
			type = 'oneOf';
		}
		else if(choiceComponent.items.anyOf)
		{
			type = 'anyOf';
		}
		else if(choiceComponent.items.allOf)
		{
			type = 'allOf';
		}

		
	});


</script>

	{#if type }
		{#if type == 'oneOf'}
			<ChoiceOneOf	choiceComponent={choiceComponent} {path} />
		{:else	if type == 'anyOf'}
			<ChoiceAnyOf	choiceComponent={choiceComponent} {path} />
		{:else if type == 'allOf'}
			<ChoiceAllOf	choiceComponent={choiceComponent} {path} />
		{/if}
	{/if}
