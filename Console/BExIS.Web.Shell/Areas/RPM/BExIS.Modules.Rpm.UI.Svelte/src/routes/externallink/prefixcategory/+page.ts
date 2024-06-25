import { host, setApiConfig } from '@bexis2/bexis2-core-ui';

/** @type {import('./$types').PageLoad} */
export function load() {
	if (import.meta.env.DEV) {
		console.log('dev');
		setApiConfig('http://localhost:44345', 'davidschoene', '123456');
	}

	console.log(host);

	return {};
}
