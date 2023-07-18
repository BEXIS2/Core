<script>
import Fa from 'svelte-fa';
import { faXmark } from '@fortawesome/free-solid-svg-icons';

export let value;

</script>

<b>
  {#if value}
   {value}
   {:else}
   <span class="chip variant-ghost-error" ><Fa icon={faXmark}></Fa></span>
  {/if}
</b>