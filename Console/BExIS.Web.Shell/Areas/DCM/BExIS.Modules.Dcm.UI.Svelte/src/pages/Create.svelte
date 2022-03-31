<script>
 import List from '../components/create/List.svelte'
 import Form from '../components/create/Form.svelte'
 import { onMount } from 'svelte'; 

 import { setApiConfig }  from '@bexis2/svelte-bexis2-core-ui'

 import {Spinner,Row,Col, Collapse } from 'sveltestrap';

 import { getEntityTemplateList, getSystemKeys }  from '../services/Caller'
 import { goTo }  from '../services/BaseCaller'

 $:entitytemplates= [];
 $:systemkeys= [];
 $:selected= null;

 onMount(async () => {
  setApiConfig("https://localhost:44345","davidschoene","123456");
  entitytemplates = await getEntityTemplateList();
  systemkeys = await getSystemKeys();
 })

$:isOpen = selected;

function handleSelect(e)
{
 //remove form from dom
 isOpen = false;

 // reopen form with new object
 setTimeout(async () => {
   let index = e.detail;
   selected = entitytemplates[index];
   console.log(selected);
   isOpen = true;
  },500)
}

function onSaveHandler(e)
{
  //e.detail == id of teh new created dataset
  goTo('/dcm/edit?id='+e.detail);

}

</script>

{#if entitytemplates}
<Row>
 <Col>
  <List items={entitytemplates} on:select={handleSelect}/>
 </Col>
 <Col>
  {#if selected}
  <Collapse {isOpen} >
   <Form id={selected.id} on:cancel={()=>isOpen=false} on:save={(e)=>onSaveHandler(e)} />
  </Collapse>
  {/if}
 </Col>
</Row>

{:else}
<Spinner color="info" size="sm" type ="grow" text-center />
{/if}