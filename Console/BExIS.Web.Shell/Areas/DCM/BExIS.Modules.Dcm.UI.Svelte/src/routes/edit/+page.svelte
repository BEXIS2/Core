<script lang="ts">

 import { onMount,  } from 'svelte'; 
 import { fade  } from 'svelte/transition'; 

 import { getEdit }  from './services'
 import { setApiConfig, Spinner, Page }  from '@bexis2/bexis2-core-ui'
 
 import Header from './Header.svelte'
 import Data from './Data.svelte'
 import Hooks from './Hooks.svelte'
 import Message from './MessagesContainer.svelte'
 import Debug from '$lib/components/Debug.svelte'
 
 import { latestFileUploadDate, latestDataDescriptionDate, hooksStatus } from './stores';
 
 import type { EditModel, HookModel, ViewModel} from './types';
 import { isEditModel} from './types';
 import Hook from '$lib/components/Hook.svelte';


 // load attributes from div
 let container ;
 let id:number = 0;

 let version:number;
 

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
$:hookStatusList
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

   // get data from parent
   container = document.getElementById('edit');
   id = Number(container?.getAttribute("dataset"));
   version = Number(container?.getAttribute("version"));

   console.log("start edit",id,version);

   // load
   await load();
 })


 latestFileUploadDate.subscribe(e =>{
  reload();
})

latestDataDescriptionDate.subscribe(e =>{
  reload();
})

async function load()
{
  console.log("load");
  // load model froms server
  model = await getEdit(id);
  console.log("edit after wait", model);


  if(isEditModel(model))
  {

   console.log("edit after check", model);

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
   console.log("test")
  }

  console.log("model and hooks", model,hookStatusList);
}

async function reload()
{
  console.log("reload");

  // setApiConfig("https://localhost:44345","davidschoene","123456");
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

  // console.log(messageView)
  // console.log(additionalViews)
}

function updateStatus(_hooks)
{
  let dic:{[key:string]:number}={[""]:0};

  if(_hooks!==undefined)
  {
    _hooks.forEach(hook => {
      dic[hook.name] = hook.status
    });

    console.log("cuurent Hookstatus",$hooksStatus);
    hooksStatus.set(dic);
    console.log("update Hookstatus",$hooksStatus);
  }

  hookStatusList = $hooksStatus;
}

// debug infos 
let visible=false;

</script>

<Page title="Edit dataset: {model.title}">


{#if model && hookStatusList} <!--if the model == true, load page-->
<!-- Header -->
<Header {id} {version} {title} />

<!-- Data Module Hooks -->
<Data bind:hooks= {datasethooks} {id} {version}/>

<hr class="!border-dashed" />

<Hooks bind:hooks= {addtionalhooks} {id} {version} />

{:else}  <!-- access denied -->
<div class="h-screen">
  <Spinner textCss="text-surface-800" label="loading edit page" position="center"/>
</div>
{/if}

</Page>
