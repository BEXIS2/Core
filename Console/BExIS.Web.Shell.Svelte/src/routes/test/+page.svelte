<script type="ts">
	import { SlideToggle } from '@skeletonlabs/skeleton';

	$: toggle = false;
</script>

<h1>Test with layout</h1>

<SlideToggle name="david" bind:checked={toggle}></SlideToggle>
<br />

<b>toggle : {toggle}</b>
