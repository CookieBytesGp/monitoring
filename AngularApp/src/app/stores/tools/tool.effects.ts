import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { ToolService } from '../../services/toolsServices/tool.service';
import { loadTools, loadToolsSuccess, loadToolsFailure } from './tool.actions';
import { catchError, map, switchMap, of } from 'rxjs';

@Injectable()
export class ToolEffects {
  loadTools$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadTools),
      switchMap(() =>
        this.toolService.getAllTools().pipe(
          map((tools) => loadToolsSuccess({ tools })),
          catchError((error) => of(loadToolsFailure({ error: error.message })))
        )
      )
    )
  );

  constructor(private actions$: Actions, private toolService: ToolService) {}
}