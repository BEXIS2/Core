<script>
 import { onMount } from 'svelte'; 
 import { hosturl } from '../stores/store.js'
	import Header from '../components/view/Header.svelte'
	import Tab from '../components/view/Tab.svelte'
	import Debug from '../components/Debug.svelte'
	import {TabContent, Container } from 'sveltestrap';

 // load attributes from div
let container = document.getElementById('view');
let id = container.getAttribute("dataset");
$:version = container.getAttribute("version");

let title = "";
let model = false;

// tabs
let activeTab = 'metadata';

$:hooks=null;

// for test ui
$:testPage="";

onMount(async () => {

// load menu froms server
const url = hosturl+'dcm/view/load?id='+id;
const res = await fetch(url);
model = await res.json();

hooks = model.hooks;
title = model.title;
version = model.version


//test ui as html
const testPageUrl = hosturl+'dcm/view/test';
const resTestPage = await fetch(testPageUrl);
testPage = await resTestPage.text();
console.log(testPage)
})

async function loadTab(action, id, version)
{
			const url = hosturl+action+"?id="+id+"&version="+version;
			const res = await fetch(url);
			let test = await res.text();
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