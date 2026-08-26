import type { dataSetType, ValidationReturn, createDatasetReturn } from './models.ts';
import mappingPublication from './mappingPublication.json';
import * as mapMetadata from './mapMetadata';
import * as apiCalls from './services/apiCalls';
import { tick } from 'svelte';
import { DataCounterStore } from './stores.js';

export async function createAllDatasets(
    metadataStructureId: any,
    entityTemplateId: any,
    validationReturnObj: ValidationReturn,
    createDatasetReturnObj: createDatasetReturn,
    dataStructureId: number = 0
) {
    try {
        let dss: dataSetType[] = [];

        // 1️⃣ CSV → Dataset-Grundobjekte (Title / Description)
        for (const row of validationReturnObj.validData) {
            dss.push(mapToApiFormat(row, mappingPublication, createDatasetReturnObj));
        }

        createDatasetReturnObj.uploadedCount = 0;

        // 2️⃣ Datasets anlegen
        for (let index = 0; index < dss.length; index++) {
            const ds = dss[index];
            const csvId = index;

            ds.MetadataStructureId = metadataStructureId;
            ds.EntityTemplateId = entityTemplateId;
            ds.DataStructureId = dataStructureId;

            let res = await apiCalls.createDataset(ds);
            getDatasets(res, csvId, createDatasetReturnObj);

            createDatasetReturnObj.uploadedCount++;
            DataCounterStore.set(createDatasetReturnObj.uploadedCount);

            if (
                createDatasetReturnObj.uploadedCount % 100 === 0 ||
                createDatasetReturnObj.uploadedCount === validationReturnObj.validData.length
            ) {
                await tick();
            }
        }

        // 3️⃣ Metadaten holen, mappen & aktualisieren
        for (const map of createDatasetReturnObj.idMapping) {
            const rowIndex = map[0];
            const metadataId = map[1];

            let metadata = await apiCalls.GetMetadata(metadataId);

            let MetadataScheema: any = null;
            if (MetadataScheema == null) {
                MetadataScheema = await apiCalls.GetMetadataScheema(metadata['@id']);
            }

            // bestehendes Mapping (Publication, Authors, etc.)
            mapMetadata.applyMappingToMetadata(
                metadata,
                validationReturnObj.validData[rowIndex],
                mappingPublication.Mappings
            );

            // ✅ NEU: Endnote-ID als @comment auf Root-Ebene setzen
            const csvRow = validationReturnObj.validData[rowIndex];
            const endnoteId = csvRow?.ID;

            if (endnoteId) {
                const comment = `Import based on endnote ID: ${endnoteId}`;

                const reorderedMetadata: any = {};

                for (const key of Object.keys(metadata)) {
                    // zuerst alles bis inkl. @id übernehmen
                    reorderedMetadata[key] = metadata[key];

                    if (key === '@id') {
                        reorderedMetadata['@comment'] = comment;
                    }
                }

                metadata = reorderedMetadata;
            }

            console.log('finaldata', metadata);

            await apiCalls.putMetadata(metadataId, metadata);
            await apiCalls.GetMetadata(metadataId);
        }
    } catch (error) {
        console.error('Fehler beim Erstellen der Datensätze:', error);
    }
}

function mapToApiFormat(
    csvRow: any,
    mapping: any,
    createDatasetReturnObj: createDatasetReturn
) {
    let ds: dataSetType = {
        Title: '',
        Description: '',
        DataStructureId: 0,
        MetadataStructureId: 0,
        EntityTemplateId: 0
    };

    // Mapping für Title / Description
    for (let index = 0; index < 2; index++) {
        const map = mapping.Mappings[index];
        let sourceField = map.Source;

        if (sourceField) {
            if (index % 2 === 0) {
                ds.Title = csvRow[sourceField];
            } else if (ds.Title !== null) {
                ds.Title = ds.Title;
                ds.Description = csvRow[sourceField];
            } else {
                createDatasetReturnObj.tempTitle = null;
            }
        }
    }

    return ds;
}

function getDatasets(
    response: any,
    csvId: number,
    createDatasetReturnObj: createDatasetReturn
) {
    createDatasetReturnObj.idMapping.push([csvId, response.id]);
}
