<script>
	// The ordering of these imports is critical to your app working properly

	// import '../../node_modules/@bexis2/bexis2-core-ui/dist/css/themes/theme-bexis2.css';
	// If you have source.organizeImports set to true in VSCode, then it will auto change this ordering

	// load generell skeleton changes for bexis2
	//import '@bexis2/bexis2-core-ui/css/core.ui.postcss';
	//Most of your app wide CSS should be put in this file
	import '../app.postcss';

	import { computePosition, autoUpdate, offset, shift, flip, arrow } from '@floating-ui/dom';
	import { storePopup } from '@skeletonlabs/skeleton';
	storePopup.set({ computePosition, autoUpdate, offset, shift, flip, arrow });

	import { initializeStores } from '@skeletonlabs/skeleton';
	initializeStores();
</script>

<slot />
