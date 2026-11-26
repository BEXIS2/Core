<script lang="ts">
    import { TextInput } from '@bexis2/bexis2-core-ui';
    import type { MeaningModel } from '$lib/components/SearchConfig/SearchConfigModel';

    export let entitytemplates: any[] = [];
    export let searchConfigData: any;
    export let onChangeHandler: (event: Event | CustomEvent<any>) => void;
    export let res: any;

    const ensureExternalSources = (localCfg: any) => {
        if (!localCfg.external_sources) {
            localCfg.external_sources = { source: '', local_path: '', external_name: '' };
        }
        return localCfg.external_sources;
    };

    const getTemplateName = (templateId: number): string => {
        const fromList = entitytemplates?.find((et) => et.id === templateId);
        return fromList?.name ?? `Template #${templateId}`;
    };

    let externalSourcesMap = new Map();

    $: if (searchConfigData?.local) {
        externalSourcesMap.clear();
        searchConfigData.local.forEach((cfg: any) => {
            externalSourcesMap.set(cfg.entity_template_id, ensureExternalSources(cfg));
        });
    }
</script>

<h4 class="text-xl font-semibold mb-4">External Sources Configuration (needs different display / selection)</h4>

{#if !searchConfigData?.local}
    <p class="text-sm text-surface-500">No local configuration available.</p>
{:else}
    <div class="grid grid-cols-1 gap-6">
        {#each searchConfigData.local as localCfg (localCfg.entity_template_id)}
            {@const externalSources = externalSourcesMap.get(localCfg.entity_template_id)}
            {#key localCfg.entity_template_id}
                <div class="p-4 rounded-md border border-surface-200">
                    <h3 class="text-lg font-semibold mb-3">{getTemplateName(localCfg.entity_template_id)}</h3>
                    <div class="grid grid-cols-3 gap-4">
                        <TextInput
                            id={`external_source_${localCfg.entity_template_id}`}
                            label="Source"
                            value={externalSources?.source ?? ''}
                            
                        />
                        <TextInput
                            id={`external_local_path_${localCfg.entity_template_id}`}
                            label="Local Path"
                            value={externalSources?.local_path ?? ''}
                        />
                        <TextInput
                            id={`external_name_${localCfg.entity_template_id}`}
                            label="External Name"
                            value={externalSources?.external_name ?? ''}
        
                        />
                    </div>
                </div>
            {/key}
        {/each}
    </div>
{/if}
