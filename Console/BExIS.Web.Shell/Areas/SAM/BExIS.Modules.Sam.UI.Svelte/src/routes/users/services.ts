import { Api } from '@bexis2/bexis2-core-ui';
import { writable } from 'svelte/store';
import type { CreateUserModel, UpdateUserModel } from './types';

export const usersStore = writable([]);
export const groupsStore = writable([]);

export async function getUsers() {
  try {
    const response = await Api.get('/api/users');
    usersStore.set(await response.data); // Speichere Daten im Store
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    usersStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function getGroups() {
  try {
    const response = await Api.get('/api/groups');
    groupsStore.set(await response.data); // Speichere Daten im Store
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    groupsStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function deleteUser(id:number) {
  try {
    const response = await Api.delete('/api/users/' + id, {  });
  } catch (err) {
    usersStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function updateUser(id:number, model:UpdateUserModel) {
  try {
    console.log('Updating user with model:', model);
		console.log('User ID:', id);
    await Api.put('/api/users/' + id, model);
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    usersStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function createUser(model:CreateUserModel) {
  try {
    const response = await Api.post('/api/users/', model);
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    usersStore.set([]); // Fehlerfall: leere Liste
  }
}