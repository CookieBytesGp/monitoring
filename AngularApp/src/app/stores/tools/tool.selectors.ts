import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ToolState } from './tool.state';

export const selectToolState = createFeatureSelector<ToolState>('tools');

export const selectTools = createSelector(selectToolState, (state) => state.tools);
export const selectSelectedTool = createSelector(selectToolState, (state) => state.selectedTool);
export const selectLoading = createSelector(selectToolState, (state) => state.loading);
export const selectError = createSelector(selectToolState, (state) => state.error);