<script>

import { onMount,  } from 'svelte'; 
import { fade  } from 'svelte/transition'; 
import {Spinner, Container} from 'sveltestrap';

import { getEdit }  from '../services/Caller'
import { setApiConfig }  from '@bexis2/svelte-bexis2-core-ui'

import Header from '../components/edit/Header.svelte'
import Data from '../components/edit/Data.svelte'
import Hooks from '../components/edit/Hooks.svelte'
import Message from '../components/edit/MessagesContainer.svelte'
import Debug from '../components/Debug.svelte'

// load attributes from div
let container = document.getElementById('edit');
let id = container.getAttribute("dataset");
$:version = container.getAttribute("version");

let title = "";
let model = false;

$:hooks=null;
$:views=null;

$:datasethooks= [];
$:addtionalhooks= [];
$:messageView=[];
$:additionalViews=[];

onMount(async () => {
  console.log("start edit");
  setApiConfig("https://localhost:44345","davidschoene","123456");
  load();
})

async function load()
{
  // load model froms server
  model = await getEdit(id);

  console.log(model);

  hooks = model.hooks;
  views = model.views;
  title = model.title;
  version = model.version


  // sam/ui/scripts/userpermission.js
  // load svelte

  // seperate dcm hooks from other hooks
  seperateHooks(hooks);

  // get resultView
  seperateViews(views);
}

// seperate dcm hooks from other hooks
// known hooks - metadata, fileupload, validation
function seperateHooks(hooks)
{
  hooks.forEach(element => {
     if(
     element.name == "metadata" || 
     element.name == "fileupload" || 
     element.name == "validation" ||
     element.name == "submit" ||
     element.name == "datadescription")
     {
      datasethooks.push(element);
     } 
     else
     {
      addtionalhooks.push(element);
     }
    });
}

// seperate known views from other views
// known view - resultMessages
function seperateViews(views)
{
  views.forEach(element => {
     if(element.name == "messages")
     {
      messageView = element;
     } 
     else
     {
      additionalViews.push(element);
     }
  });

  console.log(messageView)
  console.log(additionalViews)
}

// debug infos 
let visible=false;

</script>

{#if !model} <!--if the model == false, access denied-->
  <Spinner color="primary" size="sm" type ="grow" text-center />
{:else}  <!-- load page -->

<div in:fade={{ delay: 500 }}>

<!-- Header -->
<Header {id} {version} {title} />

<div class="content">

<div id="dataContainer" >

  <!-- Data Module Hooks -->
  <Data bind:hooks= {datasethooks} {id} {version}/>

</div> 

<div class="top">
<div id="additonalContainer">

  <Hooks bind:hooks= {addtionalhooks} {id} {version} />

</div>
<!-- ResultMessageView -->
<!-- <Message bind:view={messageView} {id} {version}/> -->

</div>
</div>

<Debug {hooks}/>

</div>

{/if}



<style>

.content{
  margin:40px 0; 
 }

#additonalContainer
{
  background-color: var(--bg-grey);
  padding-top: 3rem;
  padding-bottom: 1rem;
  margin-bottom: 1rem;
}

.top {
  box-shadow: 0px -16px 5px -3px #a1a1a1;
  height: auto;
  width: 100%;

}

</style>



