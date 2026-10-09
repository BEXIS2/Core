import { Api } from '@bexis2/bexis2-core-ui';
import { writable } from 'svelte/store';
import type { CreateDataCiteModel, ReadDataCiteModel, UpdateDataCiteModel } from './types';

export const dataCiteStore = writable<ReadDataCiteModel[]>([]);

export async function getDataCites() {
  try {
    const response = await Api.get('/api/dataCite');
    dataCiteStore.set(await response.data); // Speichere Daten im Store
  } catch (err) {
    console.error('Fehler beim Laden der Posts:', err);
    dataCiteStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function deleteDataCiteById(id:number) {
  try {
    const response = await Api.delete('/api/dataCite/' + id, {  });
  } catch (err) {
    dataCiteStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function updateUserById(id:number, model:UpdateDataCiteModel) {
  try {
    await Api.put('/api/dataCite/' + id, model);
  } catch (err) {
    dataCiteStore.set([]); // Fehlerfall: leere Liste
  }
}

export async function createUser(model:CreateDataCiteModel) {
  try {
    const response = await Api.post('/api/users/', model);
  } catch (err) {
    dataCiteStore.set([]); 
  }
}