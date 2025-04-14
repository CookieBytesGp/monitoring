import { Component, HostListener, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { PageModel } from '../../../models/PageBuilder/PageModel';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PageFacade } from '../../../stores/pages/page.facade';
import { firstValueFrom, map } from 'rxjs';
import { Actions, ofType } from '@ngrx/effects';
import { updateFailure, updateSuccess } from '../../../stores/pages/page.actions';

@Component({
  selector: 'app-pbsoft-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  providers: [],
  templateUrl: './pbsoft-edit.component.html',
  styleUrl: './pbsoft-edit.component.css',
})
export class PBSoftEditComponent implements OnInit {

   // Create a mutable copy of the page data
   pageData$ = this.pageStore.selectedPage$.pipe(
    map(page => page ? {...page} : null)
  );
  
  loading$ = this.pageStore.loading$;
  constructor(
    private pageStore: PageFacade,
    private router: Router,
    private actions$: Actions
  ) {}
  ngOnInit() {
    // Check if we need to redirect after a refresh
    if (sessionStorage.getItem('shouldRedirect')) {
      sessionStorage.removeItem('shouldRedirect');
      this.router.navigate(['/pageBuilder']);
    }
  }
  @HostListener('window:beforeunload', ['$event'])
  handleBeforeUnload(event: BeforeUnloadEvent) {
    event.preventDefault();
    event.returnValue = 'Changes you made may not be saved.';
    return event.returnValue;
  }

  @HostListener('window:unload')
  navigateOnUnload() {
    // Store a flag in sessionStorage
    sessionStorage.setItem('shouldRedirect', 'true');
  }
  async saveTitle(pageData: PageModel) {
    try {
      // Disable the save button during the update
      const saveButton = document.querySelector('.save-button') as HTMLButtonElement;
      if (saveButton) {
        saveButton.disabled = true;
      }

      this.pageStore.updatePage(pageData);
      
      // Wait for either success or failure
      const result = await Promise.race([
        firstValueFrom(this.actions$.pipe(ofType(updateSuccess))),
        firstValueFrom(this.actions$.pipe(ofType(updateFailure)))
      ]);

      if ('error' in result) {
        alert('Failed to update page: ' + result.error);
      } else {
        alert('Page successfully updated!');
      }
    } catch (error) {
      alert('An unexpected error occurred');
    } finally {
      // Re-enable the save button
      const saveButton = document.querySelector('.save-button') as HTMLButtonElement;
      if (saveButton) {
        saveButton.disabled = false;
      }
    }
  }
  goEditor() {
    this.router.navigate(['/pageBuilder/pageMaker']);
  }
}
