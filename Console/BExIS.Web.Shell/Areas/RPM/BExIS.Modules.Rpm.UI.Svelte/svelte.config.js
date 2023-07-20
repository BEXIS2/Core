// import adapter from '@sveltejs/adapter-auto';
import adapter from '@sveltejs/adapter-static';

import { vitePreprocess } from '@sveltejs/kit/vite';
import { build } from 'vite';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	// Consult https://kit.svelte.dev/docs/integrations#preprocessors
	// for more information about preprocessors
	preprocess: vitePreprocess(),

	kit: {
		adapter: adapter({
			pages: '../BExIS.Modules.Rpm.UI/Scripts/svelte',
			assets: '../BExIS.Modules.Rpm.UI/Scripts/svelte',
			fallback: null,
			precompress: true,
			preprocess: true,
			strict: false
		}),
		paths: {
			relative: true,
			base: process.env.NODE_ENV === 'production' ? '/rpm' : '' // add module id here e.g. /dcm
		}
	}
};

export default config;
