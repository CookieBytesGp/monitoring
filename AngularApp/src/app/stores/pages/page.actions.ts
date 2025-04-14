import { createAction, props } from '@ngrx/store';
import { PageModel } from '../../models/PageBuilder/PageModel';

export const loadPages = createAction('[Pages] Load Pages');
export const loadPagesSuccess = createAction('[Pages] Load Pages Success', props<{ pages: PageModel[] }>());
export const loadPagesFailure = createAction('[Pages] Load Pages Failure', props<{ error: string }>());
export const selectPage = createAction('[Pages] Select Page', props<{ page: PageModel }>());
export const updatePage = createAction('[Pages] Update Page', props<{ page: PageModel }>());
export const updateSuccess = createAction('[Pages] Select Page Updated', props<{ page: PageModel }>());
export const updateFailure = createAction('[Pages] Selected Page Update Failure', props<{ error: string }>());
export const removePage = createAction('[Pages] Remove Page', props<{ pageId: string }>());