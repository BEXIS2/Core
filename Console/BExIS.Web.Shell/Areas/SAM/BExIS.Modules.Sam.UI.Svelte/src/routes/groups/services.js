import { Api } from '@bexis2/bexis2-core-ui';
import { writable } from 'svelte/store';

export const groupsStore = writable([]);

export async function getGroups() {
  try {
    const response = await Api.get('/api/groups');
    groupsStore.set(await response.data); // Speichere Daten im Store
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    groupsStore.set([]); // Fehlerfall: leere Liste
  }
}