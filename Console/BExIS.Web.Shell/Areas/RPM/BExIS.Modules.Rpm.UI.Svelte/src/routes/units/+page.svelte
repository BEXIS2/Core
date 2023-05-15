<script lang="ts">

import { onMount } from 'svelte';
import { SlideToggle } from '@skeletonlabs/skeleton';
import { setApiConfig }  from '@bexis2/bexis2-core-ui';
import { getUnitListItems }  from '../services/unitCaller';

import UnitTable from '../components/unitTable.svelte';

import type { UnitListItem } from "../../models/Models";
  
let u: UnitListItem[];
$:units = u;

onMount(async () => {
  setApiConfig("https://localhost:44345","*","*");
  u = await getUnitListItems();
  console.log(u);
})

</script>
<h1>units</h1>

{#if units}
  <UnitTable units = {units}/>
{/if}