<script>

 import {Row,Col} from 'sveltestrap'
 
  export let pSize=6;

</script>

<Row>
 <Col xs={pSize}>
       <slot name="property"></slot>
 </Col>
 <Col>
      <div class="variable-displaypattern">
            <slot name="displaypattern"></slot>
      </div>
 </Col>
 <Col>
       <div class="variable-description">
         <slot name="description"></slot>
       </div>
 </Col>
</Row>



<style>

 .variable-description{
       padding-top: 30px;
       font-size: small;
 }
 
 </style>