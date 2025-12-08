import type { dataSetType, ValidationReturn, createDatasetReturn } from './models.ts';
import mappingPublication from './mappingPublication.json';
import * as mapMetadata from './mapMetadata';
import * as apiCalls from './services/apiCalls';
import { tick } from 'svelte';
import { createDatasetReturnStore } from './stores.js';
import type { Writable } from 'svelte/store';
import { get } from 'svelte/store';

export async function createAllDatasets(
    metadataStructureId: any,
    entityTemplateId: any,
    validationReturnObj: ValidationReturn,
    createDatasetReturnStore: Writable<createDatasetReturn>, // <-- hier!
    dataStructureId: number = 0
) {
    try {
        let dss: dataSetType[] = [];

        createDatasetReturnStore.update(v => ({ ...v, uploadedCount: 0, idMapping: [] }));

        for (const row of validationReturnObj.validData) {
            const v = get(createDatasetReturnStore);
            dss.push(mapToApiFormat(row, mappingPublication, v));
        }

        for (let index = 0; index < dss.length; index++) {
            const ds = dss[index];
            const csvId = index;

            ds.MetadataStructureId = metadataStructureId;
            ds.EntityTemplateId = entityTemplateId;
            ds.DataStructureId = dataStructureId;

            let res = await apiCalls.createDataset(ds);

            // Aktualisiere den Store reaktiv
            createDatasetReturnStore.update(v => {
                v.idMapping.push([csvId, res.id]);
                v.uploadedCount++;
                return v;
            });

            console.log(`Datensatz ${index + 1} erstellt.`);
            await tick();
        }

        // Danach ggf. deine Metadaten-Logik
    } catch (error) {
        console.error('Fehler beim Erstellen der Datensätze:', error);
    }
}


function mapToApiFormat(csvRow: any, mapping: any, createDatasetReturnObj: createDatasetReturn) {
    let ds: dataSetType = {
        Title: '',
        Description: '',
        DataStructureId: 0,
        MetadataStructureId: 0,
        EntityTemplateId: 0
    };

    for (let index = 0; index < 2; index++) {
        const map = mapping.Mappings[index];
        let sourceField = map.Source;

        if (sourceField) {
            if (index % 2 === 0) {
                // Jeder 1. Durchgang (Titel)
                ds.Title = csvRow[sourceField];
            } else if (ds.Title !== null) {
                // Jeder 2. Durchgang (Beschreibung)
                ds.Title = ds.Title;
                ds.Description = csvRow[sourceField];
            } else {
                createDatasetReturnObj.tempTitle = null; // Zurücksetzen für das nächste Paar
            }
        }
    }

    return ds;
}

function getDatasets(response: any, csvId: number, createDatasetReturnObj: createDatasetReturn) {
    createDatasetReturnObj.idMapping.push([csvId, response.id]);
}