import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { ToolFacade } from './tool.facade';
import { Observable } from 'rxjs';
import { filter, first } from 'rxjs/operators';
import { ToolModel } from '../../models/PageBuilder/ToolModel';

export const toolResolver: ResolveFn<ToolModel[]> = (): Observable<ToolModel[]> => {
  const toolFacade = inject(ToolFacade);

  return toolFacade.loadTools().pipe(
    filter((tools): tools is ToolModel[] => !!tools?.length),
    first()
  );
};