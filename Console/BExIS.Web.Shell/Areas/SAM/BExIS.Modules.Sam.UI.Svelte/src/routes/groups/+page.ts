import {setApiConfig} from '@bexis2/bexis2-core-ui';
import { getGroups } from '../../services/groupService';
import type { ReadGroupModel } from '../../models/groupModels';



/** @type {import('./$types').PageLoad} */
export async function load() {

	if (import.meta.env.DEV) {
		console.log('dev');
		setApiConfig('https://localhost:44345', 'davidschoene', '123456');
	}

	// return { groups: await getGroups() as ReadGroupModel[]};
	return {};
}