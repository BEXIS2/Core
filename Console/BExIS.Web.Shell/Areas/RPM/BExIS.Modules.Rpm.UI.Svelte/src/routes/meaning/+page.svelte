<script lang="ts">
	import { Modal, getModalStore, type ModalSettings } from '@skeletonlabs/skeleton';
	import { getMeanings, getConstraints, remove, update, getLinks } from './services';
	import { MeaningModel, type externalLinkType } from '$lib/components/meaning/types';
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

	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';
	import {
		externalLinksStore,
		meaningsStore,
		constraintsStore
	} from '$lib/components/meaning/stores';
	import { fade, slide } from 'svelte/transition';
	import TableIsApproved from './table/tableIsApproved.svelte';
	import TableExnternalLink from './table/tableExnternalLink.svelte';
	import TableMeaning from './table/tableMeaning.svelte';
	import TableOptions from './table/tableOptions.svelte';
	import Meaning from './Meaning.svelte';
	import { onMount, type SvelteComponent } from 'svelte';
	import type { linkType } from '@bexis2/bexis2-core-ui';

	let isLoading = true;
	let loadingError: any = null;

	//stores
	let meanings: MeaningModel[];
	let meaning: MeaningModel = new MeaningModel(null);

	let showForm = false;

	// modal
	const modalStore = getModalStore();

	async function reload(isBackground = false) {
		// Only trigger the full page loading layout if it's not a background update
        if (!isBackground) {
            isLoading = true; 
        }

		try {
			const [meanings, constraints, externalLinks] = await Promise.all([
				getMeanings(),
				getConstraints(),
				getLinks()
			]);
			// get meanings
			// meanings = await getMeanings();
			meaningsStore.set(meanings);

			// get constraints
			// const constraints = await getConstraints();
			constraintsStore.set(constraints);

			// get external links
			// const externalLinks = await getLinks();
			externalLinksStore.set(externalLinks);
		} catch (error) {
			loadingError = error;
		} finally {
			if (!isBackground) {
                isLoading = false;
            }
		}
	}

	onMount(async () => {
		try {
			await reload(false);
		} catch (err) {
			// error handled via loadingError
		} finally {
			isLoading = false;
		}
	});

	const m: TableConfig<MeaningModel> = {
		id: 'Meaning',
		data: meaningsStore,
		optionsComponent: TableOptions as unknown as typeof SvelteComponent,
		columns: {
			approved: {
				disableFiltering: true,
				instructions: {
					renderComponent: TableIsApproved as unknown as typeof SvelteComponent
				},
				exclude: false
			},
			description: {
				disableFiltering: true,
				exclude: false
			},
			externalLinks: {
				header: 'External Link',
				instructions: {
					renderComponent: TableExnternalLink as unknown as typeof SvelteComponent
				},
				disableFiltering: true,
				exclude: true
			},
			id: {
				fixedWidth: 60,
				disableFiltering: true,
				exclude: false
			},
			name: {
				disableFiltering: false,
				exclude: false
			},
			related_meaning: {
				header: 'Related to',
				instructions: {
					renderComponent: TableMeaning as unknown as typeof SvelteComponent
				},
				disableFiltering: true,
				exclude: true
			},
			selectable: {
				instructions: {
					renderComponent: TableIsApproved as unknown as typeof SvelteComponent
				},
				disableFiltering: true,
				exclude: false
			},
			constraints: {
				disableFiltering: true,
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
		meaning = new MeaningModel(null);
	}

	function edit(type: any) {
		if (type.action == 'edit') {
			meaning = new MeaningModel(null);
			
			showForm = false;
			meaning = $meaningsStore.find((u) => u.id === type.id)!;
			showForm = true;
		}
		if (type.action == 'delete') {
			let m: MeaningModel = $meaningsStore.find((u) => u.id === type.id)!;
			const confirm: ModalSettings = {
				type: 'confirm',
				title: 'Delete Meaning',
				body: 'Are you sure you wish to delete Meaning ' + m.name + '?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: async (r: boolean) => {
					if (r === true) {
						let success: boolean = await deleteFn(m);
						if (success) {
							await reload(true);
							if (m.id === meaning.id) {
								toggleForm();
							}
						}
					}
				}
			};
			modalStore.trigger(confirm);
		}
	}

	async function deleteFn(m: MeaningModel): Promise<boolean> {
		const res = await remove(m.id);
		if (res) {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Meaning "' + m.name + '"  deleted.'
			});

			return true;
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: 'Can\'t delete Meaning "' + m.name + '" because it is still linked to one or more variable templates or variables.'
			});

			return false;
		}
	}

	async function onSuccessFn(id: number) {
		const message = id > 0 ? 'Meaning updated.' : 'Meaning created.';

		notificationStore.showNotification({
			notificationType: notificationType.success,
			message: message
		});

		// First: instigate background data fetching right away so the table refreshes
        await reload(true);

        // Second: Close the form structure smoothly.
        showForm = false;
        
        // Third: Completely eliminate setTimeout. Svelte can safely clear data 
        // objects when they aren't bound to active components.
        clear();
	}

	function onFailFn() {
		notificationStore.showNotification({
			notificationType: notificationType.error,
			message: "Can't save Meaning."
		});
	}

	let links: linkType[] = [
		{
			label: 'Manual',
			url: '/home/docs/Data%20Description#meanings'
		}
	];
</script>

<Page help={true} title="Manage Meanings" {links}>
	{#if isLoading}
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
	{:else if loadingError}
		<ErrorMessage error={loadingError} />
	{:else}
		<!-- svelte-ignore a11y-click-events-have-key-events -->
		<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h3 h-9">
				{#if meaning.id < 1}
					<span in:fade={{ delay: 400 }} out:fade>Create new Meaning</span>
				{:else}
					<span in:fade={{ delay: 400 }} out:fade>{meaning.name}</span>
				{/if}
			</div>
			<div class="text-right">
				{#if !showForm}
					<!-- svelte-ignore a11y-mouse-events-have-key-events -->
					<button
						transition:fade
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						title="Create new Meaning"
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
				<Meaning
					{meaning}
					on:cancel={() => toggleForm()}
					on:success={() => onSuccessFn(meaning.id)}
					on:fail={onFailFn}
				/>
			</div>
		{/if}

		<div class="table table-compact w-full">
			<Table on:action={(obj) => edit(obj.detail.type)} config={m} />
		</div>
	{/if}
</Page>

<Modal />
