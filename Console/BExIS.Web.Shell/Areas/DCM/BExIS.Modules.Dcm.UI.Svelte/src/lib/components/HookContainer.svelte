<script>
 import {Row, Col, Alert} from 'sveltestrap';
 import {onMount} from 'svelte';
 
 import Fa from 'svelte-fa/src/fa.svelte'
 import { faAngleRight } from '@fortawesome/free-solid-svg-icons'

import { hooksStatus } from '../../routes/edit/stores';


 export let name; 
 export let displayName;
 export let content=9;
 export let visible = true;
 export let status=0;

 $:error = [];
 $:success = null;
 $:warnings = [];

 $:active = false;

onMount(async () => {

  //active = setActive(status);
  hooksStatus.subscribe(h=>{
    if(h[name]!=undefined)
    {
      active = setActive(h[name]);
    }
  })
})


function errorHandler (e){ 
  console.log("handle errors here")
  console.log(e.detail.messages)
  error = e.detail.messages;
}

function successHandler (e){ 
  console.log("handle success here")
  console.log( e.detail.text)
  success = e.detail.text;

}

function warningHandler (e){ 
  console.log("handle warnings here");
  console.log( e.detail.text)
}

// visibility
function setActive(status)
{

  if(status == 0 ||status == 1 || status==5) // disabled || access denied || inactive
  {
    return false;
  }
    
  return true; // every other status enable the hook
}

</script>

<!-- {$hooksStatus[name]}
{status}
{active} -->

{#if visible && active}
<div class="hook-container" >
 <Row> 
 
  <Col xs="{2}">
    <div class="title-container">
      <b><Fa icon={faAngleRight} /> {displayName}</b> 
    </div>
  </Col>

  <Col xs={{ size: content, order: 2}}>

    {#if error}
      {#each error as item}
        <Alert color="danger" dismissible>{item}</Alert>
      {/each}
    {/if}
    {#if success}
      <Alert color="success" dismissible>{success}</Alert>
    {/if}

    <slot name="view" {errorHandler} {successHandler} {warningHandler}> render view</slot>
  </Col>
 </Row>
</div>

<style>
 .title-container
 {
   padding: 0 2rem 1rem 1rem;
 }

 .hook-container
 {
   padding: 1rem 0;
   border-bottom: 1px solid #eee;
 }

</style>
{/if}