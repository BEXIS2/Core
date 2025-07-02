<script lang="ts">
	import Fa from 'svelte-fa';
	import { CurationEntryType } from './types';
	import { faCirclePlus, faCopy } from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from './stores';

	// if a linkString is given, it will be a button to create the template from the string
	export let linkString: string | undefined = undefined;
	// else it will be a button to copy the given type, name, and description as link
	export let type: CurationEntryType = CurationEntryType.None;
	export let name = '';
	export let description = '';

	const { editMode } = curationStore;

	function encodeEntry(template: {
		type?: CurationEntryType;
		name?: string;
		description?: string;
	}) {
		return encodeURIComponent(JSON.stringify(template));
	}

	function decodeEntry(templateString: string) {
		return JSON.parse(decodeURIComponent(templateString));
	}

	const copyEntryTemplateLink = () => {
		let template: { type?: CurationEntryType; name?: string; description?: string } = {};
		if (type !== CurationEntryType.None) template.type = type;
		if (name.trim().length > 0) template.name = name;
		if (description.trim().length > 0) template.description = description;
		const templateString = encodeEntry(template);
		const markdownLink = `[Entry](?createEntryFromJSON=${templateString})`;
		navigator.clipboard.writeText(markdownLink);
	};

	const createEntryFromTemplate = () => {
		if (!linkString) return;
		const templateString = linkString.replace(/^\?createEntryFromJSON=/, '');
		const json = decodeEntry(templateString);
		curationStore.addEmptyEntryFromJson(json);
		editMode.set(true);
		setTimeout(
			() =>
				curationStore.jumpToEntryWhere.set(
					(entry) =>
						entry.isDraft() &&
						(json.type === undefined || entry.type === json.type) &&
						(json.name === undefined || entry.name === json.name) &&
						(json.description === undefined || entry.description === json.description)
				),
			500
		);
	};
</script>

{#if linkString}
	<button
		class="inline-block rounded px-1 text-sm text-surface-400 hover:text-success-500 active:text-success-700"
		title="Create curation entry from task"
		on:click|preventDefault={createEntryFromTemplate}
	>
		<Fa icon={faCirclePlus} class="inline-block" />
		Create
	</button>
{:else}
	<button
		type="button"
		on:click|preventDefault={copyEntryTemplateLink}
		title="Copy markdown link for template"
		class="text-nowrap rounded bg-surface-300 px-2 py-1 hover:bg-surface-500 focus-visible:bg-surface-500 active:bg-surface-600"
	>
		<Fa icon={faCopy} class="inline-block" />
	</button>
{/if}
