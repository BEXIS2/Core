<script>
  import HookContainer from '../HookContainer.svelte';
  import Hook from '../Hook.svelte';
  import { Spinner} from 'sveltestrap';


 export let id;
 export let version;
 export let hooks=[];
 
</script>

 {#if hooks} <!-- if hooks list is loaded render hooks -->
 {#each hooks as hook}

  <HookContainer displayName = {hook.displayName}>
    <div slot="view">
      <Hook {id} {version} {...hook} />
    </div>
  </HookContainer>
 {/each}
 
 {:else} <!-- while data is not loaded show a loading information -->
    <Spinner color="primary" size="sm" type ="grow" text-center />
 {/if}



