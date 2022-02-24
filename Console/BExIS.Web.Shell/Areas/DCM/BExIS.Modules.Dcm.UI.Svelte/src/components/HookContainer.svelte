<script>
 import {Row, Col, Alert} from 'sveltestrap';
 
 import Fa from 'svelte-fa/src/fa.svelte'
 import { faAngleRight } from '@fortawesome/free-solid-svg-icons'

 export let displayName;

 $:error = [];
 $:success = null;
 $:warnings = [];

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
</script>

<div class="hook-container" >
 <Row> 
  <Col class="col-sm-2">
    <div class="title-container">
      <b><Fa icon={faAngleRight} /> {displayName}</b>
    </div>
  </Col>
  <Col class="col-sm-6">
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
  <Col class="col-sm-4"> 
    <slot name="settings"> settings area</slot>
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