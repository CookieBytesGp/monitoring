import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PBPageMakerComponent } from './pbpage-maker.component';

describe('PBPageMakerComponent', () => {
  let component: PBPageMakerComponent;
  let fixture: ComponentFixture<PBPageMakerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PBPageMakerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PBPageMakerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
