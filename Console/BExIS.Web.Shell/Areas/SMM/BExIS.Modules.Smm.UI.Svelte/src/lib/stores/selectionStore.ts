import { persisted } from "./persist";

/**
 * Currently selected dataset and datastructure to progress the mapping process on.
 * -1 means selected none.
 */
export const mappingSelection = persisted('mappingSelection', {
    datasetId: -1,
    datastructureId: -1
});

