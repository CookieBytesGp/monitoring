import { AssetModel } from "./AssetModel";
import { TemplateBodyModel } from "./TemplateBodyModel";

export interface BaseElemntModel {
    id:string;
    toolId:string;
    order:number;
    templateBody: TemplateBodyModel;
    asset: AssetModel;
}