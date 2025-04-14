import { Component, ElementRef, OnInit, Renderer2 } from '@angular/core';
import { ToolService } from '../../../services/toolsServices/tool.service';
import { ToolModel } from '../../../models/PageBuilder/ToolModel';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TemplateModel } from '../../../models/PageBuilder/TemplateModel';

@Component({
  selector: 'app-edit-tool',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './edit-tool.component.html',
  styleUrl: './edit-tool.component.css'
})
export class EditToolComponent implements OnInit {
  editingToolId: string | null = null;
  editingToolData: ToolModel | null = null;
  selectedTemplate!: TemplateModel;
  constructor(
    private toolService: ToolService,
    private route: ActivatedRoute,
    private el: ElementRef,
    private renderer: Renderer2
  ) { };

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.editingToolId = params['id'];
      console.log('Editing Tool ID:', this.editingToolId);
      if (this.editingToolId) {
        this.toolService.getToolById(this.editingToolId).subscribe((data: ToolModel) => {
          this.editingToolData = data;
          console.log('Editing Tool Data:', this.editingToolData);
        });
      }
    });

    if (this.editingToolData && this.editingToolData.templates.length > 0) {
      this.selectedTemplate = this.editingToolData.templates[0];
      this.loadStyle();
    }
  }
  onStyleChange() {
    // This method is triggered when the user selects a new style
    console.log('Selected template:', this.selectedTemplate);
    this.loadStyle();
  }
  saveTool() {
    // Ensure defaultAssets[0] properties are not null or empty
    if (this.editingToolData?.defaultAssets && this.editingToolData.defaultAssets.length > 0) {
      const defaultAsset = this.editingToolData.defaultAssets[0];
    
      // Set mock data for null or empty properties
      if (!defaultAsset.url || defaultAsset.url.trim() === '') {
        defaultAsset.url = 'https://example.com/mock-url';
      }
      if (!defaultAsset.type || defaultAsset.type.trim() === '') {
        defaultAsset.type = 'Mock Type';
      }
      if (!defaultAsset.altText || defaultAsset.altText.trim() === '') {
        defaultAsset.altText = 'Mock Alt Text';
      }
      if (!defaultAsset.content || defaultAsset.content.trim() === '') {
        defaultAsset.content = 'Mock Content';
      }
      if (
        !defaultAsset.metadata || 
        (typeof defaultAsset.metadata === 'object' && Object.keys(defaultAsset.metadata).length === 0)
      ) {
        defaultAsset.metadata = { mockKey: 'Mock Metadata' };
      }
    }
  
    // Proceed with saving the tool
    if (this.editingToolData && this.selectedTemplate) {
      // Update the selected template in the templates list
      const templateIndex = this.editingToolData.templates.findIndex(
        (template) => template.htmlTemplate === this.selectedTemplate.htmlTemplate
      );
  
      if (templateIndex !== -1) {
        this.editingToolData.templates[templateIndex] = { ...this.selectedTemplate };
      } else {
        console.error('Selected template not found in the templates list.');
        return;
      }
  
      // Save the updated tool to the database using the service
      this.toolService.updateTool(this.editingToolData).subscribe(
        (response) => {
          console.log('Tool successfully saved:', response);
          alert('Tool saved successfully!');
        },
        (error) => {
          console.error('Error saving tool:', error);
          alert('Failed to save the tool. Please try again.');
        }
      );
    } else {
      console.error('Editing tool data or selected template is missing.');
      alert('Cannot save the tool. Please ensure all fields are filled.');
    }
  }
  loadStyle() {
    if (this.selectedTemplate && this.selectedTemplate.customCss) {
      // Remove any previously added <style> tags
      const existingStyle = this.el.nativeElement.querySelector('style.custom-style');
      if (existingStyle) {
        this.renderer.removeChild(this.el.nativeElement, existingStyle);
      }

      // Create a new <style> tag
      const styleElement = this.renderer.createElement('style');
      this.renderer.addClass(styleElement, 'custom-style');
      this.renderer.setProperty(styleElement, 'textContent', this.selectedTemplate.customCss);

      // Append the <style> tag to the component's host element
      this.renderer.appendChild(this.el.nativeElement, styleElement);
    }
  }

}
