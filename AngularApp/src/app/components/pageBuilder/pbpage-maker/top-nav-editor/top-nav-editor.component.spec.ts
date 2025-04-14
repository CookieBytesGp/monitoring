import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TopNavEditorComponent } from './top-nav-editor.component';

describe('TopNavEditorComponent', () => {
  let component: TopNavEditorComponent;
  let fixture: ComponentFixture<TopNavEditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TopNavEditorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TopNavEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
