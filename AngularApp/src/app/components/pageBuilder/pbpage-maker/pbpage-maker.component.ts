import { Component, HostListener, OnInit } from '@angular/core';
import {  Router } from '@angular/router';
import { PageModel } from '../../../models/PageBuilder/PageModel';
import { TopNavEditorComponent } from './top-nav-editor/top-nav-editor.component';
import { SideBarEditorComponent } from './side-bar-editor/side-bar-editor.component';
import { EditorMainComponent } from './editor-main/editor-main.component';
import { PageFacade } from '../../../stores/pages/page.facade';
import { map } from 'rxjs';
import { CommonModule } from '@angular/common';
import { LoaderComponent } from "../../shared/loader/loader/loader.component";

@Component({
  selector: 'app-pbpage-maker',
  standalone: true,
  imports: [TopNavEditorComponent, SideBarEditorComponent, EditorMainComponent, CommonModule, LoaderComponent],
  templateUrl: './pbpage-maker.component.html',
  styleUrl: './pbpage-maker.component.css'
})
export class PBPageMakerComponent implements OnInit {

  pageData$ = this.pageStore.selectedPage$.pipe(
    map(page => page ? { ...page } : null)
  );
  loading$ = this.pageStore.loading$;
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
  constructor(
    private router: Router,
    private pageStore: PageFacade
  ) { }

  ngOnInit() {
    if (sessionStorage.getItem('shouldRedirect')) {
      sessionStorage.removeItem('shouldRedirect');
      this.router.navigate(['/pageBuilder']);
    };
  }

}
