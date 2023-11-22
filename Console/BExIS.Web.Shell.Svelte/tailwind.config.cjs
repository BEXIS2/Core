/** @type {import('tailwindcss').Config} */
// @ts-check
import { join } from 'path';

// 1. Import the Skeleton plugin
import { skeleton } from '@skeletonlabs/tw-plugin';

//add theme 
import { bexis2theme } from './node_modules/@bexis2/bexis2-core-ui/dist/themes/theme-bexis2';


module.exports = {
	darkMode: 'class',
	content: ['./src/**/*.{html,js,svelte,ts}', 
	require('path').join(require.resolve('@skeletonlabs/skeleton'), '../**/*.{html,js,svelte,ts}'),
	require('path').join('node_modules/@bexis2/bexis2-core-ui', '../**/*.{html,js,svelte,ts}')],
	theme: {
		extend: {},
	},
	plugins: [
		require('@tailwindcss/forms'),
		require('@tailwindcss/typography'),
		skeleton({
			themes: {
				custom: [ bexis2theme ]
			}
		})
	]
}