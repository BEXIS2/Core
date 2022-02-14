<script>
 import MenuBar from './MenuBar.svelte'
 import SettingsBar from './SettingsBar.svelte'
 import { onMount } from 'svelte'; 
 import { Spinner } from 'sveltestrap';
 import {
    Collapse,
    Navbar,
    NavbarToggler,
    NavbarBrand,
    Nav
  } from 'sveltestrap';

import {hosturl} from '../stores/store.js'

 $:menu="";

 onMount(async () => {

   // load menu froms server
    const url = hosturl+'/menu';
		const res = await fetch(url);
		menu = await res.json();

})

let isOpen = false;
 const toggle = () => (isOpen = !isOpen);
</script>

<Navbar id="bexis2-nav-bar" light expand="md" class="fixed-top" >
  <NavbarBrand href="/" class="bexis2-nav-brand">
    {#if menu.Logo}
      <img src='data:{menu.Logo.Mime};charset=utf-8;base64, {menu.Logo.Data}' alt='{menu.Logo.Name}' style='height:40px;' />
    {/if}
    
  </NavbarBrand>
  <NavbarToggler id="toggle-icon" on:click={toggle} class="me-2" />
  <Collapse navbar {isOpen} expand="md" >
      {#if menu}
        <Nav navbar>
          {#if menu.MenuBar}
            <MenuBar bar={menu.MenuBar} />
          {/if}
        </Nav>

        <Nav navbar class="ms-auto">
          <!--AccountBar-->
          {#if menu.AccountBar}
          <MenuBar bar={menu.AccountBar} menuStyle="right: 0;position: absolute;"/>
          {/if}

          <!--LaunchBar-->
          {#if menu.LaunchBar}
          <MenuBar bar={menu.LaunchBar} menuStyle="right:0; position:absolute;"/>
          {/if}

          <!--Extended-->
          {#if menu.Extended}
          <MenuBar  bar={menu.Extended} menuStyle="right:0; position:absolute;"/>
          {/if}

          <!--Settings-->
          {#if menu.Settings}
          <SettingsBar bar={menu.Settings} menuStyle="right:0; position:absolute;"/>
          {/if}

          
        </Nav>
        {:else} <!-- while data is not loaded show a loading information -->
          <Spinner color="primary" size="sm" type ="grow" text-center />
        {/if}
  </Collapse>
</Navbar>


<style>
  :global(.bexis2-nav-brand)
  {
    padding-right: 16px;
  }

  :global(#bexis2-nav-bar) {
      margin-top: 0;
      margin-Bottom: 20px;
      padding:0;
  }

  :global(#bexis2-nav-bar>.container-fluid) {
    background-color: var(--bg-color-level-5);
  }

  :global(#bexis2-nav-bar a) {
    /* color: #777; */
    color:var(--nav-text-color);
    text-decoration: none;
    margin:0;

  }

  :global(#bexis2-nav-bar .bexis2-nav-item) {
    padding-left:10px;
    padding-right:10px;
  }


  :global(#bexis2-nav-bar a:hover){
    color: var(--nav-hover);
  }
  
  :global(#bexis2-nav-bar a:focus){
    color: var(--nav-hover);
    background-color: #e7e7e7;
  } 

  /* :global(#bexis2-nav-bar a:visited){
    color: var(--nav-hover);
    background-color: #e7e7e7;
  } */

  :global(#bexis2-nav-bar button ){
    border-radius: 0;
  }

  :global(#bexis2-nav-bar > div > div > ul > li.dropdown.show.nav-item > div) {
     padding-top:4px;
     padding-bottom:4px;
     margin:0;
     border-radius: 0;
  }

  :global(#bexis2-nav-bar > div > div > ul > li.dropdown.show.nav-item > div > button)
  {
      margin-left:0;
      padding-top: 0;
      padding-bottom: 0;
  }

  :global(#bexis2-nav-bar > div > div > ul > li.dropdown.show.nav-item > div > button > a) {
      font-size: 14px;
  }

</style>