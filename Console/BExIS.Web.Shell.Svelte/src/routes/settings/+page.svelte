<script lang="ts">
	import { onMount } from 'svelte';

	import Fa from 'svelte-fa';
	import { faSave, faCircleDot } from '@fortawesome/free-solid-svg-icons';

	import { Page, notificationType, notificationStore, helpStore } from '@bexis2/bexis2-core-ui';
	import type { helpItemType } from '@bexis2/bexis2-core-ui';
	import type { linkType } from '@bexis2/bexis2-core-ui';

	import Entry from '../../components/entry.svelte';
	import { get, getByModuleId, putByModuleId } from '../../services/settingManager';
	import { UpdateSettingModel } from '$models/settingModels';

	import { ListBox, ListBoxItem } from '@skeletonlabs/skeleton';

	onMount(async () => {});

	async function getSettings() {
		const response = await get();
		if (response?.status == 200) {
			var modules = await response.data;
			if (modules.length > 0) {
				module = modules[0].id;
			}
			return modules;
		}
		throw new Error('Something went wrong.');
	}

	async function getSettingsByModuleId(moduleId) {
		const response = await getByModuleId(moduleId);
		if (response?.status == 200) {
			var settings = await response.data;
			helpStore.setHelpItemList(
				settings.entries.map(
					(e) => ({ id: e.key, name: e.key, description: e.description }) as helpItemType
				)
			);
			originalSnapshot = JSON.stringify(settings);
			isDirty = false;
			return settings;
		}
		throw new Error('Something went wrong.');
	}

	export async function putSettingByModuleId(moduleId: string, model: UpdateSettingModel) {
		const response = await putByModuleId(moduleId, model);
		if (response?.status == 200) {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: `The update of settings for module ${moduleId} succeeded.`
			});
			originalSnapshot = JSON.stringify(model);
			isDirty = false;
			return await response.data;
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: `The update of settings for module ${moduleId} failed.`
			});
		}
		throw new Error('Something went wrong.');
	}

	let module: string = 'shell';

	let links: linkType[] = [
		{
			label: 'Manual',
			url: '/home/docs/Configuration#configuration-ui'
		}
	];

	// --- Dirty tracking ---
	let originalSnapshot: string = '';
	let isDirty = false;
	let currentData: any = null;

	function checkDirty() {
		if (!currentData || !originalSnapshot) return false;
		const current = JSON.stringify(currentData);
		const dirty = current !== originalSnapshot;
		console.log('checkDirty:', dirty, 'current:', current.substring(0, 100), 'original:', originalSnapshot.substring(0, 100));
		return dirty;
	}

	function onFormChange() {
		isDirty = true;
	}

	// --- Module switching with unsaved changes guard ---
	let previousModule: string = 'shell';
	let pendingModule: string | null = null;
	let showUnsavedWarning = false;

	// Intercept module changes via reactive statement
	$: if (module !== previousModule) {
		if (isDirty) {
			pendingModule = module;
			module = previousModule; // revert until user decides
			showUnsavedWarning = true;
		} else {
			previousModule = module;
		}
	}

	function confirmDiscard() {
		isDirty = false;
		showUnsavedWarning = false;
		if (pendingModule) {
			previousModule = pendingModule;
			module = pendingModule;
			pendingModule = null;
		}
	}

	function cancelDiscard() {
		showUnsavedWarning = false;
		pendingModule = null;
	}

	async function saveAndSwitch() {
		if (currentData) {
			await putSettingByModuleId(currentData.id, new UpdateSettingModel(currentData));
		}
		showUnsavedWarning = false;
		if (pendingModule) {
			previousModule = pendingModule;
			module = pendingModule;
			pendingModule = null;
		}
	}
</script>

<Page help={true} fixLeft={false} {links}>
	<div slot="left">
		{#await getSettings()}
			<div id="spinner">... loading ...</div>
		{:then data}
			<ListBox active="variant-filled-primary">
				{#each data as m}
					<ListBoxItem bind:group={module} name="medium" value={m.id}>{m.name}</ListBoxItem>
				{/each}
			</ListBox>
		{:catch error}
			<div id="spinner">{error}</div>
		{/await}
	</div>
	{#await getSettingsByModuleId(module)}
		<div id="spinner">... loading ...</div>
	{:then data}
		{@const _ = (currentData = data)}
		<!-- Sticky save bar -->
		<div class="sticky top-0 mb-4 flex items-center justify-between bg-surface-100 dark:bg-surface-800 px-4 py-2 border border-surface-200 dark:border-surface-700 rounded shadow-sm">
			<div class="flex items-center gap-2">
				<span class="text-sm font-medium">{data.name}</span>
				{#if isDirty}
					<span class="badge variant-filled-warning text-xs inline-flex items-center gap-1">
						<Fa icon={faCircleDot} class="text-[0.5rem]" />
						Unsaved changes
					</span>
				{/if}
			</div>
			<button
				class="btn {isDirty ? 'variant-filled-primary' : 'variant-ghost-surface'} h-9 px-4 shadow-md inline-flex items-center gap-2"
				type="button"
				disabled={!isDirty}
				on:click={() => putSettingByModuleId(data.id, new UpdateSettingModel(data))}>
				<Fa icon={faSave} />
				<span class="text-sm">Save</span>
			</button>
		</div>

		<form
			on:submit|preventDefault={() => putSettingByModuleId(data.id, new UpdateSettingModel(data))}
			on:input={onFormChange}
			on:change={onFormChange}
		>
			{#each data.entries as entry}
				<Entry {entry} onDirty={onFormChange} />
			{/each}

			<div class="py-5 text-right col-span-2">
				<button class="btn variant-filled-primary h-9 w-16 shadow-md" type="submit" disabled={!isDirty}>
					<Fa icon={faSave} />
					<span class="text-sm">Save</span>
				</button>
			</div>
		</form>
	{:catch error}
		<div id="spinner">{error}</div>
	{/await}

	<div />
</Page>

<!-- Unsaved changes warning dialog -->
{#if showUnsavedWarning}
	<!-- svelte-ignore a11y-click-events-have-key-events -->
	<!-- svelte-ignore a11y-no-static-element-interactions -->
	<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/50" on:click={cancelDiscard}>
		<div class="card p-6 max-w-md mx-4" on:click|stopPropagation>
			<h3 class="h3 mb-2">Unsaved changes</h3>
			<p class="text-sm text-surface-600 dark:text-surface-300 mb-4">
				You have unsaved changes in the current settings section. Do you want to save them before switching?
			</p>
			<div class="flex justify-end gap-2">
				<button class="btn variant-ghost-surface" on:click={cancelDiscard}>Cancel</button>
				<button class="btn variant-soft-error" on:click={confirmDiscard}>Discard changes</button>
				<button class="btn variant-filled-primary" on:click={saveAndSwitch}>Save &amp; switch</button>
			</div>
		</div>
	</div>
{/if}
