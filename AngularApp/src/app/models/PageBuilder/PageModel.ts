import { BaseElemntModel } from "./BaseElementModel";

export interface PageModel {
    id:string;
    title:string;
    createdAt:Date;
    updatedAt:Date;
    elements: BaseElemntModel[];
}