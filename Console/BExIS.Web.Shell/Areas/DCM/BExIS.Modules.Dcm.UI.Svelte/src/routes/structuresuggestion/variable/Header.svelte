<script>

 import {SlideToggle} from '@skeletonlabs/skeleton'
 import {Alert} from '@bexis2/bexis2-core-ui'
 import Fa from 'svelte-fa';
 import {faCheck} from '@fortawesome/free-solid-svg-icons'
	import Variable from './Variable.svelte';

 export let index;
 export let name;
 export let isKey;
 export let isOptional;
 export let isValid;

 </script>

<div id={name} class="flex gap-5">

    <div class="grow flex gap-2">
      <h1 class="h1">{name}</h1> 
      <div class="text-success-500">{#if isValid}<Fa icon={faCheck}/>{/if}</div>
    </div>

    <div  class="flex-none max-w-5xl flex gap-2 text-right">
    {#if isKey && isOptional}
    <div>
      <Alert cssClass="variant-filled-warning" deleteBtn={false}>
        If variables are defined as part of the primary key and are optional, there may be complications when uploading.
     </Alert>
    </div>
    {/if}
    {#if isOptional}
    <div>
      <Alert cssClass="variant-filled-warning" deleteBtn={false}>
        Please consider briefly if it would be possible to define missing values instead of leaving the field optional.
     </Alert>

     </div>
    {/if}
   </div>
    <div class="flex-none flex-col text-right">
      <div><SlideToggle size="sm" name="isKey" bind:checked={isKey} active="bg-primary-500">Mark as part of primary keys</SlideToggle></div>
      <div><SlideToggle size="sm" name="isOptional" active="bg-primary-500" bind:checked={isOptional}>Value can be optional</SlideToggle></div>
    </div>
</div>