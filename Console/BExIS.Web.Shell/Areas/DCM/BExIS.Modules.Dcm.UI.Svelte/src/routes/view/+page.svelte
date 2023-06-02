<script lang="ts">
 import { onMount } from 'svelte'; 

 import { getView }  from './services'
 import { getViewStart }  from '../../services/HookCaller'
 import { setApiConfig }  from '@bexis2/bexis2-core-ui'

	import Header from './Header.svelte'
	import TabContent from './Tab.svelte'
	import Debug from '../../lib/components/Debug.svelte'
	import {TabGroup, Tab} from '@skeletonlabs/skeleton';

	//types
import type {ViewModel, Hook} from '../../models/View'

export let data;
let model:ViewModel;
model = data.model

let title = "";
let version;
let id;

// tabs
let activeTab = 'metadata';

let tabSet = 0;

let hookList:Hook[];
$:hooks=hookList;

// for test ui
$:testPage="";

onMount(async () => {


	

console.log("onmount",model);

hooks = model.hooks;
title = model.title;
version = model.version
id = model.id;

console.log(model);

//test ui as html
// const resTestPage = await fetch('dcm/view/test');
// testPage = await resTestPage.text();

})

async function loadTab(action, id, version)
{
	  let test = await getViewStart(action,id,version);
			return test;
}

</script>

<div>

<!-- Header -->
<Header {id} {version} {title} />


{#if hooks}
<TabGroup>
<!-- nav -->	
	{#each hooks as hook,i}
			<Tab bind:group={tabSet} {id} {version} {...hook} value={i}/>
	{/each}

<!-- content -->
		<!-- {#each hooks as hook,i}
			<Tab {id} {version} {...hook} active={i==0}/>
		{/each} -->
</TabGroup>

{/if}


<Debug {hooks}/>

</div>
