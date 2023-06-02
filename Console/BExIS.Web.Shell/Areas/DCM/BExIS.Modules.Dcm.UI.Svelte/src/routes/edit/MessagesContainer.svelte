<script>
import {Drawer} from '@skeletonlabs/skeleton'
import View from '$lib/components/View.svelte'

export let id;
export let version;
export let view;

let open = false;
const toggle = () => (open = !open);

</script>
<button class="btn bg-warning-500" on:click={toggle}>Messages</button>

<!-- <Drawer isOpen={open} {toggle} header="{view.displayName}" placement="end">
  <View {id} {version} {...view} /> 
</Drawer> -->
