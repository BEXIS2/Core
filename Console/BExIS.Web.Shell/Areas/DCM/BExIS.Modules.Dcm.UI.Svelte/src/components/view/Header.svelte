<script>
 import {Row, Col, Container} from 'sveltestrap';

 export let id;
 export let version; 
 export let title = "";

 </script>
 <!-- Header -->
<Container fluid>
 <Row>
  <Col class="col-sm-12">
    <h1>{title}</h1>
   </Col>
  </Row>
  <Row>
  <Col class="col-sm-1">
   Dataset Id: {id}
  </Col>
  <Col class="col-sm-1">
   Version: {version}
  </Col>
  <Col class="col-sm-5">
    <b>version selection</b>
  </Col>
  <Col class="col-sm-5 text-end">
   <b>download</b>
   
  </Col>
 </Row>
</Container>

 
 
 
 