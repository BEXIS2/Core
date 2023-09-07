import {setApiConfig} from '@bexis2/bexis2-core-ui';


/** @type {import('./$types').PageLoad} */
export async function load() {

	if (import.meta.env.DEV) {
		setApiConfig('https://dev.bexis2.uni-jena.de/', 'admin', '123456');
	}

	return {};
}