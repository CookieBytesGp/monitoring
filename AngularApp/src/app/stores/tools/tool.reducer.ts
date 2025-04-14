import { createReducer, on } from '@ngrx/store';
import { loadTools, loadToolsSuccess, loadToolsFailure, selectTool, updateTool, removeTool } from './tool.actions';
import { ToolModel } from '../../models/PageBuilder/ToolModel';
import { initialToolState, ToolState } from './tool.state';

export const toolReducer = createReducer(
  initialToolState,
  on(loadTools, (state) => ({ ...state, loading: true, error: null })),
  on(loadToolsSuccess, (state, { tools }) => ({ ...state, tools, loading: false })),
  on(loadToolsFailure, (state, { error }) => ({ ...state, loading: false, error })),
  on(selectTool, (state, { tool }) => ({ ...state, selectedTool: tool })),
  on(updateTool, (state, { tool }) => ({
    ...state,
    tools: state.tools.map((t) => (t.id === tool.id ? tool : t)),
  })),
  on(removeTool, (state, { toolId }) => ({
    ...state,
    tools: state.tools.filter((t) => t.id !== toolId),
  }))
);