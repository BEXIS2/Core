/** @type {import('tailwindcss').Config} */

// @ts-check
import { createRequire } from 'module';
import path from 'path';
import { skeleton } from '@skeletonlabs/tw-plugin';
import { bexis2theme } from './node_modules/@bexis2/bexis2-core-ui/dist/themes/theme-bexis2.js';

const require = createRequire(import.meta.url);

export default {
	darkMode: 'class',
	content: [
		'./src/**/*.{html,js,svelte,ts}',
		path.join(path.dirname(require.resolve('@skeletonlabs/skeleton')), '../**/*.{html,js,svelte,ts}'),
		path.join('node_modules/@bexis2/bexis2-core-ui', '../**/*.{html,js,svelte,ts}')
	],

	theme: {
		extend: {
			height: {
				'custom-height': '200px'
			}
		}
	},
	plugins: [
		require('@tailwindcss/forms'),
		require('@tailwindcss/typography'),
		skeleton({
			themes: {
				custom: [bexis2theme]
			}
		})
	]
};