<script>
 import {Row, Col,Button} from 'sveltestrap';

 import { host } from '@bexis2/bexis2-core-ui/src/lib/index'

 
 export let id;
 export let version;

 export let status;
 export let start;

 let isEnabled = setEnable(status);
 let url = host+start+"?id="+id+"&version="+version;
  
 function setEnable(status)
 {
   if(status == 0 ||status == 1 ) // disabled || access denied
   {
     return false;
   }
     
   return true; // every other status enable the hook
 }

function clickEditMetadata() {
 
 window.open(url, '_blank').focus();
}

function clickEditVisibility() {
 
}
 
function clickEditStatus() {
 
}

</script>

<Row>
  <Col  class="col-sm-2">
   <Button on:click={clickEditMetadata}>Edit</Button>
  </Col>
  <Col class="col-sm-2">
   <!--stats-->
   <span>Last Modfied</span>
  </Col>
</Row>
<br/>
<Row>
 <Col  class="col-sm-2">
  <b>Visibility</b> 
 </Col>
 <Col class="col-sm-2">
  <!--stats-->
  <span> not implemented</span>
 </Col>
 <Col class="col-sm-2">
  <!--stats-->
  <Button on:click={clickEditVisibility}>Edit</Button>

 </Col>
</Row>
<br/>
<Row>
 <Col  class="col-sm-2">
   <b>Status</b>
 </Col>
 <Col class="col-sm-2">
  <!--stats-->
  <span> not implemented</span>

 </Col>
 <Col class="col-sm-2">
  <!--stats-->
  <Button on:click={clickEditStatus}>Edit</Button>
 </Col>
</Row>