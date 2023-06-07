<script lang="ts">    

    import { onMount } from 'svelte';
    
    import { setApiConfig }  from '@bexis2/bexis2-core-ui'
    import { getModules }  from '../../services/moduleService'
    import type { ReadModuleModel } from "../../models/moduleModels";
    
    let modules:Array<ReadModuleModel>;
    
    onMount(async () => {
        setApiConfig("https://localhost:44345", "sdfsdfs", "sdfsdfsdf");
        console.log("SUPI");
        modules = await getModules();
        console.log(modules);
    })
    
    </script>
    
    {#if modules && modules.length > 0}

        {#each modules as m} 
            {#if m.id == 'RPM'}
            <div>
            <div>Id: {m.id}</div>
            <div>Title: {m.title}</div>
            <div>Description: {m.description}</div>

            {#each m.settings['Settings'] as Setting}
                <div>{Setting.Name}</div>
            {/each}
        </div>

            {/if}
            <!-- <div>Settings: {JSON.stringify(m.settings)}</div> -->
        
        {/each}
    {:else}
    <div class="loading"></div>
    {/if}