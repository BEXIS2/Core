<script>
 import {Row, Col, Button, Input} from 'sveltestrap';

 export let id;
 export let version; 
 export let title = "";

 </script>
 <!-- Header -->
 <Row>
  <Col class="col-sm-2">
   <div class="title-container">
    <b>Title</b>
  </div>
   </Col>
  <Col class="col-sm-4">
   <Input value="{title}" />
  </Col>
  <Col class="col-sm-1 text-end">

  </Col>
  <Col class="col-sm-2 text-end">

  </Col>
 </Row>


 <style>
  .title-container
  {
    padding: 0 2rem 1rem 1rem;
  }
 </style>
 
 
 
 