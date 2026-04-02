import type { HeaderMappings, MappingProgress } from "$lib/types/types"

export interface ProgressOverview {
    success: boolean,
    hasHeaderMappings: boolean,
    hasMappingProgress: boolean,
    isTailored: boolean,
    headerMappings: HeaderMappings,
    mappingProgress: MappingProgress
}

