import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { catchError, forkJoin, map, of, startWith, throwError } from 'rxjs';
import { AuthStateService } from '../../auth/services/auth-state.service';
import {
  CreateFantasyTeamRequest,
  FantasyTeam
} from '../models/fantasy-team.models';
import {
  TeamBuilderConstructor,
  TeamBuilderDriver
} from '../models/team-builder.models';
import { FantasyTeamApiService } from '../services/fantasy-team-api.service';
import { TeamBuilderApiService } from '../services/team-builder-api.service';
import { getApiErrorMessage } from '../../../shared/utils/api-error.utils';

type TeamBuilderLoadState = {
  status: 'loading' | 'loaded' | 'error';
  drivers: TeamBuilderDriver[];
  constructors: TeamBuilderConstructor[];
  savedTeam: FantasyTeam | null;
  errorMessage: string;
};

type TeamCreationState = 'idle' | 'submitting';

@Component({
  standalone: true,
  selector: 'app-team-builder-page',
  imports: [CommonModule],
  templateUrl: './team-builder-page.component.html',
  styleUrl: './team-builder-page.component.css'
})
export class TeamBuilderPageComponent {
  private static readonly TOTAL_BUDGET = 100;
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

  private readonly teamBuilderApiService = inject(TeamBuilderApiService);
  private readonly fantasyTeamApiService = inject(FantasyTeamApiService);
  private readonly authState = inject(AuthStateService);
  private readonly router = inject(Router);
  private readonly hasHydratedSavedTeam = signal(false);
  private readonly savedTeamSignal = signal<FantasyTeam | null>(null);
  private readonly creationErrorMessageSignal = signal('');
  private readonly teamBuilderState = toSignal(
    forkJoin({
      drivers: this.teamBuilderApiService.getDrivers(),
      constructors: this.teamBuilderApiService.getConstructors(),
      savedTeam: this.loadSavedTeam()
    }).pipe(
      map(
        ({ drivers, constructors, savedTeam }): TeamBuilderLoadState => ({
          status: 'loaded',
          drivers,
          constructors,
          savedTeam,
          errorMessage: ''
        })
      ),
      catchError(() =>
        of<TeamBuilderLoadState>({
          status: 'error',
          drivers: [],
          constructors: [],
          savedTeam: null,
          errorMessage: 'Failed to load team builder data.'
        })
      ),
      startWith({
        status: 'loading',
        drivers: [],
        constructors: [],
        savedTeam: null,
        errorMessage: ''
      } satisfies TeamBuilderLoadState)
    ),
    {
      initialValue: {
        status: 'loading',
        drivers: [],
        constructors: [],
        savedTeam: null,
        errorMessage: ''
      } satisfies TeamBuilderLoadState
    }
  );

  readonly selectedDriverIds = signal<number[]>([]);
  readonly selectedConstructorIds = signal<number[]>([]);
  readonly activeTab = signal<'drivers' | 'constructors'>('drivers');
  readonly searchTerm = signal('');
  readonly creationState = signal<TeamCreationState>('idle');
  readonly isLoading = computed(() => this.teamBuilderState().status === 'loading');
  readonly errorMessage = computed(() => this.teamBuilderState().errorMessage);
  readonly drivers = computed(() => this.teamBuilderState().drivers);
  readonly constructors = computed(() => this.teamBuilderState().constructors);
  readonly isAuthenticated = this.authState.isAuthenticated;
  readonly currentUser = this.authState.currentUser;

