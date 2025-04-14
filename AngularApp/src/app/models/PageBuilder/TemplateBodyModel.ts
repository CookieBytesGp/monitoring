export interface TemplateBodyModel {
    htmlTemplate: string; 
    defaultCssClasses: { [key: string]: string }; 
    customCss: string;
    customJs: string; 
    isFloating: boolean; 
}