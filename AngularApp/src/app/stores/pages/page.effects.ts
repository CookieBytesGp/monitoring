import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { PageService } from '../../services/pagesServices/page.service';
import { loadPages, loadPagesSuccess, loadPagesFailure, updatePage, updateSuccess, updateFailure, } from './page.actions';
import { catchError, map, switchMap, of } from 'rxjs';

@Injectable()
export class PageEffects {
  loadPages$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadPages),
      switchMap(() =>
        this.pageService.getAllPages().pipe(
          map((pages) => loadPagesSuccess({ pages })),
          catchError((error) => of(loadPagesFailure({ error: error.message })))
        )
      )
    )
  );

  updatePage$ = createEffect(() =>
    this.actions$.pipe(
      ofType(updatePage),
      switchMap(({ page }) => 
        this.pageService.updatePage(page.id, page).pipe(
          // Since API returns 204, we use the page from the action
          map(() => updateSuccess({ page })),
          catchError(error => of(updateFailure({ error: error.message })))
        )
      )
    )
  );

  constructor(private actions$: Actions, private pageService: PageService) {}
}