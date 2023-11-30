<script lang="ts">
	import { Modal, getModalStore, type ModalSettings } from '@skeletonlabs/skeleton';
	const modalStore = getModalStore();
	
	import { getLinks, remove } from './services';
	import type { externalLinkType, prefixCategoryType } from '$lib/components/meaning/types';

	import {
		Page,
		Table,
		ErrorMessage,
		helpStore,
		TablePlaceholder,
		type TableConfig,
		notificationStore,
		notificationType
	} from '@bexis2/bexis2-core-ui';

	import { externalLinksStore } from '$lib/components/meaning/stores';

	let externalLinks: externalLinkType[] = [];
	let externalLink: externalLinkType = { id: 0, name: '', type: '', uri: '' };

	import TableOptions from './table/tableOptions.svelte';

	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';
	import { fade, slide } from 'svelte/transition';
	import ExternalLinkForm from './ExternalLink.svelte';
	import TableUri from './table/tableUri.svelte';

	let showForm = false;

	async function reload() {
		showForm = false;

		// get external links
		externalLinks = await getLinks();
  externalLink = { id: 0, name: '', type: '', uri: '' };
		externalLinksStore.set(externalLinks);

		console.log('store', $externalLinksStore);
	}

	const m: TableConfig<externalLinkType> = {
		id: 'ExternalLinks',
		data: externalLinksStore,
		optionsComponent: TableOptions,
		columns: {
			id: {
				fixedWidth: 100
			},
			extra: {
				disableFiltering: true,
				disableSorting: true,
				exclude: true
			},
			versionNo: {
				disableFiltering: true,
				disableSorting: true,
				exclude: true
			},
			uri: {
				header: 'Uri',
				instructions: {
					renderComponent: TableUri
				},
				disableFiltering: true,
				disableSorting:true
			},
			type: {
				disableFiltering: true,
				disableSorting: true,
				exclude: true
			},
			prefix: {
				instructions: {
					toStringFn: 
					 ((pc: externalLinkType) =>	pc?.name	),
					toSortableValueFn: 
					((pc: externalLinkType) =>	pc?.name	),
					toFilterableValueFn: 
					((pc: externalLinkType) =>	pc?.name	)
				}

			},
			prefixcategory: {
				instructions: {
					toStringFn: 
					 ((pc: prefixCategoryType) =>	pc?.name	),
					toSortableValueFn: 
					((pc: prefixCategoryType) =>	pc?.name	),
					toFilterableValueFn: 
					((pc: prefixCategoryType) =>	pc?.name	)
				}
			},
			optionsColumn: {
				fixedWidth: 100
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
		externalLink = { id: 0, name: '', type: '', uri: '' };
		showForm = false;
	}

	function edit(type: any) {
		console.log('🚀 ~ file: +page.svelte:88 ~ edit ~ type:', type);

		if (type.action == 'edit') {
			showForm = false;
			externalLink = $externalLinksStore.find((u) => u.id === type.id)!;
			showForm = true;
		}

		if (type.action == 'delete') {
			console.log('🚀 ~ file: +page.svelte:97 ~ edit ~ type.action:', type.action);
			const confirm: ModalSettings = {
				type: 'confirm',
				title: 'Delete External Link',
				body: 'Are you sure you wish to delete external link ' + externalLink.name + '?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: (r: boolean) => {
					if (r === true) {
						deleteFn(type.id);
					}
				}
			};
			modalStore.trigger(confirm);
		}
	}

	async function deleteFn(id: number) {
		console.log('🚀 ~ file: +page.svelte:112 ~ deleteFn ~ id:', id);

		const res = await remove(id);

		if (res) {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'External Link deleted.'
			});

			reload();
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: "Can't delete external link."
			});
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
					<span in:fade={{ delay: 400 }} out:fade>Create neẇ External link</span>
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
						title="Create neẇ External Link"
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
					on:cancel={() => clear()}
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
