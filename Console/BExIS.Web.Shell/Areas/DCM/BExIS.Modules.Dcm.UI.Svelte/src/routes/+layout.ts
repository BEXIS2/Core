import { setApiConfig } from '@bexis2/bexis2-core-ui';

/** @type {import('./$types').PageLoad} */
export async function load({ fetch }) {
	if (import.meta.env.DEV) {
		console.log('dev');
		setApiConfig('http://localhost:44345', 'majoho', 'woggel');
		console.log('layout');
	}

	return {};
}

export const prerender = true;
export const ssr = false;
export const trailingSlash = 'always';
