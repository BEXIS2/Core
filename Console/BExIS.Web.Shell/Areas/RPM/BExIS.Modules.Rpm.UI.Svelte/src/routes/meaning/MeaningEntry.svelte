<script lang="ts">
	import { MultiSelect, type listItemType } from '@bexis2/bexis2-core-ui';
	import type { meaningEntryType } from '$lib/components/meaning/types';

	export let entry: meaningEntryType;
	export let releationList: listItemType[] = [];
	export let othersList: listItemType[] = [];

	// unset entity mappingRelation if id -1
	if (entry.mappingRelation.id === -1) {
		// assign a typed null via cast to satisfy TypeScript
		entry.mappingRelation = null as unknown as listItemType;
	}

	// set by default invalid to false if not both mappings are set
	entry.isValid = false;
	$: if (entry.mappingRelation && entry.mappedLinks && entry.mappedLinks.length > 0) {
		entry.isValid = true;
	}

	// also invalid if both mappings are not set
	$: if (!entry.mappingRelation || !entry.mappedLinks || entry.mappedLinks.length == 0) {
		entry.isValid = false;
	}


	function onChangeFn() {
		console.log(entry);
	}
</script>

<div class="flex gap-2 items-center">
	<!--Releation-->

	<div class="w-1/4">
		<MultiSelect
			id="relation"
			placeholder="-- Please select --"
			title="Relation"
			itemId="id"
			itemLabel="text"
			itemGroup="group"
			bind:source={releationList}
			complexSource={true}
			complexTarget={true}
			isMulti={false}
			clearable={true}
			required={true}
			bind:target={entry.mappingRelation}
			on:change={(e) => onChangeFn(e)}
		/>
	</div>
	<!--others-->
	<div class="grow">
		<MultiSelect
			id="others"
			title="Entity | Vocabulary | Link (List can be extended under: External Link)"
			placeholder="-- Please select at least one --"
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
			required={true}
		/>
	</div>
</div>
