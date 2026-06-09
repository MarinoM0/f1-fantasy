import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { interval } from 'rxjs';
import { PredictionsApi } from '../services/predictions-api.service';
import { PredictionDriver, PredictionRace } from '../models/prediction.models';

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
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.submitError.set(this.extractErrorMessage(err, 'Failed to save your prediction.'));
        },
      });
  }

  trackDriver(_index: number, driver: PredictionDriver): number {
    return driver.id;
  }

  private pad(value: number): string {
    return String(value).padStart(2, '0');
  }

  private extractErrorMessage(err: unknown, fallback: string): string {
    const httpError = err as { error?: { detail?: string } };
    return httpError.error?.detail ?? fallback;
  }
}
