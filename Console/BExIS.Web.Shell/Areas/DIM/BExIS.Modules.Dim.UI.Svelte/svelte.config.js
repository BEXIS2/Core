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
			pages: '../BExIS.Modules.Dim.UI/Scripts/svelte', // ../BExIS.Modules.Dcm.UI/Scripts/svelte
			assets: '../BExIS.Modules.Dim.UI/Scripts/svelte', // ../BExIS.Modules.Dcm.UI/Scripts/svelte
			fallback: null,
			precompress: true,
			preprocess: true,
			strict: false
		}),
		paths: {
			relative: true,
			base: process.env.NODE_ENV === 'production' ? '/dim' : '' // add module id here,
		},

		alias: {
			$models: './src/models',
			$services: './src/services'
		}
	}
};

export default config;
