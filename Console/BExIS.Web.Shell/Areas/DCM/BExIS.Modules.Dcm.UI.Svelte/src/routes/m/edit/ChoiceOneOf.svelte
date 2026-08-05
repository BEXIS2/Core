<script lang="ts">
	import { RadioGroup, RadioItem } from '@skeletonlabs/skeleton';
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponent.svelte';
	import { onMount } from 'svelte';
	import { activateShow, getNodeByPath, setActive, ValidationStoreSetSimpleTypeValid } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { activeStore, hideStore, validationStore } from '$lib/components/utils/metadata/stores';
	import { isActive} from '$lib/components/utils/metadata/metadataComponentUtils';


	import { slide } from 'svelte/transition';
	import Header from './MetadataComponentHeader.svelte';

	export let choiceComponent: any;
	export let path: string;

	let target = "";
	let choices: {key:string, value:string, display:string}[] = getChoices(choiceComponent);
	let targetKey = '';
	let selectedChoice: any = null;
	let radioName = '';
	let previousTarget = '';
	let initializedTarget = false;

	onMount(() => {
		// If metadata already contains one branch, use it as initial target.
		const existingChoice = choices.find((item) => getNodeByPath(path + '.' + item.key) !== undefined);
		if (existingChoice) {
			target = existingChoice.key;
			targetKey = existingChoice.key;
			selectedChoice = choiceComponent?.properties?.[existingChoice.key] ?? null;
			previousTarget = existingChoice.key;
			initializedTarget = true;
		}
	});


	$:{
		radioName = 'oneof-' + normalizeKey(path || 'choice');
		if (target) {
			targetKey = resolveTargetKey(target);
			selectedChoice = targetKey && choiceComponent?.properties ? choiceComponent.properties[targetKey] : null;
			if (targetKey) {
				const selectedPath = path + '.' + targetKey;
				setActive(selectedPath);
				activateShow(selectedPath);

				if (!initializedTarget) {
					initializedTarget = true;
					previousTarget = targetKey;
				} else if (targetKey !== previousTarget) {
					cleanupBranch(previousTarget);
					previousTarget = targetKey;
				}
			}
		}
	}
	
	function getChoices(cComponent: any): {key:string, value:string, display:string}[] {
		let c: {key:string, value:string, display:string}[] = [];

		if (cComponent != undefined || cComponent != null)
		{
			let items: any[] = [];
			if (cComponent.oneOf !=null && cComponent.oneOf != undefined && cComponent.oneOf.length > 0) {
				items = cComponent.oneOf;
			}

			items.forEach((e) => {
				for (let key in e.properties)
				{

							if(isActive(path+"."+key,false)){ 
								target = key;
							}

							let item = e.properties[key];
							const refTail = item['$ref'].split('/')[item['$ref'].split('/').length - 1];

							c.push({
								key,
								value: key,
								display: refTail
							});


				}
			});
		}
		return c;
	}	

	function resolveTargetKey(rawTarget: string): string {
		if (!rawTarget) {
			return '';
		}

		const properties = choiceComponent?.properties ?? {};
		const propertyKeys = Object.keys(properties);

		if (properties[rawTarget]) {
			return rawTarget;
		}

		const normalizedRawTarget = normalizeKey(rawTarget);
		const normalizedPropertyKey = propertyKeys.find((key) => normalizeKey(key) === normalizedRawTarget);
		if (normalizedPropertyKey) {
			return normalizedPropertyKey;
		}

		const byDisplay = choices.find((item) => normalizeKey(item.display) === normalizedRawTarget);
		if (byDisplay && properties[byDisplay.key]) {
			return byDisplay.key;
		}

		const byChoiceKey = choices.find((item) => normalizeKey(item.key) === normalizedRawTarget);
		if (byChoiceKey && properties[byChoiceKey.key]) {
			return byChoiceKey.key;
		}

		return '';
	}

	function normalizeKey(value: string): string {
		return String(value ?? '')
			.toLowerCase()
			.replace(/[^a-z0-9]/g, '');
	}

	function clearValidationErrorsForPrefix(prefix: string) {
		

  // console.log('active',active,path, $activeStore);

		// get all prefied paths items from validation store and remove them
		validationStore.update(store => {
			if (!store) {
				return store;
			}

			return {
				...store,
				simpleTypeValidationItems: store.simpleTypeValidationItems.filter(item => !item.path.startsWith(prefix)),
				complexTypeValidationItems: store.complexTypeValidationItems.filter(item => !item.path.startsWith(prefix))
			};
		});
		
		
		
	}

	function cleanupBranch(branchKey: string) {
		if (!branchKey) {
			return;
		}

		const branchPath = path + '.' + branchKey;
		clearValidationErrorsForPrefix(branchPath);
	}

	function changeFn() {
		// Selection changes are handled by the reactive target block above.
	}

</script>

<div class="grid grid-cols-1 gap-0 m-2">
		<Header {path} />
	{#if !$hideStore.includes(path) && $activeStore.includes(path)}
	<div in:slide out:slide class="card px-5 py-4" id={path}>
		{#if choiceComponent.oneOf}
			<RadioGroup on:change={changeFn}>
			{#each choices as item}
				<RadioItem bind:group={target} name={radioName} title={item.display} label={item.display} value={item.value}> {item.display}</RadioItem>
			{/each}
			</RadioGroup>
		{/if}

		{#if targetKey && targetKey.length > 0 && selectedChoice} 
			{#if choiceComponent.oneOf}
				{#if selectedChoice.type === 'object' && selectedChoice.properties && !selectedChoice.properties['#text']}
	
				<div class="grid grid-cols-1 gap-0 m-2">
					<Header path = {path + '.' + targetKey} />
					
					{#if !$hideStore.includes(path + '.' + targetKey) && $activeStore.includes(path)}
					<div in:slide out:slide class="card px-5 py-4" id={path + '.' + targetKey}>
					{#key targetKey}
					<ComplexComponent
						complexComponent={selectedChoice}
						path={path + '.' + targetKey}
						required={true}
					/>
					{/key}
					</div> 
					{/if}
				</div>
				{:else if selectedChoice.type === 'object' && selectedChoice.properties && selectedChoice.properties['#text']}
					<div class="px-5 py-2">
						{#key targetKey}
						<SimpleComponent
							simpleComponent={selectedChoice.properties['#text']}
							path={path + '.' + targetKey}
							required={true}
							value={null}
							label={targetKey}
						/>
						{/key}
					</div>
				{/if}
			{/if}
{/if}
</div>
{/if}
</div>

