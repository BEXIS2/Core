import { setApiConfig } from '@bexis2/bexis2-core-ui';

/** @type {import('./$types').PageLoad} */
export async function load({ fetch }) {
	if (import.meta.env.DEV) {
		// setApiConfig('http://mv-bexis.bioimbgle.uni-jena.de/', 'majoho', 'Mjh271181#');
		setApiConfig('https://rc.bexis2.uni-jena.de/', 'admin', '123456');
	}

	return {};
}

export const prerender = true;
export const ssr = false;
export const trailingSlash = 'always';
