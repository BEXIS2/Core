<script>

import Fa from 'svelte-fa/src/fa.svelte'
import { faFileAlt } from '@fortawesome/free-solid-svg-icons'


 export let id;
 export let index;
 export let name;
 export let description;


</script>

<!-- svelte-ignore a11y-click-events-have-key-events -->
<div class="m-1 flex w-full items-center gap-10 border-b-violet-700 p-5 shadow-md ring-1 ring-gray-900/5
hover:bg-primary-100 space-x-100 active:bg-primary-300" on:click >

   <Fa size="3x" icon={faFileAlt}/>

   <div>
    <h3>{name}</h3> 
    <span >{description}</span>
   </div>

</div>
