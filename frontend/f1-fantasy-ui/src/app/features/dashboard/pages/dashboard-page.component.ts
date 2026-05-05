import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  effect,
  HostListener,
  inject,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { DashboardApiService } from '../services/dashboard-api.service';
import {
  Dashboard,
  DashboardCountdown,
  DashboardLeaderboardEntry,
  DashboardUpcomingRace,
} from '../models/dashboard.models';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { interval } from 'rxjs';
import { CommonModule } from '@angular/common';
import { AuthStateService } from '../../auth/services/auth-state.service';

@Component({
  standalone: true,
  selector: 'app-dashboard-page',
  imports: [CommonModule],
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPageComponent implements OnInit {
  private readonly dashboardApiService = inject(DashboardApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly authState = inject(AuthStateService);
  private leaderboardScrollElement: HTMLElement | null = null;
  private visibilityMeasureTimeout: number | null = null;

  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly isCurrentUserCardVisible = signal(true);

  readonly dashboard = signal<Dashboard | null>(null);
  readonly countdown = signal<DashboardCountdown | null>(null);

  readonly currentUser = this.authState.currentUser;
  readonly upcomingRace = computed(() => this.dashboard()?.upcomingRace ?? null);
  readonly leaderboard = computed(() => this.dashboard()?.leaderboard ?? []);
  readonly currentUserEntry = computed(() => {
    const user = this.currentUser();

    if (!user) {
      return null;
    }

    return this.leaderboard().find((entry) => entry.userId === Number(user.id)) ?? null;
  });

  @ViewChild('leaderboardScroller')
  set leaderboardScroller(ref: ElementRef<HTMLElement> | undefined) {
    this.leaderboardScrollElement = ref?.nativeElement ?? null;
    this.scheduleCurrentUserVisibilityMeasure();
  }

  constructor() {
    effect(() => {
      this.currentUserEntry();
      this.scheduleCurrentUserVisibilityMeasure();
    });

    this.destroyRef.onDestroy(() => {
      if (this.visibilityMeasureTimeout !== null) {
        window.clearTimeout(this.visibilityMeasureTimeout);
      }
    });
  }

  readonly raceDateLabel = computed(() => {
    const race = this.upcomingRace();
    return this.formatRaceDate(race?.startTimeUtc ?? null);
  });

  readonly raceLocationLabel = computed(() => {
    const race = this.upcomingRace();

    if (!race) {
      return '';
    }

    return this.buildRaceLocationLabel(race);
  });

  private buildRaceLocationLabel(race: DashboardUpcomingRace): string {
    return [race.locality, race.country].filter(Boolean).join(', ');
  }

  ngOnInit(): void {
    this.loadDashboard();
    this.startCountdownTimer();
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    this.scheduleCurrentUserVisibilityMeasure();
  }

  private startCountdownTimer(): void {
    interval(1000)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.updateCountdown();
      });
  }

  private loadDashboard(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.dashboardApiService
      .getDashboard()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (dashboard) => {
          this.dashboard.set(dashboard);
          this.updateCountdown();
          this.scheduleCurrentUserVisibilityMeasure();
          this.isLoading.set(false);
        },
        error: () => {
          this.dashboard.set(null);
          this.updateCountdown();
          this.scheduleCurrentUserVisibilityMeasure();
          this.errorMessage.set('Failed to load dashboard data');
          this.isLoading.set(false);
        },
      });
  }

  private updateCountdown(): void {
    const race = this.upcomingRace();

    if (!race?.startTimeUtc) {
      this.countdown.set(null);
      return;
    }

    const raceTime = new Date(race.startTimeUtc).getTime();
    const currentTime = Date.now();
    const milisecondsUntilRace = raceTime - currentTime;

    if (milisecondsUntilRace <= 0) {
      this.countdown.set({
        days: 0,
        hours: 0,
        minutes: 0,
        seconds: 0,
        isStarted: true,
      });

      return;
    }
    this.countdown.set(this.calculateCountdown(milisecondsUntilRace));
  }

  private calculateCountdown(milisecondsUntilRace: number): DashboardCountdown {
    const totalSeconds = Math.floor(milisecondsUntilRace / 1000);

    const secondsInDay = 86400;
    const secondsInHour = 3600;
    const secondsInMinute = 60;

    const days = Math.floor(totalSeconds / secondsInDay);
    const hours = Math.floor((totalSeconds % secondsInDay) / secondsInHour);
    const minutes = Math.floor((totalSeconds % secondsInHour) / secondsInMinute);
    const seconds = totalSeconds % secondsInMinute;

    return { days, hours, minutes, seconds, isStarted: false };
  }

  private formatRaceDate(startTimeUtc: string | null): string {
    if (!startTimeUtc) {
      return 'Date undefined';
    }

    return new Intl.DateTimeFormat(undefined, {
      dateStyle: 'full',
      timeStyle: 'short',
    }).format(new Date(startTimeUtc));
  }

  formatCountdownUnit(value: number): string {
    return String(value).padStart(2, '0');
  }

  trackLeaderboardEntry(index: number, entry: DashboardLeaderboardEntry): number {
    return entry.userId;
  }

  isCurrentUserEntry(entry: DashboardLeaderboardEntry): boolean {
    const user = this.currentUser();

    if (!user) {
      return false;
    }

    return Number(user.id) === entry.userId;
  }

  onLeaderboardScroll(): void {
    this.measureCurrentUserVisibility();
  }

  private scheduleCurrentUserVisibilityMeasure(): void {
    if (this.visibilityMeasureTimeout !== null) {
      return;
    }

    this.visibilityMeasureTimeout = window.setTimeout(() => {
      this.visibilityMeasureTimeout = null;
      this.measureCurrentUserVisibility();
    });
  }

  private measureCurrentUserVisibility(): void {
    const scroller = this.leaderboardScrollElement;

    if (!scroller || !this.currentUserEntry()) {
      this.isCurrentUserCardVisible.set(true);
      return;
    }

    const currentUserCard = scroller.querySelector<HTMLElement>('[data-current-user="true"]');

    if (!currentUserCard) {
      this.isCurrentUserCardVisible.set(true);
      return;
    }

    const scrollerRect = scroller.getBoundingClientRect();
    const cardRect = currentUserCard.getBoundingClientRect();
    const isVisible = cardRect.top >= scrollerRect.top && cardRect.bottom <= scrollerRect.bottom;

    this.isCurrentUserCardVisible.set(isVisible);
  }
}
