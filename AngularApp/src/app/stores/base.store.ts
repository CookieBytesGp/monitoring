// import { BehaviorSubject, Observable } from 'rxjs';
// import { Action } from '../models/sotre/action.model';

// export abstract class BaseStore<T> {
//   private stateSubject = new BehaviorSubject<T | null>(null);
//   state$: Observable<T | null> = this.stateSubject.asObservable();

//   private loadingSubject = new BehaviorSubject<{ [key: string]: boolean }>({});
//   loading$: Observable<{ [key: string]: boolean }> = this.loadingSubject.asObservable();

//   private errorSubject = new BehaviorSubject<{ [key: string]: string | null }>({});
//   error$: Observable<{ [key: string]: string | null }> = this.errorSubject.asObservable();

//   dispatch(action: Action): void {
//     switch (action.type) {
//       case 'SET_STATE':
//         this.setState(action.payload);
//         break;

//       case 'SET_LOADING':
//         this.setLoading(action.payload.key, action.payload.isLoading);
//         break;

//       case 'SET_ERROR':
//         this.setError(action.payload.key, action.payload.error);
//         break;

//       default:
//         this.handleCustomAction(action);
//     }
//   }

//   protected abstract handleCustomAction(action: Action): void;

//   protected setState(state: T): void {
//     this.stateSubject.next(state);
//   }

//   getState(): T | null {
//     return this.stateSubject.getValue();
//   }

//   protected setLoading(key: string, isLoading: boolean): void {
//     const currentLoadingState = this.loadingSubject.getValue();
//     this.loadingSubject.next({ ...currentLoadingState, [key]: isLoading });
//   }

//   protected setError(key: string, error: string | null): void {
//     const currentErrorState = this.errorSubject.getValue();
//     this.errorSubject.next({ ...currentErrorState, [key]: error });
//   }

//   resetState(): void {
//     this.stateSubject.next(null);
//     this.loadingSubject.next({});
//     this.errorSubject.next({});
//   }
// }