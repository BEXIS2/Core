<script lang="ts">
  import List from '../../components/create/List.svelte'
  import Form from '../../components/create/Form.svelte'
  import { onMount } from 'svelte'; 
  import { fade } from 'svelte/transition'; 

  import { setApiConfig }  from '@bexis2/bexis2-core-ui/src/lib/index'

  import {Spinner,Row,Col, Collapse } from 'sveltestrap';

  import { getEntityTemplateList, getSystemKeys }  from '../../services/Caller'
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
 <Row>
  <Col>
   <List items={entitytemplates} on:select={handleSelect}/>
  </Col>
  <Col>
    <b>test</b>
   {#if selected && isOpen}

   <Collapse  {isOpen}>

    <div class="form-container" >
     <Form bind:id={selected.id} on:cancel={()=>isOpen=false} on:save={(e)=>onSaveHandler(e)} />
    </div>
   </Collapse> 
   {/if}
  </Col>
 </Row>
 
 {:else}
 <Spinner color="info" size="sm" type ="grow" text-center />
 {/if}
 
 </div>
 
 <style>
 
 .form-container
 {
   padding: 1em;
 }
 
 </style>