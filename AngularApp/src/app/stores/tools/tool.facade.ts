import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { loadTools, selectTool, updateTool, removeTool } from './tool.actions';
import { selectTools, selectSelectedTool, selectLoading, selectError } from './tool.selectors';
import { ToolModel } from '../../models/PageBuilder/ToolModel';

@Injectable({
  providedIn: 'root',
})
export class ToolFacade {
  tools$ = this.store.select(selectTools);
  selectedTool$ = this.store.select(selectSelectedTool);
  loading$ = this.store.select(selectLoading);
  error$ = this.store.select(selectError);

  private toolsLoaded = false;

  constructor(private store: Store) {}

   loadTools() {
    if (!this.toolsLoaded) {
      this.store.dispatch(loadTools());
      this.toolsLoaded = true;
    }
    return this.tools$;
  }

  selectTool(tool: ToolModel) {
    this.store.dispatch(selectTool({ tool }));
  }

  updateTool(tool: ToolModel) {
    this.store.dispatch(updateTool({ tool }));
  }

  removeTool(toolId: string) {
    this.store.dispatch(removeTool({ toolId }));
  }
}