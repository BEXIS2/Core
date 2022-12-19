<script>
 import { onMount } from 'svelte'; 

 import { getView }  from './caller'
 import { getViewStart }  from '../../services/HookCaller'
 import { setApiConfig }  from '@bexis2/bexis2-core-ui/src/lib/index'

	import Header from './Header.svelte'
	import Tab from './Tab.svelte'
	import Debug from '../../lib/components/Debug.svelte'
	import {TabContent, Container } from 'sveltestrap';


 // load attributes from div
let container = document.getElementById('view');
let id = container?.getAttribute("dataset");
$:version = container?.getAttribute("version");

let title = "";
let model = false;

// tabs
let activeTab = 'metadata';

$:hooks=null;

// for test ui
$:testPage="";

onMount(async () => {

 setApiConfig("https://localhost:44345","davidschoene","123456");
// load menu froms server
model = await getView(id);

hooks = model.hooks;
title = model.title;
version = model.version


//test ui as html
const resTestPage = await fetch('dcm/view/test');
testPage = await resTestPage.text();
console.log(testPage)
})

async function loadTab(action, id, version)
{
	  let test = await getViewStart(action,id,version);
			return test;
}

</script>

<Container fluid>

<!-- Header -->
<Header {id} {version} {title} />

{#if hooks}
<div class="tab-container">
<!-- nav -->
	<TabContent {activeTab} >
		{#each hooks as hook,i}
			<Tab {id} {version} {...hook} active={i==0}/>
		{/each}
	</TabContent>
</div>
{/if}


<Debug {hooks}/>

</Container>


<style>

	.tab-container
	{
		margin: 1rem 0; 
	
	}
	
	</style>