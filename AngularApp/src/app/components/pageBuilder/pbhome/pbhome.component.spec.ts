import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PBHomeComponent } from './pbhome.component';

describe('PBHomeComponent', () => {
  let component: PBHomeComponent;
  let fixture: ComponentFixture<PBHomeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PBHomeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PBHomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