  readonly driverCount = computed(() => this.drivers().length);
  readonly constructorCount = computed(() => this.constructors().length);
  readonly selectedDrivers = computed(() =>
    this.drivers().filter((driver) => this.selectedDriverIds().includes(driver.id))
  );
  readonly selectedConstructors = computed(() =>
    this.constructors().filter((constructorItem) =>
      this.selectedConstructorIds().includes(constructorItem.id)
    )
  );
  readonly selectedDriverCount = computed(() => this.selectedDrivers().length);
  readonly selectedConstructorCount = computed(() => this.selectedConstructors().length);
  readonly totalBudget = computed(() => TeamBuilderPageComponent.TOTAL_BUDGET);
  readonly spentBudget = computed(
    () =>
      this.selectedDrivers().reduce((total, driver) => total + driver.price, 0) +
      this.selectedConstructors().reduce(
        (total, constructorItem) => total + constructorItem.price,
        0
      )
  );
  readonly remainingBudget = computed(() => this.totalBudget() - this.spentBudget());
  readonly filteredDrivers = computed(() => {
    const search = this.searchTerm().trim().toLowerCase();

    return this.drivers()
      .filter((driver) => {
        if (!search) {
          return true;
        }

        return [driver.fullName, driver.code, driver.constructorName]
          .join(' ')
          .toLowerCase()
          .includes(search);
      })
      .sort((first, second) => second.price - first.price);
  });
  readonly filteredConstructors = computed(() => {
    const search = this.searchTerm().trim().toLowerCase();

    return this.constructors()
      .filter((constructorItem) => {
        if (!search) {
          return true;
        }

        return [constructorItem.name, constructorItem.code]
          .join(' ')
          .toLowerCase()
          .includes(search);
      })
      .sort((first, second) => second.price - first.price);
  });
  readonly isTeamComplete = computed(
    () =>
      this.selectedDriverCount() === TeamBuilderPageComponent.MAX_DRIVERS &&
      this.selectedConstructorCount() === TeamBuilderPageComponent.MAX_CONSTRUCTORS
  );
  readonly isSubmitting = computed(() => this.creationState() === 'submitting');
  private readonly hasExistingTeamSignal = computed(() => !!this.savedTeamSignal());

  private readonly transferUsedSignal = computed(
    () => this.savedTeamSignal()?.hasUsedTransfer ?? false
  );

  private readonly canCreateTeamSignal = computed (
    () => 
      this.isAuthenticated() &&
      this.isTeamComplete() &&
      !this.isSubmitting() &&
      !this.transferUsedSignal()
  );

  private readonly createTeamButtonLabelSignal = computed(() => {
    if (!this.isAuthenticated()) {
      return 'Login to create a team';
    }

    if(this. transferUsedSignal()) {
      return 'No transfers remaining';
    }

    if (this.isSubmitting()) {
      return this.hasExistingTeamSignal() ? 'Updating team...' : 'Creating team...';
    }

    return this.hasExistingTeamSignal() ? 'Update team' : 'Create team';
  });

  constructor() {
    effect(
      () => {
        const state = this.teamBuilderState();

        if (state.status !== 'loaded' || this.hasHydratedSavedTeam()) {
          return;
        }

        this.hasHydratedSavedTeam.set(true);
        this.savedTeamSignal.set(state.savedTeam);

        if (!state.savedTeam) {
          return;
        }

        this.selectedDriverIds.set(state.savedTeam.drivers.map((driver) => driver.id));
        this.selectedConstructorIds.set(
          state.savedTeam.constructors.map((constructorItem) => constructorItem.id)
        );
      },
      { allowSignalWrites: true }
    );
  }

  trackDriver(index: number, driver: TeamBuilderDriver): number {
    return driver.id;
  }

  trackConstructor(index: number, constructorItem: TeamBuilderConstructor): number {
    return constructorItem.id;
  }

  setActiveTab(tab: 'drivers' | 'constructors'): void {
    this.activeTab.set(tab);
    this.searchTerm.set('');
  }

  setSearchTerm(value: string): void {
    this.searchTerm.set(value);
  }

  canCreateTeam(): boolean {
    return this.canCreateTeamSignal();
  }

  createTeamButtonLabel(): string {
    return this.createTeamButtonLabelSignal();
  }

  creationErrorMessage(): string {
    return this.creationErrorMessageSignal();
  }

  transferUsed(): boolean {
    return this.transferUsedSignal();
  }

