<script>
	// The ordering of these imports is critical to your app working properly
	//import '@skeletonlabs/skeleton/themes/theme-skeleton.css';
	// import '@bexis2/bexis2-core-ui/dist/css/themes/theme-bexis2.css';
	import '../../node_modules/@bexis2/bexis2-core-ui/dist/css/themes/theme-bexis2.css';

	import { computePosition, autoUpdate, offset, shift, flip, arrow } from '@floating-ui/dom';
	import { storePopup } from '@skeletonlabs/skeleton';
	storePopup.set({ computePosition, autoUpdate, offset, shift, flip, arrow });
	// If you have source.organizeImports set to true in VSCode, then it will auto change this ordering
	import '@skeletonlabs/skeleton/styles/all.css';
 // load generell skeleton changes for bexis2 
	// import "@bexis2/bexis2-core-ui/static/test.css";
	//Most of your app wide CSS should be put in this file
	import '../app.postcss';


</script>

<slot></slot>