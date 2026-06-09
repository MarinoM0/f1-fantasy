import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { interval } from 'rxjs';
import { PredictionsApi } from '../services/predictions-api.service';
import {
  Prediction,
  PredictionDriver,
  PredictionLeaderboardEntry,
  PredictionRace,
} from '../models/prediction.models';
import { AuthStateService } from '../../auth/services/auth-state.service';

type PredictionsTab = 'predict' | 'history' | 'leaderboard';
type SlotStatus = 'exact' | 'podium' | 'miss' | 'pending';

@Component({
  standalone: true,
  selector: 'app-predictions-page',
  imports: [CommonModule],
  templateUrl: './predictions-page.html',
  styleUrl: './predictions-page.css',
})
export class PredictionsPage implements OnInit {
  private readonly predictionsApi = inject(PredictionsApi);
  private readonly destroyRef = inject(DestroyRef);
  private readonly authState = inject(AuthStateService);

  readonly activeTab = signal<PredictionsTab>('predict');

  readonly race = signal<PredictionRace | null>(null);
  readonly availableDrivers = signal<PredictionDriver[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  readonly selectedP1 = signal<PredictionDriver | null>(null);
  readonly selectedP2 = signal<PredictionDriver | null>(null);
  readonly selectedP3 = signal<PredictionDriver | null>(null);

  readonly isSubmitting = signal(false);
  readonly submitError = signal('');
  readonly successMessage = signal('');

  readonly nowMs = signal(Date.now());

  readonly predictions = signal<Prediction[]>([]);
  readonly isLoadingHistory = signal(false);
  readonly historyLoaded = signal(false);

  readonly leaderboard = signal<PredictionLeaderboardEntry[]>([]);
  readonly isLoadingLeaderboard = signal(false);
  readonly leaderboardLoaded = signal(false);

  readonly currentUserId = computed(() => {
    const user = this.authState.currentUser();
    return user ? Number(user.id) : null;
  });

  readonly isPodiumComplete = computed(
    () => !!this.selectedP1() && !!this.selectedP2() && !!this.selectedP3()
  );

  readonly isLocked = computed(() => {
    const r = this.race();
    if (!r) return false;
    return new Date(r.startTimeUtc).getTime() <= this.nowMs();
  });

  readonly canSubmit = computed(
    () => this.isPodiumComplete() && !this.isLocked() && !this.isSubmitting()
  );

  readonly lockCountdown = computed(() => {
    const r = this.race();
    if (!r) return '';
    const ms = new Date(r.startTimeUtc).getTime() - this.nowMs();
    if (ms <= 0) return 'Locked';

    const totalSeconds = Math.floor(ms / 1000);
    const days = Math.floor(totalSeconds / 86400);
    const hours = Math.floor((totalSeconds % 86400) / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    if (days > 0) return `${days}d ${hours}h ${minutes}m`;
    return `${this.pad(hours)}:${this.pad(minutes)}:${this.pad(seconds)}`;
  });

  ngOnInit(): void {
    this.loadUpcoming();

    interval(1000)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.nowMs.set(Date.now()));
  }

  setTab(tab: PredictionsTab): void {
    this.activeTab.set(tab);
    if (tab === 'history' && !this.historyLoaded()) this.loadHistory();
    if (tab === 'leaderboard' && !this.leaderboardLoaded()) this.loadLeaderboard();
  }

  private loadUpcoming(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.predictionsApi
      .getUpcoming()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.race.set(data.race);
          this.availableDrivers.set(data.availableDrivers);

          const existing = data.existingPrediction;
          if (existing) {
            this.selectedP1.set(existing.predictedP1);
            this.selectedP2.set(existing.predictedP2);
            this.selectedP3.set(existing.predictedP3);
          }

          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('Failed to load the upcoming race.');
          this.isLoading.set(false);
        },
      });
  }

  private loadHistory(): void {
    this.isLoadingHistory.set(true);

    this.predictionsApi
      .getMyPredictions()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (predictions) => {
          this.predictions.set(predictions);
          this.historyLoaded.set(true);
          this.isLoadingHistory.set(false);
        },
        error: () => {
          this.predictions.set([]);
          this.isLoadingHistory.set(false);
        },
      });
  }

  private loadLeaderboard(): void {
    this.isLoadingLeaderboard.set(true);

    this.predictionsApi
      .getLeaderboard()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (entries) => {
          this.leaderboard.set(entries);
          this.leaderboardLoaded.set(true);
          this.isLoadingLeaderboard.set(false);
        },
        error: () => {
          this.leaderboard.set([]);
          this.isLoadingLeaderboard.set(false);
        },
      });
  }

  assignDriver(driver: PredictionDriver): void {
    if (this.isLocked() || this.isDriverSelected(driver)) {
      return;
    }

    if (!this.selectedP1()) {
      this.selectedP1.set(driver);
    } else if (!this.selectedP2()) {
      this.selectedP2.set(driver);
    } else if (!this.selectedP3()) {
      this.selectedP3.set(driver);
    }

    this.successMessage.set('');
  }

  clearSlot(position: 1 | 2 | 3): void {
    if (this.isLocked()) return;

    if (position === 1) this.selectedP1.set(null);
    if (position === 2) this.selectedP2.set(null);
    if (position === 3) this.selectedP3.set(null);

    this.successMessage.set('');
  }

  isDriverSelected(driver: PredictionDriver): boolean {
    return (
      this.selectedP1()?.id === driver.id ||
      this.selectedP2()?.id === driver.id ||
      this.selectedP3()?.id === driver.id
    );
  }

  submit(): void {
    const race = this.race();
    const p1 = this.selectedP1();
    const p2 = this.selectedP2();
    const p3 = this.selectedP3();

    if (!race || !p1 || !p2 || !p3 || this.isLocked()) {
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set('');
    this.successMessage.set('');

    this.predictionsApi
      .submit({
        raceId: race.id,
        p1DriverId: p1.id,
        p2DriverId: p2.id,
        p3DriverId: p3.id,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.successMessage.set('Prediction saved! You can change it until the race locks.');
          this.historyLoaded.set(false);
          this.leaderboardLoaded.set(false);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.submitError.set(this.extractErrorMessage(err, 'Failed to save your prediction.'));
        },
      });
  }

  slotStatus(prediction: Prediction, position: 1 | 2 | 3): SlotStatus {
    if (!prediction.isScored) {
      return 'pending';
    }

    const predicted =
      position === 1 ? prediction.predictedP1
      : position === 2 ? prediction.predictedP2
      : prediction.predictedP3;

    const actualAtSlot =
      position === 1 ? prediction.actualP1
      : position === 2 ? prediction.actualP2
      : prediction.actualP3;

    if (actualAtSlot && predicted.id === actualAtSlot.id) {
      return 'exact';
    }

    const podiumIds = [
      prediction.actualP1?.id,
      prediction.actualP2?.id,
      prediction.actualP3?.id,
    ];

    return podiumIds.includes(predicted.id) ? 'podium' : 'miss';
  }

  pickedCode(prediction: Prediction, position: 1 | 2 | 3): string {
    if (position === 1) return prediction.predictedP1.code;
    if (position === 2) return prediction.predictedP2.code;
    return prediction.predictedP3.code;
  }

  isCurrentUserEntry(entry: PredictionLeaderboardEntry): boolean {
    return entry.userId === this.currentUserId();
  }

  trackDriver(_index: number, driver: PredictionDriver): number {
    return driver.id;
  }

  trackPrediction(_index: number, prediction: Prediction): number {
    return prediction.id;
  }

  trackEntry(_index: number, entry: PredictionLeaderboardEntry): number {
    return entry.userId;
  }

  private pad(value: number): string {
    return String(value).padStart(2, '0');
  }

  private extractErrorMessage(err: unknown, fallback: string): string {
    const httpError = err as { error?: { detail?: string } };
    return httpError.error?.detail ?? fallback;
  }
}
