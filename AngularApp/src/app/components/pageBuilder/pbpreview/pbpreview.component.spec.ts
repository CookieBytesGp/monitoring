import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PBPreviewComponent } from './pbpreview.component';

describe('PBPreviewComponent', () => {
  let component: PBPreviewComponent;
  let fixture: ComponentFixture<PBPreviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PBPreviewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PBPreviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
