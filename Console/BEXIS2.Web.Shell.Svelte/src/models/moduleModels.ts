import { ReadSettingModel } from './settingModels';

export class ReadModuleModel {
	id: string;
	title: string;
	description: string;
	settings: ReadSettingModel;

	constructor(json: any) {
		this.id = json.id;
		this.title = json.title;
		this.description = json.description;
		this.settings = json.settings;
	}
}
