import { writable } from 'svelte/store';

/* If the content of a hook changes on the edit page, other hooks may need to be updated.
The date shows the last change on the page.*/
export const latestFileUploadDate = writable(0);

export const latestDataDescriptionDate = writable(0);

export const latestFileReaderDate = writable(0);

export const latestValidationDate = writable(0);

export const latestDataDate = writable(0);

export const latestSubmitDate = writable(0);

const dic: { [key: string]: number } = { ['']: 0 };
export const hooksStatus = writable(dic);
