<script lang="ts">
  import { onMount } from 'svelte';
  import { SlideToggle } from '@skeletonlabs/skeleton';

  import Fa from 'svelte-fa/src/fa.svelte'
  import { faPen } from '@fortawesome/free-solid-svg-icons'

  export let row:any;
	export let dispatchFn:any;

</script>
<tableOption>
  <button type="button" class="btn variant-filled" on:click|preventDefault={() => 
    dispatchFn({
      type: {action: 'edit', id: row.id}
    })}>
      <Fa icon={faPen} /></button>
</tableOption>