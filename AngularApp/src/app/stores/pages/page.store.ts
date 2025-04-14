// import { Injectable } from '@angular/core';
// import { BaseStore } from '../base.store';
// import { PageModel } from '../../models/PageBuilder/PageModel';
// import { PageService } from '../../services/pagesServices/page.service';
// import { Action } from '../../models/sotre/action.model';
// import { EventService } from '../../services/eventService/event.service';
// import { LOAD_PAGES_SUCCESS, REMOVE_PAGE, SELECT_PAGE, UPDATE_PAGE } from './page.constants';
// import { PAGE_REMOVED, PAGE_UPDATED } from './page.constants';
// import { selectAllPages, selectPageById } from './page.selectors';

// @Injectable({
//   providedIn: 'root',
// })
// export class PageStore extends BaseStore<PageModel[]> {
//   private pagesLoaded = false;

//   constructor(private pageService: PageService, private eventService: EventService) {
//     super();
//   }

//   loadPages(): void {
//     if (this.pagesLoaded) return;

//     this.setLoading('pages', true);

//     this.pageService.getAllPages().subscribe({
//       next: (pages) => {
//         this.setState(pages);
//         this.setLoading('pages', false);
//         this.pagesLoaded = true;
//       },
//       error: (err) => {
//         this.setLoading('pages', false);
//         this.setError('pages', err.message);
//       },
//     });
//   }

//   selectPage(page: PageModel): void {
//     this.dispatch({ type: SELECT_PAGE, payload: page });
//     this.eventService.emit(PAGE_UPDATED, page);
//   }

//   protected handleCustomAction(action: Action): void {
//     const currentState = this.getState() || [];

//     switch (action.type) {
//       case LOAD_PAGES_SUCCESS:
//         this.setState(action.payload);
//         this.setLoading('pages', false);
//         break;

//       case UPDATE_PAGE:
//         this.setState(
//           currentState.map((page) =>
//             page.id === action.payload.id ? action.payload : page
//           )
//         );
//         this.eventService.emit(PAGE_UPDATED, action.payload);
//         break;

//       case REMOVE_PAGE:
//         this.setState(
//           currentState.filter((page) => page.id !== action.payload.id)
//         );
//         this.eventService.emit(PAGE_REMOVED, action.payload.id);
//         break;

//       default:
//         console.warn(`Unhandled action type: ${action.type}`);
//     }
//   }

//   getAllPages(): PageModel[] {
//     return selectAllPages(this.getState());
//   }

//   getPageById(id: string): PageModel | undefined {
//     return selectPageById(this.getState(), id);
//   }
// }