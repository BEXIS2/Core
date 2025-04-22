/** @type {import('tailwindcss').Config} */

// @ts-check
import { join } from 'path';

// 1. Import the Skeleton plugin
import { skeleton } from '@skeletonlabs/tw-plugin';

// add custom theme
import { bexis2theme } from './node_modules/@bexis2/bexis2-core-ui/dist/themes/theme-bexis2';

module.exports = {
	darkMode: 'class',
	content: [
		'./src/**/*.{html,js,svelte,ts}',
		require('path').join(require.resolve('@skeletonlabs/skeleton'), '../**/*.{html,js,svelte,ts}'),
		require('path').join('node_modules/@bexis2/bexis2-core-ui', '../**/*.{html,js,svelte,ts}')
		// 'C:/Users/admin/source/repos/Bexis2/BEXIS2 - Core - Workspace/Core/Console/BExIS.Web.Shell/Areas/DCM/BExIS.Modules.Dcm.UI.Svelte/node_modules/@bexis2/bexis2-core-ui/src/**/*.{html,js,svelte,ts}'],
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
				// Register each theme within this array:
				custom: [bexis2theme]
			}
		})
	]
};
