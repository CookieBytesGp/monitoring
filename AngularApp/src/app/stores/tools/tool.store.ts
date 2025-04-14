// import { Injectable } from '@angular/core';
// import { BaseStore } from '../base.store';
// import { ToolModel } from '../../models/PageBuilder/ToolModel';
// import { ToolService } from '../../services/toolsServices/tool.service';
// import { selectAllTools, selectToolById } from './tool.selectors';
// import { Action } from '../../models/sotre/action.model';
// import { EventService } from '../../services/eventService/event.service';
// import { CLEAR_SELECTED_TOOL, LOAD_TOOLS_SUCCESS, REMOVE_TOOL, SELECT_TOOL, TOOL_REMOVED, TOOL_UPDATED, UPDATE_TOOL } from './tool.constants';

// @Injectable({
//   providedIn: 'root',
// })
// export class ToolStore extends BaseStore<ToolModel[]> {
//   private toolsLoaded = false;

//   constructor(private toolService: ToolService, private eventService: EventService) {
//     super();
//   }

//   loadTools(): void {
//     if (this.toolsLoaded) return;

//     this.setLoading('tools', true);

//     this.toolService.getAllTools().subscribe({
//       next: (tools) => {
//         this.setState(tools);
//         this.setLoading('tools', false);
//         this.toolsLoaded = true;
//       },
//       error: (err) => {
//         this.setLoading('tools', false);
//         this.setError('tools', err.message);
//       },
//     });
//   }

//   selectTool(tool: ToolModel): void {
//     this.dispatch({ type: SELECT_TOOL, payload: tool });
//     this.eventService.emit('TOOL_SELECTED', tool);
//   }

//   clearSelectedTool(): void {
//     this.dispatch({ type: CLEAR_SELECTED_TOOL });
//     this.setState([]);
//   }

//   protected handleCustomAction(action: Action): void {
//     const currentState = this.getState() || [];

//     switch (action.type) {
//       case LOAD_TOOLS_SUCCESS:
//         this.setState(action.payload);
//         this.setLoading('tools', false);
//         break;

//       case UPDATE_TOOL:
//         this.setState(
//           currentState.map((tool) =>
//             tool.id === action.payload.id ? action.payload : tool
//           )
//         );
//         this.eventService.emit(TOOL_UPDATED, action.payload);
//         break;

//       case REMOVE_TOOL:
//         this.setState(
//           currentState.filter((tool) => tool.id !== action.payload.id)
//         );
//         this.eventService.emit(TOOL_REMOVED, action.payload.id);
//         break;

//       default:
//         console.warn(`Unhandled action type: ${action.type}`);
//     }
//   }

//   getAllTools(): ToolModel[] {
//     return selectAllTools(this.getState());
//   }

//   getToolById(id: string): ToolModel | undefined {
//     return selectToolById(this.getState(), id);
//   }
// }