<script lang="ts">
	import type { ExternalApiMetadata, SourceKeyInfoItem, ClbOptions, GbifOptions, IApiOptions } from "$lib/types/types";
	import { MultiSelect } from "@bexis2/bexis2-core-ui";


    export let externalApiMetadata: ExternalApiMetadata;
    let selectedApiOption: string = "";
    let selectedMetaOption: string = "";

    const createDefaultClb = (): ClbOptions => ({
        type: 'clb',
        sourceKey: '',
        synonyms: true
    });

    const createDefaultGbif = (): GbifOptions => ({
        type: 'gbif',
        parameter1: '',
        parameter2: ''
    });


    export let selectedOptions: IApiOptions = createDefaultClb();

    interface MultiSelectOption {
        value: string, label: string
    }

    $: apiSelectOptions = Object.entries(externalApiMetadata || {}).map(([key, value]) => {
        return {
            value: key,
            label: key
        };
    });

    $: metaDataSelectOptions = (externalApiMetadata?.clb?.sourceKeyInfo || []).map((item: SourceKeyInfoItem): MultiSelectOption => {
        return {
            value: item.sourceKey,
            label: `(${item.sourceKey}) ${item.title} ${item.alias}`
        }
    })

    // TODO: - only for testing purposes (only works with CLB), abstract this later
    function testHandleMetaOptionsChange(e: CustomEvent<any>) {
        const value: string = e.detail.value;
        var testOptions = createDefaultClb();
        testOptions.sourceKey = e.detail.value;

        selectedOptions = testOptions;
	}
</script>


{#if apiSelectOptions.length > 0}
        <MultiSelect
            id="apiSelector"
            title="Api Selector"
            source={apiSelectOptions}
            bind:target={selectedApiOption}
            isMulti={false}
        />

        <!-- TODO: Replace this with api-specific child component! -->
        <MultiSelect
            id="apiSelector"
            title="Api Selector"
            source={metaDataSelectOptions}
            bind:target={selectedMetaOption}
            on:change={testHandleMetaOptionsChange}
            isMulti={false}
        />
{/if}