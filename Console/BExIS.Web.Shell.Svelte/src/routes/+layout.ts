export const prerender = true;
export const ssr = false;
export const trailingSlash = 'always';

import { setApiConfig } from '@bexis2/bexis2-core-ui';

/** @type {import('./$types').PageLoad} */
export async function load() {
	if (import.meta.env.DEV) {
		// setApiConfig('https://dev.bexis2.uni-jena.de/', 'Admin', '123456');
		setApiConfig('http://localhost:44345/', 'admin', '123456');
	}

	return {};
}

