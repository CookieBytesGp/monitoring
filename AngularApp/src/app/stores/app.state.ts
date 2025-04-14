import { ActionReducerMap } from '@ngrx/store';
import { toolReducer } from './tools/tool.reducer';
import { pageReducer } from './pages/page.reducer';
import { PageState } from './pages/page.state';
import { ToolState } from './tools/tool.state';

export interface AppState {
  tools: ToolState;
  pages: PageState;
}

export const appReducers: ActionReducerMap<AppState> = {
  tools: toolReducer,
  pages: pageReducer,
};