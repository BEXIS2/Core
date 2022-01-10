<script>
import { onMount } from 'svelte'; 
import { hosturl } from '../../stores/store.js'
import {TabPane, Spinner } from 'sveltestrap';


//entity infos
export let id;
export let version;

//entity hook infos
export let name;
export let displayName;
export let start

// tab pane properties
export let active;


$:content=null;

onMount(async () => {

console.log(name);

// load menu froms server
const url = hosturl+start+'?id='+id+'&version='+version;
const res = await fetch(url);
content = await res.text();

})

</script>

<TabPane id="{name}" tabId="{name}" tab={displayName.toUpperCase()} {active}> 
{#if content}
 <div class="tab-content">
  {@html content}
 </div>
 {:else} <!-- while data is not loaded show a loading information -->

 <div class="spinner-container">
  <Spinner color="primary" size="sm" type ="grow" text-center />
 </div>
{/if}

</TabPane>

<style>

.tab-content
{
 
 padding: 1rem;
 /* background-color: var(--bg-tab); */
 border-bottom: 1px #eee solid;
 border-left: 1px #eee solid;
 border-right: 1px #eee solid;
}

.spinner-container
{
  margin:2rem;
}

</style>