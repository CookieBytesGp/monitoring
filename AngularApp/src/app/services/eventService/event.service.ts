import { Injectable } from '@angular/core';
import { filter, map, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class EventService {
  private eventSubject = new Subject<{ event: string; payload: any }>();

  emit(event: string, payload: any): void {
    this.eventSubject.next({ event, payload });
  }

  on(event: string) {
    return this.eventSubject.asObservable().pipe(
      filter((e) => e.event === event),
      map((e) => e.payload)
    );
  }
}