<script>
import Fa from 'svelte-fa';
import { faXmark } from '@fortawesome/free-solid-svg-icons';

export let name;
export let value;

</script>

<div class="grid grid-cols-6 gap-2">
 <div class="col-span-2"><b>{name} :</b></div>
 <div class="col-span-4">
  {#if value}
   {value}
   {:else}
   <span class="chip variant-ghost-error" ><Fa icon={faXmark}></Fa></span>
  {/if}
 </div>
</div>