import type { dataSetType, ValidationReturn, createDatasetReturn } from './models.ts';
import mappingPublication from './mappingPublication.json';
import * as mapMetadata from './mapMetadata';
import * as apiCalls from './services/apiCalls';
import { tick } from 'svelte';

export async function createAllDatasets(
    metadataStructureId: any, // wenn type = number, dann Fehler bei Funktionsaufruf in +page.svelte <- war vor dem auslagern number
    entityTemplateId: any,  // wenn type = number, dann Fehler bei Funktionsaufruf in +page.svelte <- war vor dem auslagern number
    validationReturnObj: ValidationReturn,
    createDatasetReturnObj: createDatasetReturn,
    dataStructureId: number = 0
) {
    try {
        let dss: dataSetType[] = [];

        for (const row of validationReturnObj.validData) {
            dss.push(mapToApiFormat(row, mappingPublication, createDatasetReturnObj));
        }

        createDatasetReturnObj.uploadedCount = 0; // Reset beim Start

        for (let index = 0; index < dss.length; index++) {
            const ds = dss[index];
            const csvId = index;

            ds.MetadataStructureId = metadataStructureId;
            ds.EntityTemplateId = entityTemplateId;
            ds.DataStructureId = dataStructureId;

            let res = await apiCalls.createDataset(ds);
            getDatasets(res, csvId, createDatasetReturnObj);

            createDatasetReturnObj.uploadedCount++;

            // Update alle 100 oder beim letzten
            if (createDatasetReturnObj.uploadedCount % 100 === 0 || createDatasetReturnObj.uploadedCount === validationReturnObj.validData.length) {
                await tick(); // sorgt für UI-Update
            }
        }

        for (const map of createDatasetReturnObj.idMapping) {
            const rowIndex = map[0];
            const metadataId = map[1];

            let metadata = await apiCalls.GetMetadata(metadataId);
            let MetadataScheema: any = null;
            if (MetadataScheema == null) {
                MetadataScheema = await apiCalls.GetMetadataScheema(metadata['@id']);
            }

            // console.log('metadata', metadata);

            mapMetadata.applyMappingToMetadata(metadata, validationReturnObj.validData[rowIndex], mappingPublication.Mappings);
            console.log('finaldata', metadata);
            await apiCalls.putMetadata(metadataId, metadata);
            await apiCalls.GetMetadata(metadataId);
        }
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