<script lang="ts">
	import { MappingComponentConfig } from '$lib/components/utils/metadata/models';
	import { MultiSelect } from '@bexis2/bexis2-core-ui';
	import { createEventDispatcher, onMount } from 'svelte';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';

	import {
		getByPath,
		getParentPath,
		getPartyIdByPath,
		getIsRequiredBySchemaAndPath,
		removeJsonPathIndices,
		updateMetadataStore,
		ValidationStoreSetSimpleTypeValid
	} from '$lib/components/utils/metadata/metadataComponentUtils';

	import { getMappingComponentConfig } from '$lib/components/utils/metadata/mappingHelper';
	import { GetPartyValue } from '../../../services/MetadataCaller';
	import { systemMappingsStore } from '$lib/components/utils/metadata/stores';

	import Fa from 'svelte-fa';
	import { faLink } from '@fortawesome/free-solid-svg-icons';
	import { validationStore } from '$lib/components/utils/metadata/stores';

	const dispatch = createEventDispatcher();

	export let path: string;
	export let value: any;
	let partyId: number;
	export let label: string;
	export let required: boolean = false;
	export let isMulti: boolean = false;
	export let mappingComponentConfig: MappingComponentConfig;
	export let description: string = '';
	export let handleShowDescription: (event: CustomEvent<any>) => void;
	export let handleHideDescription: (event: CustomEvent<any>) => void;

	let partyMappingObject: any = null;
	let pathWithoutIndices: string = '';
	let selectorValue: any = null;
	let list: any;

	// load form result object
	let res = suite.get();

	onMount(() => {
		if (!mappingComponentConfig) {
			mappingComponentConfig = getMappingComponentConfig(path, value);
		}

		partyMappingObject = mappingComponentConfig?.partyMappingObject;
		pathWithoutIndices = removeJsonPathIndices(path);
		list = mappingComponentConfig?.partyMappingObject?.list ?? [];
		console.log('🚀 ~ list:', partyMappingObject, list);

		if (value) {
			if (mappingComponentConfig.partyMappingObject.complexity) {
				// get party id from parent
				console.log('🚀 ~ onMount ~ path:', path);
				const parentPath = getParentPath(path);
				partyId = getPartyIdByPath(parentPath);
				//alert(partyId);
			} else {
				// get party id from this
				partyId = getPartyIdByPath(path);
			}
			selectorValue = list.find((item: any) => item.partyId == partyId);
		}

		// initial check
		setTimeout(() => {
			updateValue(value, path);
		}, 10);
	});

	//handle mapping change of party mapping with selector
	// we need to update the value with the new selected party and also trigger the validation for this field because maybe there are some validation rules on the party id
	async function onUpdateParty(e: any) {
		//console.log("onUpdateParty",value, e.detail);
		const detail = e?.detail ?? {};
		const partyid = detail.partyId ?? 0;
		const newValue = detail.value ?? '';

		// selectorValue.value = newValue;
		// selectorValue.partyId = partyid;
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			// update selected value
			// if mapping is simple, set party id
			if (!partyMappingObject.complexity) {
				updateMetadataStore(path, newValue, isMulti, undefined, partyid);
				updateValue(newValue ?? '', path);
			} else {
				updateMetadataStore(path, newValue, isMulti, undefined, undefined);
				updateValue(newValue ?? '', path);

				const parentPath = getParentPath(path);
				const parentPathWithoutIndices = removeJsonPathIndices(parentPath);

				// update parent with pary	id if not already set
				updateMetadataStore(parentPath, null, false, undefined, partyid);

				// if mapping is complex
				// get all partymappings where parent path is the same as the changed one
				$systemMappingsStore.partyMappings
					.filter(
						(mapping: any) =>
							mapping.parentPath == parentPathWithoutIndices && mapping.path !== pathWithoutIndices
					)
					.forEach(async (mapping: any) => {
						// updateMetadataStore(mapping.path, value,	isMulti, undefined, e.detail.partyId);
						const childvalue = await GetPartyValue(partyid, mapping.linkElementId);

						const childPathWithIndex = parentPath + '.' + mapping.path.split('.').slice(-1)[0];

						// update child value with new party value
						updateMetadataStore(childPathWithIndex, childvalue, isMulti, undefined, undefined);

						// update because of validation
						updateValue(childvalue, childPathWithIndex);

						//console.log("🚀 ~ onUpdateParty ~ dispatch reload for path:", selectorValue)
					});
			}
		}, 100);
	}

	function updateValue(value: any, _path: string) {
		console.log('🚀 ~ updateValue ~ value:', value);

		// check changed field
		res = suite(_path);
		//console.log("🚀 ~ onChangeHandler ~ res:", res)
		let errorMessage = '';
		if (res.hasErrors(_path)) {
			errorMessage = res.getErrors(_path).join('.  ');
			//console.log("🚀 ~ onChangeHandler ~ errorMessage:", errorMessage)
		}
		// update validationstore
		ValidationStoreSetSimpleTypeValid(_path, res.isValid(_path), errorMessage);
	}

	if (getIsRequiredBySchemaAndPath(path)) {
		console.log('🚀 ~ PartySelector.svelte ~ onMount ~ path is required:', path);
		required = true;
	}

	$: validationItem = $validationStore?.simpleTypeValidationItems?.find(
		(item) => item.path === path
	);

	$: commonProps = {
		id: path,
		label,
		required,
		invalid: validationItem ? !validationItem.isValid : false,
		valid: validationItem ? validationItem.isValid : false,
		feedback: validationItem?.errorMessage ? validationItem.errorMessage.split('\n') : [],
		description: description,
		showDescription: false,
		showIcon: false,
		disabled: false
	};
	// $: console.log('🚀 ~ PartySelector.svelte ~ commonProps:', commonProps);
</script>

<div class="flex items-center gap-2">
	<div class="grow" id={path}>
		<MultiSelect
			{...commonProps}
			title={label}
			source={list}
			complexSource={true}
			complexTarget={true}
			itemId="partyId"
			itemLabel="value"
			bind:target={selectorValue}
			isMulti={false}
			clearable={true}
			searchable={false}
			on:change={onUpdateParty}
			on:clear={onUpdateParty}
			on:showDescription={handleShowDescription}
			on:hideDescription={handleHideDescription}
		/>
	</div>
	<div
		class="pt-7"
		title="This field is linked to a party. Changing the value here will update all other fields linked to the same party."
	>
		<Fa icon={faLink} class="text-gray-500" />
	</div>
</div>
