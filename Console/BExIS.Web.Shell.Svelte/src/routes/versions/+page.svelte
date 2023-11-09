<script lang="ts">    

import { onMount } from 'svelte';

import { setApiConfig }  from '@bexis2/bexis2-core-ui'
import { getVersions }  from '../../services/versionManager'
import type { ReadVersionsModel } from "../../models/versionModels";

let versions:ReadVersionsModel;

onMount(async () => {
    setApiConfig("https://localhost:44345", "sdfsdfs", "sdfsdfsdf");
    console.log("SUPI");
    versions = await getVersions();
    console.log(versions)
})

</script>

{#if versions}
<div>
    <div>Database: {versions.database}</div>
    <div>Workspace: {versions.workspace}</div>
    <div>Site: {versions.site}</div>
</div>
{:else}
<div class="loading"></div>
{/if}