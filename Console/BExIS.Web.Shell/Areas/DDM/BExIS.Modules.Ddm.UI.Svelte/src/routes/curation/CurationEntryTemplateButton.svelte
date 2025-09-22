<script lang="ts">
	import Fa from 'svelte-fa';
	import { faCirclePlus } from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from './stores';
	import { parseTemplateLink } from './CurationEntryTemplate';
	import { get } from 'svelte/store';
	import { DefaultCurationEntryCreationModel } from './types';

	export let markdown: string;

	const { curation } = curationStore;

	const createEntryFromTemplate = () => {
		if (!markdown) return;
		const template = parseTemplateLink(markdown);
		console.log('Creating entry from template', template);
		const type = template.type ?? DefaultCurationEntryCreationModel.type;
		const highestPosition = get(curation)?.highestPositionPerType?.[type];
		const position =
			template.position === 'top' ? 1 : highestPosition !== undefined ? highestPosition + 1 : 1;
		const entryModel = {
			...template,
			position
		};
		curationStore.addEmptyEntry(entryModel, false, template.createAsDraft ?? false, true);
	};
</script>

<button
	class="inline-block rounded px-1 text-sm text-surface-400 hover:text-success-500 active:text-success-700"
	title="Create curation entry from task"
	on:click|preventDefault={createEntryFromTemplate}
>
	<Fa icon={faCirclePlus} class="inline-block" />
	Create
</button>
