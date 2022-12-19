<script>

import { Button, Offcanvas } from 'sveltestrap';
import View from '../../lib/components/View.svelte'

export let id;
export let version;
export let view;

let open = false;
const toggle = () => (open = !open);

</script>
<Button color="danger" on:click={toggle}>Messages</Button>

<Offcanvas isOpen={open} {toggle} header="{view.displayName}" placement="end">
  <View {id} {version} {...view} /> 
</Offcanvas>
