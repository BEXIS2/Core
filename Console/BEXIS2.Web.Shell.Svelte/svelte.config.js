// import adapter from '@sveltejs/adapter-auto';
import adapter from '@sveltejs/adapter-static';

import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	// Consult https://kit.svelte.dev/docs/integrations#preprocessors
	// for more information about preprocessors
	preprocess: vitePreprocess(),

	kit: {
		adapter: adapter({
			pages: '../BEXIS2.Web.Shell/wwwroot/svelte', // ../BExIS.Modules.Dcm.UI/wwwroot/svelte
			assets: '../BEXIS2.Web.Shell/wwwroot/svelte', // ../BExIS.Modules.Dcm.UI/wwwroot/svelte
			fallback: null,
			precompress: false,
			preprocess: true,
			strict: false
			
		}),
		alias: {
			$models: './src/models',
			$services: './src/services'
		}
	}
};

export default config;
