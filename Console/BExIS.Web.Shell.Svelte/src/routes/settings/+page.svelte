<script lang="ts">
	import { onMount } from 'svelte';

	import Fa from 'svelte-fa';
	import { faSave } from '@fortawesome/free-solid-svg-icons';

	import { Page, notificationType, notificationStore, helpStore } from '@bexis2/bexis2-core-ui';
	import type { helpItemType } from '@bexis2/bexis2-core-ui';
	import Entry from '../../components/entry.svelte';
	import { get, getByModuleId, putByModuleId } from '../../services/settingManager';
	import { UpdateSettingModel } from '$models/settingModels';

	import { ListBox, ListBoxItem } from '@skeletonlabs/skeleton';

	onMount(async () => {
		// module = await getModuleByName('sam');
	});

	async function getSettings() {
		const response = await get();
		console.log(response);
		if (response?.status == 200) {
			var modules = await response.data;
			console.log('🚀 ~ file: +page.svelte:24 ~ getSettings ~ modules:', modules);

			if (modules.length > 0) {
				module = modules[0].id;
			}

			return modules;
		}

		throw new Error('Something went wrong.');
	}

	async function getSettingsByModuleId(moduleId) {
		const response = await getByModuleId(moduleId);
		console.log(response);
		if (response?.status == 200) {
			var settings = await response.data;
			helpStore.setHelpItemList(
				settings.entries.map(
					(e) => ({ id: e.key, name: e.key, description: e.description }) as helpItemType
				)
			);
			return settings;
		}

		throw new Error('Something went wrong.');
	}

	export async function putSettingByModuleId(moduleId: string, model: UpdateSettingModel) {
		console.log(model);
		const response = await putByModuleId(moduleId, model);
		if (response?.status == 200) {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: `The update of settings for module ${moduleId} succeeded.`
			});
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
</script>

<Page help={true} fixLeft={false}>
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
		<form
			on:submit|preventDefault={() => putSettingByModuleId(data.id, new UpdateSettingModel(data))}
		>
			{#each data.entries as entry}
				<Entry {entry} />
			{/each}

			<div class="py-5 text-right col-span-2">
				<button class="btn variant-filled-primary h-9 w-16 shadow-md" type="submit">
					<Fa icon={faSave} />
				</button>
			</div>
		</form>
	{:catch error}
		<div id="spinner">{error}</div>
	{/await}

	<div />
</Page>
