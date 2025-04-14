import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { loadPages, selectPage, updatePage, removePage } from './page.actions';
import { selectPages, selectSelectedPage, selectLoading, selectError } from './page.selectors';
import { PageModel } from '../../models/PageBuilder/PageModel';
import { first, map, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PageFacade {
  pages$ = this.store.select(selectPages);
  selectedPage$ = this.store.select(selectSelectedPage);
  loading$ = this.store.select(selectLoading);
  error$ = this.store.select(selectError);

  private pagesLoaded = false; // Tracks whether pages are already loaded
  
  constructor(private store: Store) {}
  
  loadPages() {
    if (!this.pagesLoaded) {
      this.store.dispatch(loadPages());
      this.pagesLoaded = true;
    }
    return this.pages$;
  }
  selectPage(page: PageModel) {
    this.store.dispatch(selectPage({ page }));
  }

  updatePage(page: PageModel) {
    this.store.dispatch(updatePage({ page }));
  }

  removePage(pageId: string) {
    this.store.dispatch(removePage({ pageId }));
  }
}