// Auto-generated from SearchConfigSchema.json
// Date: 2026-01-15

export interface SearchConfigSchema {
  global: GlobalConfig;
  local: LocalConfig[];
}

export interface GlobalConfig {
  search_components: GlobalSearchComponents;
  primary_data: GlobalPrimaryData;
  spatial_data: GlobalSpatialData;
  general: GlobalGeneral;
}

export interface GlobalSearchComponents {
  facets_to_index: boolean;
  categories_to_index: boolean;
  properties_to_index: boolean;
  generals_to_index: boolean;
  facets?: GlobalComponent[];
  categories?: GlobalComponent[];
  properties?: GlobalComponent[];
  general?: GlobalComponent[];
}

export interface GlobalComponent {
  id: number;
  component_name: string;
  description: string;
  placeholder: string;
  header_item: boolean;
  default_header_item: boolean;
}

export interface GlobalPrimaryData {
  to_index: boolean;
  calc: CalcBlock;
  spatial_data: GlobalPrimarySpatialData;
}

export interface CalcBlock {
  operation: "min/max" | "avg" | "sum";
  allowed_meanings: number[];
}

export interface GlobalPrimarySpatialData {
  allowed_data_type_ids: number[];
  lat_meaning: number;
  long_meaning: number;
}

export interface GlobalSpatialData {
  spatial_search?: boolean;
  spatial_search_settings?: SpatialSearchSettings;
}

export interface SpatialSearchSettings {
  crs: "EPSG:4326";
  axis_order: string[];
  basemap: "satellite";
  start_extend: string;
}

export interface GlobalGeneral {
  show_only_completed_metadata?: boolean;
  auto_complete_trigger?: number;
  show_empty_facets?: boolean;
}

export interface LocalConfig {
  entity_template_id: number;
  index_not_completed_metadata: boolean;
  search_components: LocalSearchComponents;
  spatial_data: LocalSpatialData;
  primary_data: LocalPrimaryData;
  external_sources: LocalExternalSources;
}

export interface LocalSearchComponents {
  facets?: LocalComponent[];
  categories?: LocalComponent[];
  properties?: LocalComponent[];
  general?: LocalComponent[];
}

export interface LocalComponent {
  global_id: number;
  data_type_id: "byte" | "short" | "integer" | "long" | "float" | "half_float" | "double" | "scaled_float" | "text" | "keyword" | "date" | "date_nanos" | "boolean" | "geo_point" | "geo_shape" | "object" | "nested" | "ip" | "version" | "binary";
  metadata_nodes: string[];
}

export interface LocalSpatialData {
  spatial_metadata?: LocalSpatialMetadata;
}

export type LocalSpatialMetadata =
  | BBoxSpatialMetadata
  | PointSpatialMetadata;

export interface BBoxSpatialMetadata {
  type: "bbox";
  WestBoundLongitude: string;
  EastBoundLongitude: string;
  SouthBoundLatitude: string;
  NorthBoundLatitude: string;
}

export interface PointSpatialMetadata {
  type: "point";
  longitude: string;
  latitude: string;
  radius: number;
}

export interface LocalPrimaryData {
  to_index: boolean;
  calc: CalcBlock | CalcBlock[];
}

export interface LocalExternalSources {
  source: string;
  local_path: string;
  external_name: string;
}

export interface CalcBlockListItem {
  id: string;
  template_name: string;
  operation: "min/max" | "avg" | "sum";
  allowed_meanings: MeaningsListItem[];
}

export interface MeaningsListItem {
  id: number;
  name: string;
}



// copied from RPM module types.ts

export class MeaningModel {
  id: number;
  name: string;
  description: string;
  selectable: boolean;
  approved: boolean;
  externalLinks: meaningEntryType[];
  related_meaning: MeaningModel[];
  constraints: listItemType[];

  public constructor(data: any) {
    if (data) {
      (this.id = data.id), (this.name = data.name);
      this.approved = data.approved;
      this.description = data.description;
      this.selectable = data.selectable;
      this.externalLinks = data.externalLinks;
      this.related_meaning = data.related_meaning;
      this.constraints = data.constraints;
    } else {
      this.id = 0;
      this.name = '';
      this.approved = false;
      this.description = '';
      this.selectable = false;
      this.externalLinks = [];
      this.related_meaning = [];
      this.constraints = [];
    }
  }
}

import type { listItemType } from '@bexis2/bexis2-core-ui';

export class meaningEntryType {
  mappingRelation: listItemType;
  mappedLinks: listItemType[];
  isValid: boolean = true;

  public constructor() {
    this.mappingRelation = { id: -1, text: '', group: '', description: '' };
    this.mappedLinks = [];
  }
}

export class externalLinkType {
  id: number;
  uri: string;
  name: string;
  type: listItemType | undefined;
  prefix: prefixListItemType | undefined;
  prefixCategory: prefixCategoryType | undefined;

  public constructor() {
    this.id = 0;
    this.uri = '';
    this.name = '';
    this.type = undefined;
    this.prefix = undefined;
    this.prefixCategory = undefined;
  }
}

export interface prefixListItemType {
  id: number;
  text: string;
  description: string;
  url: string;
}

export interface prefixCategoryType {
  id: number;
  name: string;
  description: string;
}

export enum externalLinkTypeEnum {
  prefix = 1,
  link = 2,
  entity = 3,
  characteristics = 4,
  vocabulary = 5,
  relationship = 6
}
