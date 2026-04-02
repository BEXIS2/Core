<script lang="ts">
	import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner } from "@bexis2/bexis2-core-ui";
    import { loadDataStructure } from "./services";
    import type { HeaderMappings, MappingEntry } from "$lib/types/types";
    import type { DataStructureEditModel, MultiSelectSourceDetailed } from "./types";
	import { onMount } from "svelte";
	import { MultiSelect } from "@bexis2/bexis2-core-ui";
    import { submitHeaderMappings } from "./services";
    import { mappingSelection } from '../../lib/stores/selectionStore';

    let dataStructure: DataStructureEditModel;
    let headerMappings: HeaderMappings = {
        datastructureId: -1,
        datasetId: 1,
        mappings: []
    };
    let selectedOptionsHelper: MultiSelectSourceDetailed[] = [];
    $: headerMappingOptions = [
		{ value: 'scientificName', label: 'scientificName' },
		{ value: 'authorship', label: 'authorship' },
		{ value: 'rank', label: 'rank' },
		{ value: 'kingdom', label: 'kingdom' },
		{ value: 'IGNORE', label: 'IGNORE' },
		// ...
	]
    $: headerMappingValid = false;
    $: handlingSubmit = false;

    onMount(() => {
        async function test() {
            var dataStructureHelper: DataStructureEditModel = await loadDataStructure($mappingSelection.datastructureId);
            
            // build headerMappings and pre-assign values if possible based on meanings
            for (let i = 0; i < dataStructureHelper.variables.length; i++) {
                // TODO: maybe refine this a little bit more
                const hasScientificName: boolean = dataStructureHelper.variables[i].meanings.some(
                    (m) => m.text === "scientificName"
                );

                const hasKingdom: boolean = dataStructureHelper.variables[i].meanings.some(
                    (m) => m.text === "kingdom"
                );

                const hasTaxonRank: boolean = dataStructureHelper.variables[i].meanings.some(
                    (m) => m.text === "taxonRank"
                );

                var preassignedMapping: string = "IGNORE";

                if (hasScientificName) {
                    preassignedMapping = "scientificName";
                } else if (hasKingdom) {
                    preassignedMapping = "kingdom";
                } else if (hasTaxonRank) {
                    preassignedMapping = "rank";
                }

                headerMappings.mappings.push({
                    variableId: dataStructureHelper.variables[i].id,
                    variableName: dataStructureHelper.variables[i].name,
                    headerMapping: preassignedMapping
                })
                headerMappings.datastructureId = dataStructureHelper.id;

                tidyMappingsAfterChange(preassignedMapping);
            }

            dataStructure = dataStructureHelper;
            headerMappings.datasetId = $mappingSelection.datasetId;
            headerMappings.datastructureId = $mappingSelection.datastructureId;

            validateHeaderMappings();
        }

        test();
    });

    // remove/add Mapping options based on the selected value
    function tidyMappingsAfterChange(value: string) {
        const itemToMove = headerMappingOptions.find(item => item.value === value);
        
        // remove from options (on select)
        if (value !== "IGNORE") {
            if (itemToMove) {
                selectedOptionsHelper = [...selectedOptionsHelper, itemToMove];
                headerMappingOptions = headerMappingOptions.filter(item => item.value !== value);
            }
        }
    
        // add to options (on deselect)
        for (let i = 0; i < selectedOptionsHelper.length; i++) {
            const target: MultiSelectSourceDetailed = selectedOptionsHelper[i];
            const targetHeaderMapping: MappingEntry | undefined = headerMappings.mappings.find(item => item.headerMapping === target.value);
            if (!targetHeaderMapping) {
                headerMappingOptions = [...headerMappingOptions, target];
                selectedOptionsHelper = selectedOptionsHelper.filter(item => item.value !== target.value);
            }
        }

        validateHeaderMappings();
    }

    // handles MultiSelect change event
    function handleHeaderMappingChanged(e: CustomEvent<any>) {
        const value: string = e.detail.value;
        
        tidyMappingsAfterChange(value);
	}

    // check if HeaderMappings is in a state that allows for submission
    function validateHeaderMappings() {
        // TODO: extend this when conditions are clearer
		if (headerMappings.mappings.filter(item => item.headerMapping == 'scientificName').length != 1) {
			headerMappingValid = false;
			return;
		} else {
            headerMappingValid = true;
        }
    }

    async function handleSubmit() {
        handlingSubmit = true;
        console.log(headerMappings);
        const response = await submitHeaderMappings(headerMappings);
        console.log(response);
    }

</script>

<Page 
	title="Header Mapping" 
	note=""
	contentLayoutType={pageContentLayoutType.center}
>

<h2 class="h2">Select header mapping</h2>

<p>
    You are working on <b>Dataset: {$mappingSelection.datasetId}</b>. Below this text, the corresponding <b>Datastructure: {$mappingSelection.datastructureId}</b> is shown. Each row corresponds to a column in the original dataset,
    and an option to select a mapping for it. Some mappings might already be pre-assigned based on your datasets metadata. Please try to select as many matching mappings as possible, but at the very least select a scientificName mapping.
    For columns that have no clear associated mapping, just select IGNORE. If there are no conflicts, you should be able to submit the mappings with the button below.
</p>

<p>
    This information is used to cut off unnecessary data and help matching APIs understand your data better. Your original data will <b>NOT</b> be changed at any step and will remain fully functional.
</p>

{#if dataStructure}
    {#each dataStructure.variables as variable, i}
        <div class="grid grid-cols-5">
			<div class="flex items-center justify-center col-span-1">{variable.name}</div>
			<div class="flex items-center justify-center col-span-1">+</div>
			<div class="col-span-3">
				<MultiSelect 
					id={`header_${variable.id}`}
					title={``}
					source={headerMappingOptions}
					bind:target={headerMappings.mappings[i].headerMapping}
					isMulti={false}
					on:change={handleHeaderMappingChanged}
				/>
			</div>
		</div>
    {/each}

    <button class="btn variant-filled-secondary" disabled={(!headerMappingValid || handlingSubmit)} on:click|preventDefault={handleSubmit}>Submit</button>
{/if}

<div class="h-80"></div>

</Page>