<script>
 import { host } from '@bexis2/bexis2-core-ui'

 
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
 
 window.open(url, '_blank')?.focus();
}

function clickEditVisibility() {
 
}
 
function clickEditStatus() {
 
}

</script>

<div class="grid grid-cols-3 w-1/3 gap-3 content-center">
  
   <b class="self-center">Metadata</b> 

   <button class="btn bg-secondary-500 text-white w-1/2" on:click={clickEditMetadata}>Edit</button>
   <!--stats-->
   <div class="self-center"><span>Last Modfied</span></div>
   


  <b class="self-center">Visibility</b> 
  <!--stats-->
  <span class="self-center"> not implemented</span>
  <!--stats-->
  <button class="btn bg-secondary-500 text-white btn-sm w-1/2" on:click={clickEditVisibility}>Edit</button>



  <b class="self-center">Status</b>
  <!--stats-->
  <span class="self-center"> not implemented</span>
  <!--stats-->
  <button class="btn bg-secondary-500 text-white btn-sm w-1/2" on:click={clickEditStatus}>Edit</button>
 

</div>