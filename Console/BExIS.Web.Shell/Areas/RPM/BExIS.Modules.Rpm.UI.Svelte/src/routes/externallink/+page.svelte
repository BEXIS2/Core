<script lang="ts">
	import { Modal, getModalStore, type ModalSettings } from '@skeletonlabs/skeleton';
	const modalStore = getModalStore();

	import {
		getLinks,
		remove,
		getLinkTypes,
		getPrefixCategoriesAsList,
		getPrefixListItems
	} from './services';
	import {
		externalLinkType,
		type prefixCategoryType,
		type prefixListItemType
	} from '$lib/components/meaning/types';

	import {
		Page,
		Table,
		ErrorMessage,
		helpStore,
		TablePlaceholder,
		type TableConfig,
		notificationStore,
		notificationType,
		type listItemType
	} from '@bexis2/bexis2-core-ui';

	import {
		externalLinksStore,
		externalLinkTypesStore,
		prefixCategoryStore,
		prefixesStore
	} from '$lib/components/meaning/stores';

	let externalLinks: externalLinkType[] = [];
	let externalLink: externalLinkType = new externalLinkType();

	import TableOptions from './table/tableOptions.svelte';

	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';
	import { fade, slide } from 'svelte/transition';
	import ExternalLinkForm from './ExternalLink.svelte';
	import TableUri from './table/tableUri.svelte';
	import { goTo } from '$services/BaseCaller';
	import UrlPreview from './UrlPreview.svelte';
	import type { SvelteComponent } from 'svelte';

	let showForm = false;

	async function reload() {

		// get external links
		externalLinks = await getLinks();
		externalLinksStore.set(externalLinks);
		console.log('🚀 ~ file: +page.svelte:50 ~ reload ~ externalLinks:', externalLinks);

		const externalLinkTypes = await getLinkTypes();
		externalLinkTypesStore.set(externalLinkTypes);

		const prefixCategoryAsList = await getPrefixCategoriesAsList();
		prefixCategoryStore.set(prefixCategoryAsList);

		const prefixesAsList = await getPrefixListItems();
		prefixesStore.set(prefixesAsList);
		console.log('🚀 ~ file: +page.svelte:60 ~ reload ~ prefixesAsList:', prefixesAsList);

		console.log('store', $externalLinksStore);
	}

	const m: TableConfig<externalLinkType> = {
		id: 'ExternalLinks',
		data: externalLinksStore,
		optionsComponent: TableOptions as unknown as typeof SvelteComponent,
		columns: {
			id: {
				fixedWidth: 30
			},
			uri: {
				header: 'Uri',
				disableFiltering: true,
				disableSorting: true,
				exclude: false
			},
			type: {
				instructions: {
					toStringFn: (pc: listItemType) => pc?.text,
					toSortableValueFn: (pc: listItemType) => pc?.text,
					toFilterableValueFn: (pc: listItemType) => pc?.text
				}
			},
			prefix: {
				instructions: {
					toStringFn: (pc: prefixListItemType) => (pc != null ? pc.text : ''),
					toSortableValueFn: (pc: prefixListItemType) => (pc != null ? pc.text : ''),
					toFilterableValueFn: (pc: prefixListItemType) => (pc != null ? pc.text : '')
				}
			},
			prefixCategory: {
				header: 'Prefix category',
				instructions: {
					toStringFn: (pc: prefixCategoryType) => (pc != null ? pc.name : ''),
					toSortableValueFn: (pc: prefixCategoryType) => (pc != null ? pc.name : ''),
					toFilterableValueFn: (pc: prefixCategoryType) => (pc != null ? pc.name : '')
				}
			},
			optionsColumn: {
				fixedWidth: 140
			}
		}
	};

	function toggleForm() {
		if (showForm) {
			clear();
		}
		showForm = !showForm;
	}

	function clear() {
		externalLink = new externalLinkType();
	}

	function edit(type: any) {
		if (type.action == 'edit') {
			showForm = false;
			externalLink = $externalLinksStore.find((u) => u.id === type.id)!;
			showForm = true;
			window.scrollTo({ top: 60, behavior: 'smooth' });
		}

		if (type.action == 'link') {
			let u = type.url;
			// add protocol if not exist
			if (!u.startsWith('http')) {
				u = 'https://' + u;
			}
			window.open(type.url);
		}

		if (type.action == 'delete') {
			let el: externalLinkType = $externalLinksStore.find((u) => u.id === type.id)!;
			const confirm: ModalSettings = {
				type: 'confirm',
				title: 'Delete External Link',
				body: 'Are you sure you wish to delete external link ' + el.name + '?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: async (r: boolean) => {
					if (r === true) {
						let success :boolean = await deleteFn(el);
						if (success)
						{
							reload();
							if (el.id === externalLink.id) {
								toggleForm();
							}
						}

					}
				}
			};
			modalStore.trigger(confirm);
		}
	}

	async function deleteFn(el: externalLinkType) : Promise<boolean> {
		console.log('🚀 ~ file: +page.svelte:112 ~ deleteFn ~ id:', el.id);

		const res = await remove(el.id);

		if (res) {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'External Link "'+ el.name +'" deleted.'
			});
			return true;
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: 'Can\'t delete external link "'+ el.name +'".'
			});
			return false;
		}
	}

	function onSuccessFn(id: number) {
		const message = id > 0 ? 'External link updated.' : 'External link created.';

		notificationStore.showNotification({
			notificationType: notificationType.success,
			message: message
		});

		showForm = false;
		setTimeout(async () => {
			reload();
		}, 10);
	}

	function onFailFn() {
		notificationStore.showNotification({
			notificationType: notificationType.error,
			message: "Can't save external Link."
		});
	}
</script>

<Page help={true} title="Manage External Links">
	{#await reload()}
		<div class="grid w-full grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h-9 w-96 placeholder animate-pulse" />
			<div class="flex justify-end">
				<button class="btn placeholder animate-pulse shadow-md h-9 w-16"
					><Fa icon={faPlus} /></button
				>
			</div>
		</div>
		<div class="table-container w-full">
			<TablePlaceholder cols={5} />
		</div>
	{:then}
		<!-- svelte-ignore a11y-click-events-have-key-events -->
		<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h3 h-9">
				{#if externalLink.id < 1}
					<span in:fade={{ delay: 400 }} out:fade>Create new External link</span>
				{:else}
					<span in:fade={{ delay: 400 }} out:fade>{externalLink.name}</span>
				{/if}
			</div>
			<div class="text-right">
				{#if !showForm}
					<!-- svelte-ignore a11y-mouse-events-have-key-events -->
					<button
						transition:fade
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						title="Create new External Link"
						id="create"
						on:mouseover={() => {
							helpStore.show('create');
						}}
						on:click={() => toggleForm()}><Fa icon={faPlus} /></button
					>
				{/if}
			</div>
		</div>

		{#if showForm}
			<div in:slide out:slide>
				<ExternalLinkForm
					link={externalLink}
					on:cancel={() => toggleForm()}
					on:success={() => onSuccessFn(externalLink.id)}
					on:fail={onFailFn}
				/>
			</div>
		{/if}

		<div class="table table-compact w-full">
			<Table on:action={(obj) => edit(obj.detail.type)} config={m} />
		</div>
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</Page>
<Modal />
