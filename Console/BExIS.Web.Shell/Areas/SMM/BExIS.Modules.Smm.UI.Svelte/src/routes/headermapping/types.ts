export interface Variable {
    id: number,
    name: string,
    unit: string,
    dataType: string,
    isKeys: boolean
}

export interface ListItem {
    id: number,
    text: string,
    group: string,
    description: string
}

export interface UnitItem {
    id: number,
    text: string,
    group: string,
    data_types: string[]
}

export interface VariableTemplateItem {
    id: number,
    text: string,
    group: string,
    description: string,
    data_type: string,
    unit: string,
    data_types: string[],
    units: string[],
    meanings: string[],
    constraints: string[]
}

export interface Link {
    label: string,
    link: string,
    prefix: string,
    releation: string
}

export interface Meaning {
    group: string,
    id: number,
    constraints: string[],
    links: Link[],
    text: string,
}

export interface VariableInstanceModel {
    is_key: boolean,
    is_optional: boolean,
    display_pattern: ListItem,
    possible_units: UnitItem[],
    name: string,
    id: number,
    meanings: Meaning[],
    possible_templates: VariableTemplateItem[],
    possible_display_patterns: ListItem[]
}

export interface MissingValueModel {
    display_name: string,
    description: string
}

export interface DataStructureEditModel {
    id: number,
    title: string,
    description: string,
    preview: string[],
    variables: VariableInstanceModel[],
    missing_values: MissingValueModel[]
}

export interface MultiSelectSourceDetailed {
    value: string,
    label: string
}
