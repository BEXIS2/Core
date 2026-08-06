import { Api } from '@bexis2/bexis2-core-ui';
import { writable } from 'svelte/store';
import type { CreateGroupModel, UpdateGroupModel } from './types';

export const usersStore = writable([]);
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

export async function deleteGroup(id:number) {
  try {
    const response = await Api.delete('/api/groups/' + id, {  });
  } catch (err) {
    groupsStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function updateGroup(id:number, model:UpdateGroupModel) {
  try {
    console.log('Updating user with model:', model);
    console.log('User ID:', id);
    await Api.put('/api/groups/' + id, model);
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    groupsStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function createGroup(model:CreateGroupModel) {
  try {
    const response = await Api.post('/api/groups/', model);
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    groupsStore.set([]); // Fehlerfall: leere Liste
  }
}