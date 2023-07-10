<script lang="ts">
	// The ordering of these imports is critical to your app working properly
	//import '@skeletonlabs/skeleton/themes/theme-skeleton.css';
	import '@bexis2/bexis2-core-ui/css/themes/theme-modern.css';
	// If you have source.organizeImports set to true in VSCode, then it will auto change this ordering
	import '@skeletonlabs/skeleton/styles/all.css';
	// load generell skeleton changes for bexis2
	import '@bexis2/bexis2-core-ui/css/core.ui.postcss';
	//Most of your app wide CSS should be put in this file
	import '../app.postcss';

	import { Modal } from '@skeletonlabs/skeleton';
</script>

<slot />
