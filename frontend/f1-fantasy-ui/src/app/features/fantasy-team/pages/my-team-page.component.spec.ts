import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { FantasyTeamApiService } from '../services/fantasy-team-api.service';
import { MyTeamPageComponent } from './my-team-page.component';

describe('MyTeamPageComponent', () => {
  let component: MyTeamPageComponent;
  let fixture: ComponentFixture<MyTeamPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyTeamPageComponent],
      providers: [
        {
          provide: FantasyTeamApiService,
          useValue: {
            getMyTeam: () =>
              of({
                id: 1,
                name: 'Test Team',
                budgetCap: 100,
                remainingBudget: 12.5,
                userId: 1,
                username: 'tester',
                constructors: [],
                drivers: []
              })
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MyTeamPageComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
