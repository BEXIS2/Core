<script>
import { onMount, onDestroy } from 'svelte'; 
import { hosturl } from '../stores/store.js'
import { Spinner, Button } from 'sveltestrap';

 
 //entity infos
 export let id;
 export let version;
 
 //entity view infos
 export let name;
 export let displayName;
 export let start;

$:ExtComponent = null;

onMount(async () =>
{
  // load javascript from server
  const urlscript = hosturl+start+'?id='+id+'&version='+version;

  import(urlscript).then(resp => {
  ExtComponent = resp.default;
  });

})

</script>
 
<div id="{name}_view">
  <div id="{name}" dataset="{id}" version="{version}" />
  <svelte:component this={ExtComponent}/>
</div>
