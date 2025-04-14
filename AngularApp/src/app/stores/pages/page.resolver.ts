import { inject } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { PageFacade } from './page.facade';
import { Observable } from 'rxjs';
import { filter, first } from 'rxjs/operators';
import { PageModel } from '../../models/PageBuilder/PageModel';

export const pageResolver: ResolveFn<PageModel[]> = (): Observable<PageModel[]> => {
  const pageFacade = inject(PageFacade);

  return pageFacade.loadPages().pipe(
    filter((pages): pages is PageModel[] => !!pages?.length),
    first()
  );
};