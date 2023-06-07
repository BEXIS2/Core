<script>
	// The ordering of these imports is critical to your app working properly
	//import '@skeletonlabs/skeleton/themes/theme-skeleton.css';
	// import '@bexis2/bexis2-core-ui/dist/css/themes/theme-bexis2.css';
	import '../../node_modules/@bexis2/bexis2-core-ui/dist/css/themes/theme-bexis2.css';


	// If you have source.organizeImports set to true in VSCode, then it will auto change this ordering
	import '@skeletonlabs/skeleton/styles/skeleton.css';

 // load generell skeleton changes for bexis2 
	// import "@bexis2/bexis2-core-ui/static/test.css";
	//Most of your app wide CSS should be put in this file
	import '../app.postcss';

 import { onMount,  } from 'svelte'; 
	import { dev } from '$app/environment'
 import { setApiConfig }  from '@bexis2/bexis2-core-ui'

	onMount(async () => {
	//setup api
	if(dev)
	{
				 console.log("layout api setup")
					setApiConfig("https://localhost:44345","davidschoene","123456");
	}

})

</script>



<slot></slot>