<script lang="ts">
import { onMount }from 'svelte'
import Fa from 'svelte-fa'
import { faHourglass } from '@fortawesome/free-regular-svg-icons/index'

export let milliseconds = 0;
$:currentDate = new Date();
let diff:string;
$:diff;

onMount(async () => {

  const interval = setInterval(() => {
    currentDate = new Date();
    diff = formatTime(Number(currentDate) - milliseconds);

		}, 1000);

})


// utility functions used in the project
// prepend a zero to integers smaller than 10 (used for the second and minute values)
function zeroPadded(number) {
    return number >= 10 ? number.toString() : `0${number}`;
}

/* format time in the following format
mm:ss
zero padded minutes, zero padded seconds, tenths of seconds
*/
function formatTime(milliseconds) {
    // const hh = zeroPadded(Math.floor(milliseconds / 1000 / 60 / 24));
    const mm = zeroPadded(Math.floor(milliseconds / 1000 / 60));
    // const ss = zeroPadded(Math.floor(milliseconds / 1000) % 60);

    if(mm<60) {
      return `${mm} min`;
    }
    else 
    {
      return `> 60 min`
    }
}


</script>
<div class="flex gap-1 pb-5">
  <div class="self-center"><Fa icon={faHourglass} /></div>
  <div class="self-center"> 
  {#if diff}
    {diff}
  {/if}
  </div>

</div>
