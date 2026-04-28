import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, map, of, startWith } from 'rxjs';
import { getApiErrorMessage } from '../../../shared/utils/api-error.utils';
import {
  FantasyTeam,
  FantasyTeamConstructor,
  FantasyTeamDriver
} from '../models/fantasy-team.models';
import { FantasyTeamApiService } from '../services/fantasy-team-api.service';

type MyTeamState =
  | {
      status: 'loading';
      team: FantasyTeam | null;
      errorMessage: string;
    }
  | {
      status: 'loaded';
      team: FantasyTeam;
      errorMessage: string;
    }
  | {
      status: 'empty' | 'error';
      team: FantasyTeam | null;
      errorMessage: string;
    };

@Component({
  standalone: true,
  selector: 'app-my-team-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './my-team-page.component.html',
  styleUrl: './my-team-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyTeamPageComponent {
  private static readonly MAX_DRIVERS = 5;
  private static readonly MAX_CONSTRUCTORS = 2;
  private static readonly TEAM_COLORS: Record<string, string> = {
    Ferrari: '#dc0000',
    'Red Bull Racing': '#1e5bc6',
    Mercedes: '#00d2be',
    McLaren: '#ff8700',
    'Aston Martin': '#006f62',
    Alpine: '#0090ff',
    Williams: '#005aff',
    'Racing Bulls': '#6692ff',
    Audi: '#8b1e2d',
    'Audi F1 Team': '#8b1e2d',
    'Kick Sauber': '#52e252',
    Sauber: '#52e252',
    'Haas F1 Team': '#b6babd',
    Haas: '#b6babd'
  };

  private readonly fantasyTeamApiService = inject(FantasyTeamApiService);
  private readonly teamState = toSignal(
    this.fantasyTeamApiService.getMyTeam().pipe(
      map(
        (team): MyTeamState => ({
          status: 'loaded',
          team,
          errorMessage: ''
        })
      ),
      catchError((error) => {
        const status = error?.status === 404 ? 'empty' : 'error';

        return of<MyTeamState>({
          status,
          team: null,
          errorMessage:
            status === 'empty'
              ? 'You have not created a fantasy team yet.'
              : getApiErrorMessage(error, 'Failed to load your fantasy team.')
        });
      }),
      startWith({
        status: 'loading',
        team: null,
        errorMessage: ''
      } satisfies MyTeamState)
    ),
    {
      initialValue: {
        status: 'loading',
        team: null,
        errorMessage: ''
      } satisfies MyTeamState
    }
  );

  readonly isLoading = computed(() => this.teamState().status === 'loading');
  readonly team = computed(() => this.teamState().team);
  readonly errorMessage = computed(() => this.teamState().errorMessage);
  readonly spentBudget = computed(() => {
    const currentTeam = this.team();

    if (!currentTeam) {
      return 0;
    }

    return currentTeam.budgetCap - currentTeam.remainingBudget;
  });
  readonly driverCount = computed(() => this.team()?.drivers.length ?? 0);
  readonly constructorCount = computed(() => this.team()?.constructors.length ?? 0);
  readonly driverSlots = computed(() =>
    Array.from({
      length: Math.max(0, MyTeamPageComponent.MAX_DRIVERS - this.driverCount())
    })
  );
  readonly constructorSlots = computed(() =>
    Array.from({
      length: Math.max(0, MyTeamPageComponent.MAX_CONSTRUCTORS - this.constructorCount())
    })
  );
  readonly budgetUsagePercent = computed(() => {
    const currentTeam = this.team();

    if (!currentTeam || currentTeam.budgetCap <= 0) {
      return 0;
    }

    return Math.min(
      100,
      Math.max(0, (this.spentBudget() / currentTeam.budgetCap) * 100)
    );
  });
  readonly primaryTeamColor = computed(() => {
    const firstDriver = this.team()?.drivers[0];

    if (!firstDriver) {
      return '#ff2a23';
    }

    return this.getTeamColor(firstDriver.constructorName);
  });

  getDriverColor(driver: FantasyTeamDriver): string {
    return this.getTeamColor(driver.constructorName);
  }

  getConstructorColor(constructorItem: FantasyTeamConstructor): string {
    return this.getTeamColor(constructorItem.name);
  }

  private getTeamColor(teamName: string): string {
    return MyTeamPageComponent.TEAM_COLORS[teamName] ?? '#ffffff';
  }
}
