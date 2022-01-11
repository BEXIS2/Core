<script>
import {
    Nav,
    NavItem,
    NavLink,
    Dropdown,
    DropdownToggle,
    DropdownMenu,
    DropdownItem
  } from 'sveltestrap';

export let bar
export let menuStyle;


</script>

<!--MenuBar-->
{#if bar}
{#each bar as menubarItem}
  {#if menubarItem.Items.length<1}
    <NavItem>
      <NavLink href="{menubarItem.Url}" class="bexis2-nav-item">{menubarItem.Title}</NavLink>
    </NavItem>
  {:else}
    <Dropdown nav inNavbar class="bexis2-nav-item">
      <DropdownToggle nav caret>{menubarItem.Title}</DropdownToggle>
      <DropdownMenu end style="{menuStyle}">
        {#each menubarItem.Items as item}
          <DropdownItem><a href="{item.Url}" target={item.Target}>{item.Title}</DropdownItem>
        {/each}
      </DropdownMenu>
    </Dropdown>
  {/if}
{/each}
{/if}
