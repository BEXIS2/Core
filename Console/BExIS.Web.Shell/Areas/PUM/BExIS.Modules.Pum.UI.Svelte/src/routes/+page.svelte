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

<Page title="Research Planning Module">
	<div class="grid grid-cols-5 gap-10">
		<a class="btn variant-filled-secondary" href="./import">import</a>
	</div>
</Page>
