import { createFeatureSelector, createSelector } from '@ngrx/store';
import { PageState } from './page.state';

export const selectPageState = createFeatureSelector<PageState>('pages');

export const selectPages = createSelector(selectPageState, (state) => state.pages);
export const selectSelectedPage = createSelector(selectPageState, (state) => state.selectedPage);
export const selectLoading = createSelector(selectPageState, (state) => state.loading);
export const selectError = createSelector(selectPageState, (state) => state.error);