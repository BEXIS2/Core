<script>
 import Hook from './Hook.svelte'
 import { hosturl } from '../stores/store.js'

export let id=0;
export let version=1;
export let status=0;
export let displayName="";
export let start="";
export let description="";

let isEnabled = setEnable(status);

let url = hosturl+start+"?id="+id+"&version="+version;
 
function setEnable(status)
{
  if(status == 0 ||status == 1 ) // disabled || access denied
  {
    return false;
  }
    
  return true; // every other status enable the hook
}
</script>

{#if isEnabled}
<a href="{url}" target="_blank">
  <Hook {status} {displayName} {description} />
</a>
{/if}

<style>
 a,a:hover
 {
  text-decoration: none;
 }
</style>