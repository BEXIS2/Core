import { Api } from '@bexis2/bexis2-core-ui';
import { writable } from 'svelte/store';

export const usersStore = writable([]);

export async function getUsers() {
  try {
    const response = await Api.get('/api/users');
    usersStore.set(await response.data); // Speichere Daten im Store
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    usersStore.set([]); // Fehlerfall: leere Liste
  }
}