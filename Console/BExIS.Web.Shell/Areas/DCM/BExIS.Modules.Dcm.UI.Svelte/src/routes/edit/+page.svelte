<script lang="ts">

 import { onMount,  } from 'svelte'; 
 import { fade  } from 'svelte/transition'; 
 import {Spinner, Container} from 'sveltestrap';
 
 import { getEdit }  from './services'
 import { setApiConfig }  from '@bexis2/bexis2-core-ui/src/lib/index'
 
 import Header from './Header.svelte'
 import Data from './Data.svelte'
 import Hooks from './Hooks.svelte'
 import Message from './MessagesContainer.svelte'
 import Debug from '../../lib/components/Debug.svelte'
 
 import { latestFileUploadDate, latestDataDescriptionDate, hooksStatus } from './stores';
 
 import type { EditModel, HookModel, ViewModel} from './types';
 import { isEditModel} from './types';
 import Hook from '../../lib/components/Hook.svelte';

 // load attributes from div
 let container = document.getElementById('edit');

 let id = Number(container?.getAttribute("dataset"));

 let version:number;
 $:version = Number(container?.getAttribute("version"));

 let title = "";
 let model:EditModel = {
  id: 0,
  versionId: 0,
  version: 0,
  title: "",
  hooks: [],
  views: []
 };

 let hookStatusList:{[key:string]:number};

// hooks
 let hooks:HookModel[];
 $:hooks=[];

 let datasethooks:HookModel[];
 $:datasethooks= [];

 let addtionalhooks:HookModel[];
 $:addtionalhooks= [];

 // views
 let views:ViewModel[];
 $:views=[];

 let additionalViews:ViewModel[];
 $:additionalViews=[];

 let messageView:ViewModel;
 $:messageView;




 onMount(async () => {

   id=3;

   console.log("start edit");
   setApiConfig("https://localhost:44345","davidschoene","123456");
   load();

 })


 latestFileUploadDate.subscribe(e =>{
  reload();
})

latestDataDescriptionDate.subscribe(e =>{
  reload();
})

async function load()
{
  
  // load model froms server
  model = await getEdit(id);


  if(isEditModel(model))
  {

   console.log("editmodel", model);

   hooks = model.hooks;
   views = model.views;
   title = model.title;
   version = model.version

   // update store
   updateStatus(hooks);

   // sam/ui/scripts/userpermission.js
   // load svelte

   // seperate dcm hooks from other hooks
   seperateHooks(hooks);

   // get resultView
   seperateViews(views);
  }
}

async function reload()
{
  console.log("reload");

  setApiConfig("https://localhost:44345","davidschoene","123456");
  // load model froms server
  model = await getEdit(id);
  hooks = model.hooks

  // there is a need for a time delay to update the hook status
  // if not exit, the first run faild because the hooks are not  
  setTimeout(async () => {
				// update store
        updateStatus(model.hooks);
			}, 1000) /* <--- If this is enough greater than transition, it doesn't happen... */

}


// seperate dcm hooks from other hooks
// known hooks - metadata, fileupload, validation
function seperateHooks(hooks:HookModel[])
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

async function updateStatus(_hooks)
{
  let dic:{[key:string]:number}={[""]:0};

  _hooks.forEach(hook => {
    dic[hook.name] = hook.status
  });

  console.log("before",$hooksStatus);

  hooksStatus.set(dic);

  console.log("updateStatus",$hooksStatus);
  console.log("length",$hooksStatus.length);

  hookStatusList = $hooksStatus;
}

// debug infos 
let visible=false;

</script>
 


{#if model && hookStatusList} <!--if the model == true, load page-->

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

<Debug bind:hooks = {hooks}/>

</div>
{:else}  <!-- access denied -->
  <Spinner color="primary" size="sm" type ="grow"/>
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