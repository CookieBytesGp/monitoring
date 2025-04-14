import { createReducer, on } from '@ngrx/store';
import { loadPages, loadPagesSuccess, loadPagesFailure, selectPage, updatePage, removePage, updateSuccess, updateFailure } from './page.actions';
import { PageModel } from '../../models/PageBuilder/PageModel';
import { initialPageState, PageState } from './page.state';

export const pageReducer = createReducer(
  initialPageState,
  on(loadPages, (state) => ({ ...state, loading: true, error: null })),
  on(loadPagesSuccess, (state, { pages }) => ({ ...state, pages, loading: false })),
  on(loadPagesFailure, (state, { error }) => ({ ...state, loading: false, error })),
  on(selectPage, (state, { page }) => ({ ...state, selectedPage: page })),
  on(updatePage, (state) => ({ ...state, loading: true })),
  on(updateSuccess, (state, { page }) => ({
    ...state,
    loading: false,
    pages: state.pages.map(p => p.id === page.id ? page : p),
    selectedPage: page
  })),
  on(updateFailure, (state, { error }) => ({ 
    ...state, 
    loading: false,
    error 
  })),
  on(removePage, (state, { pageId }) => ({
    ...state,
    pages: state.pages.filter((p) => p.id !== pageId),
  }))
);
