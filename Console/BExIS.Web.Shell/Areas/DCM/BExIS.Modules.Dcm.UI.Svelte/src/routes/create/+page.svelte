<script lang="ts">
 import List from './List.svelte'
 import Form from './Form.svelte'
 import { onMount } from 'svelte'; 
 import { fade } from 'svelte/transition'; 

 import { setApiConfig, Spinner }  from '@bexis2/bexis2-core-ui'
 import { getEntityTemplateList, getSystemKeys }  from '../../services/EntityTemplateCaller'
 import { goTo }  from '../../services/BaseCaller'

 import type {EntityTemplateModel } from '../../models/EntityTemplate'


let entitytemplate:EntityTemplateModel;

$:entitytemplates=[];
$:systemkeys= [];
$:selected = entitytemplate;

onMount(async () => {
 setApiConfig("https://localhost:44345","davidschoene","123456");
 entitytemplates = await getEntityTemplateList();
 systemkeys = await getSystemKeys();
})



$:isOpen = false;

function handleSelect(e)
{
 console.log("on select");
//remove form from dom
isOpen = false;

// reopen form with new object
setTimeout(async () => {
  let index = e.detail;
  selected = entitytemplates[index];
  console.log(selected);
  isOpen = true;

  console.log("on isOpen",isOpen);
 },500)
}

function onSaveHandler(e)
{
 //e.detail == id of teh new created dataset
 goTo('/dcm/edit?id='+e.detail);

}

</script>


<div in:fade={{ delay: 500 }} out:fade={{ delay: 500 }}>
{#if entitytemplates}
<div class="w-full grid grid-cols-1 md:grid-cols-2 gap-1 p-5">
  <div>
    <List items={entitytemplates} on:select={handleSelect}/>
  </div>
 <div>
  {#if selected && isOpen}
    <Form bind:id={selected.id} on:cancel={()=>isOpen=false} on:save={(e)=>onSaveHandler(e)} />
  {/if}
 </div>
</div>

{:else}
<Spinner />
{/if}

</div>
