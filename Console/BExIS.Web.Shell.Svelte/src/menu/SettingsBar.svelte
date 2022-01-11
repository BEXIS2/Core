<script>

 import Fa from 'svelte-fa/src/fa.svelte'
 import { faCog } from '@fortawesome/free-solid-svg-icons'

 import {
     Nav,
     NavItem,
     NavLink,
     Dropdown,
     DropdownToggle,
     DropdownMenu,
     DropdownItem
   } from 'sveltestrap';
 
 export let bar;
 export let menuStyle;

 let lastModule = "";
 let first = true;

 function isNewModule(module)
 {
    if(module === lastModule ){
      return false;
    }
    else
    {
      lastModule = module;
      if(first)
      {
        first = false;
        return false;
      }
      
      return true;
    }
 }


</script>
  <!--Settings-->
  {#if bar}

    <Dropdown nav inNavbar>
      <DropdownToggle nav><Fa icon={faCog} /></DropdownToggle>
      <DropdownMenu style="{menuStyle}">
        {#each bar as item}
          {#if isNewModule(item.Module) }<li><hr class="dropdown-divider"></li>{/if}
          <DropdownItem><a href="{item.Url}" target={item.Target}>{item.Title}</a></DropdownItem>
        {/each}
      </DropdownMenu>
    </Dropdown>
  {/if}

