import { Component, OnInit } from '@angular/core';
import { ToolModel } from '../../../../models/PageBuilder/ToolModel';
import { ToolService } from '../../../../services/toolsServices/tool.service';
import { CommonModule } from '@angular/common';
import { ToolFacade } from '../../../../stores/tools/tool.facade';

@Component({
  selector: 'side-bar-editor',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './side-bar-editor.component.html',
  styleUrl: './side-bar-editor.component.css'
})
export class SideBarEditorComponent {

  tools$ = this.toolStore.tools$;
  Selction: string = 'tools';
  activeTab: string = 'style';

  constructor(private toolStore: ToolFacade) {}

  GoTab(tool: string) {
    this.Selction = tool;
  }

  onToolClick(toolName: string) {
    alert("Tool clicked: " + toolName);
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }
}
