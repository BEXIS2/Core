<script>
  import {FormGroup, Label} from 'sveltestrap';
  import Select from 'svelte-select/Select.svelte';
  import { onMount } from 'svelte'; 
 
  export let source;
  export let target;
  export let title;
  export let optionIdentifier ="value";
  export let labelIdentifier = "label";
  export let isMulti = true;
  export let isComplex = false;
  export let isTargetComplex = false;
  let isLoaded = false;
 
  $:value = null;
  $:updateTarget(value);
 
  function updateTarget(selection){
    //diffrent cases
    console.log("------");
 
    //a) source is complex model is simple
    if(isComplex && !isTargetComplex && isLoaded)
    {
      target = [];
      for (let i in selection) {
          let item = selection[i];
          target.push(item.key)
        }
    }

    if(!isComplex && !isTargetComplex && isLoaded)
    {
      target = [];
      for (let i in selection) {
          target.push(selection[i].value)
        }
    }
    console.log("selection "+title,selection);
    console.log("target "+title,target);
  }
 
  onMount(async () => {

    //a) source is complex model is simple
    if(isComplex && !isTargetComplex)
    {
      let items = [];
      // event.detail will be null unless isMulti is true and user has removed a single item
      for (let i in target) {
          let t = target[i];
          items.push(source.find(item=> item.key===t));
      }
 
      isLoaded = true;
      if(items.length>0){value = items;}
      console.log(value);
    }

    //b) simple liust and simple model 
    if(!isComplex && !isTargetComplex)
    {
      console.log("source", source);
      console.log("target",target);
      isLoaded = true;
      //set target only if its nit empty
      if(target!=null && target !== undefined && target != ""){
        value = target;
      }
    }
  })
 
 
 </script>
 
 <Label>{title}</Label>
 <FormGroup >
 <Select 
 items={source} 
 {optionIdentifier}
 {labelIdentifier}
 {isMulti}
 bind:value
 placeholder="-- Please select --"
 ></Select>
 </FormGroup>
 