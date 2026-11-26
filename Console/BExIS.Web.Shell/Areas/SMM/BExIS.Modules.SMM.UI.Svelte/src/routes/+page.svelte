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

<Page title="Species Mapping">
	<div class="grid grid-flow-col gap-10">
	<a class="btn variant-filled-secondary" href="./datasetsoverview">Datasets Overview</a>
	<a class="btn variant-filled-secondary" href="./species">Species</a>
	<a class="btn variant-filled-secondary" href="./headermapping">Header Mappings</a>
	<a class="btn variant-filled-secondary" href="./tailorview">Tailor Result</a>
	<a class="btn variant-filled-secondary" href="./progressoverview">Progress Overview</a>
	
	</div>
</Page>
