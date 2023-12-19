import type { listItemType } from '@bexis2/bexis2-core-ui';

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

export class meaningEntryType {
	mappingRelation: listItemType;
	mappedLinks: listItemType[];

	public constructor() {
		this.mappingRelation = { id: -1, text: '', group: '' };
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
