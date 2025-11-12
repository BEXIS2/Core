<script>
	import { Page } from '@bexis2/bexis2-core-ui';

	// hide context menu
	window.addEventListener(
		'contextmenu',
		function (e) {
			// do something here...
			e.preventDefault();
		},
		false
	);
</script>

<Page title="Data Collection Module">
	<div class="grid grid-flow-col gap-10">
		<a class="btn variant-filled-secondary" href="./entitytemplates">entitytemplate</a>
		<a class="btn variant-filled-secondary" href="./create">create</a>
		<a class="btn variant-filled-secondary" href="./edit">edit</a>
		<a class="btn variant-filled-secondary" href="./view">view</a>
		<a class="btn variant-filled-secondary" href="./metadata">metadata</a>
	</div>
</Page>
