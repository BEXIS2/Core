<script lang="ts">

import { SlideToggle } from '@skeletonlabs/skeleton';

import type {FileInfo} from '@bexis2/bexis2-core-ui'
import {Api} from '@bexis2/bexis2-core-ui'
import { setApiConfig }  from '@bexis2/bexis2-core-ui'

import { onMount } from 'svelte'; 

onMount(async () => {
   console.log("start entity template");
  //  setApiConfig("https://localhost:44345","davidschoene","123456");


   try {
    const response = await Api.get('/dcm/entitytemplates/Load');
    console.log("data",response.data);
  } catch (error) {
    console.error(error);
  }
})

$:toggle = false;

</script>
<h1>Test with layout</h1>


<SlideToggle name="david" bind:checked={toggle}></SlideToggle>
<br/>

<b>toggle : {toggle}</b>

