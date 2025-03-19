<script lang="ts">
	import { Modal, getModalStore, type ModalSettings } from '@skeletonlabs/skeleton';
	const modalStore = getModalStore();

	import { getPrefixCategories, remove } from './services';
	import type { prefixCategoryType } from '$lib/components/meaning/types';

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

	import { prefixCategoryStore } from '$lib/components/meaning/stores';

	let prefixCategories: prefixCategoryType[] = [];
	let prefixCategory: prefixCategoryType = { id: 0, name: '', description: '' };

	import TableOptions from './table/tableOptions.svelte';

	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';
	import { fade, slide } from 'svelte/transition';
	import PrefixCategoryForm from './PrefixCategory.svelte';

	let showForm = false;

	async function reload() {
		// get external links
		prefixCategories = await getPrefixCategories();
		console.log('🚀 ~ file: +page.svelte:39 ~ reload ~ prefixCategories:', prefixCategories);
		prefixCategoryStore.set(prefixCategories);

		console.log('store', $prefixCategoryStore);
	}

	const m: TableConfig<prefixCategoryType> = {
		id: 'ExternalLinks',
		data: prefixCategoryStore,
		optionsComponent: TableOptions,
		columns: {
			id: {
				fixedWidth: 100
			},
			versionNo: {
				disableFiltering: true,
				disableSorting: true,
				exclude: true
			},
			extra: {
				disableFiltering: true,
				disableSorting: true,
				exclude: true
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
		prefixCategory = { id: 0, name: '', description: '' };
	}

	function edit(type: any) {
		console.log('🚀 ~ file: +page.svelte:88 ~ edit ~ type:', type);

		if (type.action == 'edit') {
			showForm = false;
			prefixCategory = $prefixCategoryStore.find((u) => u.id === type.id)!;
			showForm = true;
		}

		if (type.action == 'delete') {
			console.log('🚀 ~ file: +page.svelte:97 ~ edit ~ type.action:', type.action);
			let pc: prefixCategoryType = $prefixCategoryStore.find((u) => u.id === type.id)!;
			const confirm: ModalSettings = {
				type: 'confirm',
				title: 'Delete Prefix Category',
				body: 'Are you sure you wish to delete Prefix Category ' + pc.name + '?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: async (r: boolean) => {
					if (r === true) {
						let success :boolean = await deleteFn(pc);
						if (success)
						{
							reload();
							if (pc.id === prefixCategory.id)
							{ 
								toggleForm();
							}
						}
					}
				}
			};
			modalStore.trigger(confirm);
		}
	}

	async function deleteFn(pc: prefixCategoryType): Promise<boolean> {
		console.log('🚀 ~ file: +page.svelte:112 ~ deleteFn ~ id:', pc.id);

		const res = await remove(pc.id);

		if (res) {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Prefix Category deleted.'
			});
			return true;
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: "Can't delete Prefix Category."
			});
			return false;
		}
	}

	function onSuccessFn(id: number) {
		const message = id > 0 ? 'Prefix Category updated.' : 'Prefix Category created.';

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
			message: "Can't save Prefix Category."
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
				{#if prefixCategory.id < 1}
					<span in:fade={{ delay: 400 }} out:fade>Create neẇ Prefix Category</span>
				{:else}
					<span in:fade={{ delay: 400 }} out:fade>{prefixCategory.name}</span>
				{/if}
			</div>
			<div class="text-right">
				{#if !showForm}
					<!-- svelte-ignore a11y-mouse-events-have-key-events -->
					<button
						transition:fade
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						title="Create neẇ Prefix Category"
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
				<PrefixCategoryForm
					{prefixCategory}
					on:cancel={() => toggleForm()}
					on:success={() => onSuccessFn(prefixCategory.id)}
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
