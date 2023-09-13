<script lang="ts">
    import type { PageData } from './$types';

    import { onMount } from 'svelte';
    import { setApiConfig }  from '@bexis2/bexis2-core-ui';
    import { getMenuItems}  from '../../services/MenuDataCaller';
    
    export let data: PageData;

    let menu= [];

    onMount(async () => {
   console.log("start entity template");
   setApiConfig("https://localhost:44345","testelli2","1234567");
 
   menu = await getMenuItems();
   
   console.log("menu", menu);

  
 })


</script>