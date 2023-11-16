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
		<a class="btn variant-filled-secondary" href="./datastructure">datastructure</a>
		<a class="btn variant-filled-secondary" href="./datastructure/create">create datastructure</a>
		<a class="btn variant-filled-secondary" href="./datastructure/edit">edit datastructure</a>
		<a class="btn variant-filled-secondary" href="./unit">unit</a>
		<a class="btn variant-filled-secondary" href="./dimension">dimension</a>
		<a class="btn variant-filled-secondary" href="./variabletemplate">variabletemplate</a>
		<a class="btn variant-filled-secondary" href="./meaning">meanings</a>
		<a class="btn variant-filled-secondary" href="./externallink">externallink</a>
		<a class="btn variant-filled-secondary" href="./externallink/prefixcategory">prefixcategory</a>
	</div>
</Page>