  createTeam(): void {
    if (!this.canCreateTeam()) {
      return;
    }

    const request = this.buildCreateTeamRequest();
    this.creationState.set('submitting');
    this.creationErrorMessageSignal.set('');

    const saveRequest = this.hasExistingTeamSignal()
      ? this.fantasyTeamApiService.updateTeam(request)
      : this.fantasyTeamApiService.createTeam(request);

    saveRequest.subscribe({
      next: (team) => {
        this.creationState.set('idle');
        this.savedTeamSignal.set(team);
        this.selectedDriverIds.set(team.drivers.map((driver) => driver.id));
        this.selectedConstructorIds.set(
          team.constructors.map((constructorItem) => constructorItem.id)
        );
        void this.router.navigate(['/my-team']);
      },
      error: (error) => {
        this.creationState.set('idle');
        this.creationErrorMessageSignal.set(
          getApiErrorMessage(error, 'Failed to save fantasy team.')
        );
      }
    });
  }

  getDriverColor(driver: TeamBuilderDriver): string {
    return this.getTeamColor(driver.constructorName);
  }

  getConstructorColor(constructorItem: TeamBuilderConstructor): string {
    return this.getTeamColor(constructorItem.name);
  }

  toggleDriver(driver: TeamBuilderDriver): void {
    this.creationErrorMessageSignal.set('');

    if (this.isDriverSelected(driver.id)) {
      this.selectedDriverIds.update((selectedIds) =>
        selectedIds.filter((id) => id !== driver.id)
      );
      return;
    }

    if (!this.canSelectDriver(driver)) {
      return;
    }

    this.selectedDriverIds.update((selectedIds) => [...selectedIds, driver.id]);
  }

  toggleConstructor(constructorItem: TeamBuilderConstructor): void {
    this.creationErrorMessageSignal.set('');

    if (this.isConstructorSelected(constructorItem.id)) {
      this.selectedConstructorIds.update((selectedIds) =>
        selectedIds.filter((id) => id !== constructorItem.id)
      );
      return;
    }

    if (!this.canSelectConstructor(constructorItem)) {
      return;
    }

    this.selectedConstructorIds.update((selectedIds) => [
      ...selectedIds,
      constructorItem.id
    ]);
  }

  isDriverSelected(driverId: number): boolean {
    return this.selectedDriverIds().includes(driverId);
  }

  isConstructorSelected(constructorId: number): boolean {
    return this.selectedConstructorIds().includes(constructorId);
  }

  canSelectDriver(driver: TeamBuilderDriver): boolean {
    return (
      this.selectedDriverCount() < TeamBuilderPageComponent.MAX_DRIVERS &&
      driver.price <= this.remainingBudget()
    );
  }

  canSelectConstructor(constructorItem: TeamBuilderConstructor): boolean {
    return (
      this.selectedConstructorCount() < TeamBuilderPageComponent.MAX_CONSTRUCTORS &&
      constructorItem.price <= this.remainingBudget()
    );
  }

  visibleResultCount(): number {
    return this.activeTab() === 'drivers'
      ? this.filteredDrivers().length
      : this.filteredConstructors().length;
  }

  private buildCreateTeamRequest(): CreateFantasyTeamRequest {
    return {
      name: this.resolveTeamName(),
      driverIds: [...this.selectedDriverIds()],
      constructorIds: [...this.selectedConstructorIds()]
    };
  }

  private resolveTeamName(): string {
    const savedTeam = this.savedTeamSignal();

    if (savedTeam) {
      return savedTeam.name;
    }

    const username = this.currentUser()?.username;

    if (username) {
      return `${username}'s Team`;
    }

    return 'My Team';
  }

  private loadSavedTeam() {
    if (!this.authState.isAuthenticated()) {
      return of<FantasyTeam | null>(null);
    }

    return this.fantasyTeamApiService.getMyTeam().pipe(
      map((team) => team as FantasyTeam | null),
      catchError((error) => {
        if (error?.status === 404) {
          return of<FantasyTeam | null>(null);
        }

        return throwError(() => error);
      })
    );
  }

  private getTeamColor(teamName: string): string {
    return TeamBuilderPageComponent.TEAM_COLORS[teamName] ?? '#ffffff';
  }
}
