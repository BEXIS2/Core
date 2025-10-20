<script lang="ts">
	import { Tab, TabGroup } from '@skeletonlabs/skeleton';
	import Entity from './Entity.svelte';
	import type { ExtensionType } from './types';
	import ExtensionCreation from './ExtensionCreation.svelte';

 let tabSet: number = 0;
 export let id: number = 0;
	export let version: number;
 export let title = "";
 export let extensions:ExtensionType[] = []

</script>

<TabGroup>
 <Tab bind:group={tabSet} name="entity" value={0}>
  entity
 </Tab>
 <!--existing extentions -->

 {#each extensions as ext (ext.id)}
  <Tab bind:group={tabSet} name={ext.title} value={ext.id}>
   {ext.title} 
  </Tab>
 {/each}
 <!--add extentions -->
 <Tab bind:group={tabSet}  name="add" value={100}>
  +
 </Tab>

  <svelte:fragment slot="panel">

  {#if tabSet === 0}
   <Entity {id} {version} {title} />
  {:else if tabSet === 100}
   <ExtensionCreation {id}/>
  {/if}

 {#each extensions as ext (ext.id)}

   {#if tabSet === ext.id}
    <Entity id={ext.id} version={0} title={ext.title} />
   {/if}
  {/each}
 </svelte:fragment>
</TabGroup>
