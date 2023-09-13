/** @type {import('tailwindcss').Config} */
module.exports = {
	darkMode: 'class',
	content: [
		'./src/**/*.{html,js,svelte,ts}',
		require('path').join(require.resolve('@skeletonlabs/skeleton'), '../**/*.{html,js,svelte,ts}'),
		require('path').join('node_modules/@bexis2/bexis2-core-ui', '../**/*.{html,js,svelte,ts}')
		// 'C:/Users/admin/source/repos/Bexis2/BEXIS2 - Core - Workspace/Core/Console/BExIS.Web.Shell/Areas/DCM/BExIS.Modules.Dcm.UI.Svelte/node_modules/@bexis2/bexis2-core-ui/src/**/*.{html,js,svelte,ts}'],
	],

	theme: {
		extend: {}
	},
	plugins: [
		require('@tailwindcss/forms'),
		require('@tailwindcss/typography'),
		...require('@skeletonlabs/skeleton/tailwind/skeleton.cjs')({ intellisense: false })
	]
};
