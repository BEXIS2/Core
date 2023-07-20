<script lang="ts">
	import { onMount } from 'svelte';

    import Fa from 'svelte-fa/src/index'
    import { faSave } from '@fortawesome/free-solid-svg-icons'

    import { Page } from '@bexis2/bexis2-core-ui';
    import Entry from '../../components/entry.svelte'
	import { get, putByModuleId } from '../../services/moduleService';
	import type { ReadModuleModel } from '../../models/moduleModels';
	import type { ReadEntryModel } from '$models/settingModels';

	let module: ReadModuleModel;

	onMount(async () => {
		// module = await getModuleByName('sam');
	});

    async function getSettings() {
        const response = await get(); //ByName('sam');
        if (response?.status == 200) {
            console.log(response.data);
            return await response.data;
        }

        throw new Error("Something went wrong.");
    }

    export async function putSettings(moduleId:string, model:any)
    {
        const response = await putByModuleId(moduleId, model); //ByName('sam');
        if (response?.status == 200) {
            return await response.data;
        }

        throw new Error("Something went wrong.");
    }
</script>

<Page>
    <div>
        {#await getSettings()}
            <div id="spinner">... loading ...</div>
        {:then data}
        {#each data as m}
        <form on:submit|preventDefault={() => putSettings(m.id, m)}>
        <div>Id: {m.id}</div>
        <div>Title: {m.title}</div>
        <div>Description: {m.description}</div>
    
        <!-- <div>Settings: {module.settings['entries']}</div> -->
        {#each m.entries as entry}
            <Entry entry={entry}/>
        {/each}

        <button class="btn variant-filled-primary" type="submit">
            <Fa icon={faSave}/>
        </button>
        </form>
        {/each}
        {:catch error}
            <div id="spinner">{error}</div>
        {/await}
    </div>

</Page>