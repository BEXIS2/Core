import { setApiConfig } from '@bexis2/bexis2-core-ui';

/** @type {import('./$types').PageLoad} */
export async function load({ fetch }) {
	if (import.meta.env.DEV) {
		console.log('dev');
		setApiConfig('https://demo.bexis2.uni-jena.de', 'datamanager', 'datamanager');
		//setApiConfig('http://mv-bexis.bioimbgle.uni-jena.de/', 'felixistlit', '!"§$%&/()=');
		console.log('layout');
	}

	return {};
}

export const prerender = true;
export const ssr = false;
export const trailingSlash = 'always';
