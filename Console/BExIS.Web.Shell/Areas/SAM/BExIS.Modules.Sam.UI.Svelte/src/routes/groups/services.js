import { Api } from '@bexis2/bexis2-core-ui';
import { writable } from 'svelte/store';

export const usersStore = writable([]);

export async function getGroups() {
  try {
    const response = await Api.get('/api/groups');
    usersStore.set(await response.data); // Speichere Daten im Store
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    usersStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function deleteGroup(id) {
  try {
    await Api.delete('/api/groups/' + id, {  });
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    usersStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function editGroup(id, model) {
  try {
    await Api.put('/api/groups/' + id, { model });
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    usersStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function createGroup(model) {
  try {
    await Api.post('/api/groups/', { model });
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    usersStore.set([]); // Fehlerfall: leere Liste
  }
}