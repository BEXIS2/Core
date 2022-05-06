<script>

import Variable from './Variable.svelte'
import {Spinner} from 'sveltestrap';

export let variables = [];
// [
//     {
//         "id": 0,
//         "name": "EP",
//         "description": "",
//         "systemType": "String",
//         "dataType": {
//             "id": 0,
//             "text": ""
//         },
//         "unit": {
//             "id": 0,
//             "text": "none"
//         },
//         "template": {
//             "id": 0,
//             "text": ""
//         }
//     }
// ];

</script>

<h1>Suggestion</h1>

{#if !variables}
  <!-- content here -->
  <Spinner color="primary" size="sm" type ="grow" text-center />
{:else}
  <!-- else content here -->
  {#each variables as variable, i}
    <!-- content here -->
    <Variable {variable} index={i} on:change={console.log(variable)}/>
  {/each}
{/if}