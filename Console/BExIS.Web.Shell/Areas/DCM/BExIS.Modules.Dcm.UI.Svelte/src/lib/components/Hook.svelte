<!-- 
  Disabled = 0, //Function behind the hook is deactivated, but shown       
  AccessDenied = 1, // User has no access to the function behind the hook
  Open = 2, // Function behind the hook is ready for use
  Ready = 3, // Function behind the hook was successfully completed
  Exist = 4 // Data behind the hook exists
  InActive = 5 // Data behind the hook exists
-->

<script>
export let status=0;
export let displayName="";
export let description="";

const isEnabled = setEnable(status);
const active = setActive(status);

console.log(active);

// let url = hosturl+start+"?id="+id+"&version="+version;

function setEnable(status)
{
  if(status == 0 ||status == 1 ) // disabled || access denied
  {
    return false;
  }
    
  return true; // every other status enable the hook
}

function setActive(status)
{
  if(status == 0 ||status == 1 || status==5) // disabled || access denied || inactive
  {
    return false;
  }
    
  return true; // every other status enable the hook
}

</script>

{#if isEnabled}
  
  <div class="mb-3 hook-status-{status} hook" class:inactive={!active} title="{description}" on:click>
    {displayName.toUpperCase()}
  </div> 

{/if}

<!-- 
<style>


.hook{
  padding:20px;
  border-radius: 3px;
  position: relative;
  display: flex;
  flex-direction: column;
  min-width: 0;
  word-wrap: break-word;
  background-color: #fff;
  background-clip: border-box;
  border: 1px solid rgba(0,0,0,.125);
  border-radius: 0.25rem; 
  background-color: whitesmoke;
  box-shadow:  0 2px 2px 0 rgba(53, 38, 38, 0.1), 0 2px 4px 0 rgba(0, 0, 0, 0.19);
  opacity: 1;
  cursor: pointer;
}

.hook:hover{
  box-shadow: 0 2px 8px 0 rgba(0, 0, 0, 0.2);
  
}

.inactive{
  opacity: 0.5;
  cursor: no-drop;
} 


.hook-status-0 /* disabled */
{
 /* background-color: grey; */
 border-left: gray 6px solid;
}

.hook-status-1 /* access denied */
{
 /* background-color: red; */

 border-left: red 6px solid;
}

.hook-status-2  /* open */
{
  /* background-color: green; */
 border-left: green 6px solid;
}

.hook-status-3 /* ready */
{
 /* background-color: yellow; */
 border-left: yellow 6px solid;

}

.hook-status-4 /* exist */
{

 /* background-color: brown; */
 border-left: brown 6px solid;

}

.hook-status-5 /* inactive */
{

 /* background-color: whitesmoke; */
 border-left: black 6px solid;
}

</style> -->