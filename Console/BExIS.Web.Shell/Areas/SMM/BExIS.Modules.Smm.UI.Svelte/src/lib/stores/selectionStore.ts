import { persisted } from "./persist";

/**
 * These values distinctly identify the data the user is currently working on.
 * [datasetId, versionId]       ...     distinct identifier for the actual scientificNames and matching results
 * [versionNr]                  ...     only for user readability
 * [stepId]                     ...     identifies a matching step (matching file-based against multiple APIs results in multiple such steps)
 * [datastructureId]            ...     only used as helper variable for backend functionalities
 * -1                           ...     not selected
 * 
 * They are used everywhere and guide the flow - and selection of data during the whole matching process.
 */
export const matchingSelection = persisted('matchingSelection', {
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

