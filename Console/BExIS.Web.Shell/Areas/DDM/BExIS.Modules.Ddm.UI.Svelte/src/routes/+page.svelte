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

<Page title="System Administration">
	<div class="grid grid-flow-col gap-10">
		<a class="btn variant-filled-secondary" href="./search">search</a>
	
	</div>
</Page>
