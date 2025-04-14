import { AssetModel } from "./AssetModel";
import { TemplateModel } from "./TemplateModel";

export interface ToolModel {
    id: string;
    name: string;
    defaultJs: string;
    elementType: string;
    templates: TemplateModel[];
    defaultAssets: AssetModel[];
}