import type { HeaderMappings, MatchingProgress } from "$lib/types/types"

export interface ProgressOverview {
    success: boolean,
    hasHeaderMappings: boolean,
    hasMatchingProgress: boolean,
    isTailored: boolean,
    headerMappings: HeaderMappings,
    matchingProgress: MatchingProgress
}

