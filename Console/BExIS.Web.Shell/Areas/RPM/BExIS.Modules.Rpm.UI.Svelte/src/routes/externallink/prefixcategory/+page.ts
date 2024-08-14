import { host, setApiConfig } from '@bexis2/bexis2-core-ui';

/** @type {import('./$types').PageLoad} */
export function load() {
	if (import.meta.env.DEV) {
		console.log('dev');
		setApiConfig('https://rc.bexis2.uni-jena.de/', 'admin', '123456');
	}

	console.log(host);

	return {};
}
