<script lang="ts">
	import { onMount } from 'svelte';

	import Fa from 'svelte-fa';
	import { faSave } from '@fortawesome/free-solid-svg-icons';

	import { Page, notificationType, notificationStore } from '@bexis2/bexis2-core-ui';
	import Entry from '../../components/entry.svelte';
	import { get, putByModuleId } from '../../services/settingService';
	import type { ReadSettingModel } from '$models/settingModels';

	import { Accordion, AccordionItem } from '@skeletonlabs/skeleton';

	onMount(async () => {
		// module = await getModuleByName('sam');
	});

	async function getSettings() {
		const response = await get();
		console.log(response);
		if (response?.status == 200) {
			return await response.data;
		}

		throw new Error('Something went wrong.');
	}

	export async function putSettingByModuleId(moduleId: string, model: ReadSettingModel) {
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
</script>

<Page>
	<div class="w-full">
		{#await getSettings()}
			<div id="spinner">... loading ...</div>
		{:then data}
			<Accordion />
			{#each data as m}
				<AccordionItem>
					<svelte:fragment slot="lead">
                        <i class="fa-solid fa-skull text-xl w-6 text-center" />
                    </svelte:fragment>
					<svelte:fragment slot="summary">
                        <h1>{m.name} ({m.id})</h1>
                    </svelte:fragment>
					<svelte:fragment slot="content">
						<form on:submit|preventDefault={() => putSettingByModuleId(m.id, m)}>
							{#each m.entries as entry}
								<Entry {entry} />
							{/each}

							<div class="py-5 text-right col-span-2">
								<button class="btn variant-filled-primary h-9 w-16 shadow-md" type="submit">
									<Fa icon={faSave} />
								</button>
							</div>
						</form>
					</svelte:fragment>
				</AccordionItem>
			{/each}
		{:catch error}
			<div id="spinner">{error}</div>
		{/await}
	</div>
</Page>
