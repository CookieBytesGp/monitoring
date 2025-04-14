export interface AssetModel {
    url: string;
    type: string;
    altText: string;
    content: string;
    metadata: { [key: string]: string };
}