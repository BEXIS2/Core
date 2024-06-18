import { setApiConfig } from '@bexis2/bexis2-core-ui';

/** @type {import('./$types').PageLoad} */
export async function load ({fetch}) {
	if (import.meta.env.DEV) {
		console.log('dev');
		setApiConfig('https://localhost:44345', 'admin', '123456');
	 console.log("layout")
	}

	return {};
}

export const prerender = true;
export const ssr = false;
export const trailingSlash = 'always';