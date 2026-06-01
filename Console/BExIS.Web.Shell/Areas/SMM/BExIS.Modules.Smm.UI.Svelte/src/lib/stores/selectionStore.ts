import { persisted } from "./persist";

/**
 * Currently selected dataset and datastructure to progress the mapping process on.
 * -1 means selected none.
 */
export const mappingSelection = persisted('mappingSelection', {
    // unique const datasetId of the selected dataset
    datasetId: -1,
    // unique const datastructureId belonging to selected datasetId + versionId pair
    datastructureId: -1,
    // unique const version identifier
    versionId: -1,
    // dynamic version number (only for client display)
    versionNr: -1,
    // unique identifier of the StepEntry that is being selected (Matching step)
    stepId: -1
});

