import { ComponentFixture, TestBed } from '@angular/core/testing';
import { computed } from '@angular/core';
import { Router } from '@angular/router';
import { of } from 'rxjs';

import { AuthStateService } from '../../auth/services/auth-state.service';
import { FantasyTeamApiService } from '../services/fantasy-team-api.service';
import { TeamBuilderApiService } from '../services/team-builder-api.service';
import { TeamBuilderPageComponent } from './team-builder-page.component';

describe('TeamBuilderPageComponent', () => {
  let component: TeamBuilderPageComponent;
  let fixture: ComponentFixture<TeamBuilderPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamBuilderPageComponent],
      providers: [
        {
          provide: TeamBuilderApiService,
          useValue: {
            getDrivers: () => of([]),
            getConstructors: () => of([])
          }
        },
        {
          provide: FantasyTeamApiService,
          useValue: {
            createTeam: () => of(null),
            updateTeam: () => of(null),
            getMyTeam: () => of(null)
          }
        },
        {
          provide: AuthStateService,
          useValue: {
            isAuthenticated: computed(() => false),
            currentUser: computed(() => null)
          }
        },
        {
          provide: Router,
          useValue: {
            navigate: vi.fn()
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TeamBuilderPageComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
