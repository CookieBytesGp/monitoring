import { createAction, props } from '@ngrx/store';
import { ToolModel } from '../../models/PageBuilder/ToolModel';

export const loadTools = createAction('[Tools] Load Tools');
export const loadToolsSuccess = createAction('[Tools] Load Tools Success', props<{ tools: ToolModel[] }>());
export const loadToolsFailure = createAction('[Tools] Load Tools Failure', props<{ error: string }>());
export const selectTool = createAction('[Tools] Select Tool', props<{ tool: ToolModel }>());
export const updateTool = createAction('[Tools] Update Tool', props<{ tool: ToolModel }>());
export const removeTool = createAction('[Tools] Remove Tool', props<{ toolId: string }>());