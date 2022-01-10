<script>
 import {Row, Col} from 'sveltestrap';
 
 import Fa from 'svelte-fa/src/fa.svelte'
 import { faAngleRight } from '@fortawesome/free-solid-svg-icons'

 export let displayName;

</script>

<div class="hook-container">
 <Row> 
  <Col class="col-sm-2">
    <div class="title-container">
      <b><Fa icon={faAngleRight} /> {displayName}</b>
    </div>
  </Col>
  <Col class="col-sm-6">
    <slot name="view"> render view</slot>
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