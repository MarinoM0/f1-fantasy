import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { LeaguesApiService } from '../services/leagues-api.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthStateService } from '../../auth/services/auth-state.service';
import { League, LeagueLeaderboardEntry } from '../models/league.models';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-league-detail-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './league-detail-page.html',
  styleUrl: './league-detail-page.css',
})

export class LeagueDetailPageComponent implements OnInit {
  private readonly leaguesApi = inject(LeaguesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly authState = inject(AuthStateService);

  readonly league = signal<League | null>(null);
  readonly leaderboard = signal<LeagueLeaderboardEntry[]>([]);
  readonly isLoading = signal(true);
  readonly isLeaderboardLoading = signal(true);
  readonly errorMessage = signal('');

  readonly pendingAction = signal<'leave' | 'delete' | null>(null);
  readonly isProcessing = signal(false);
  readonly actionError = signal('');

  readonly copiedMessage = signal('');

  readonly leagueId = computed(() => Number(this.route.snapshot.paramMap.get('id')));

  readonly currentUserId = computed(() => {
    const user = this.authState.currentUser();
    return user ? Number(user.id) : null;
  });

  ngOnInit(): void {
    this.loadLeague();
    this.loadLeaderboard();
  }

  private loadLeague(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.leaguesApi
      .getLeague(this.leagueId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (league) => {
          this.league.set(league);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.league.set(null);
          this.isLoading.set(false);
          this.errorMessage.set(
            this.isNotFoundError(err)
              ? 'League not found or you are not a member'
              : 'Failed to load this league',
          );
        }
      });
  }

  private loadLeaderboard(): void {
    this.isLeaderboardLoading.set(true);

    this.leaguesApi
      .getLeaderboard(this.leagueId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (entries) => {
          this.leaderboard.set(entries);
          this.isLeaderboardLoading.set(false);
        },
        error: () => {
          this.leaderboard.set([]);
          this.isLeaderboardLoading.set(false)
        }
      })
  }

  copyInviteCode(): void {
    const code = this.league()?.inviteCode;
    if (!code) return;

    navigator.clipboard.writeText(code).then(
      () => {
            this.copiedMessage.set('Copied');
            setTimeout(() => this.copiedMessage.set(''), 1500);
          },
          () => {
            this.copiedMessage.set('Copy failed');
            setTimeout(() => this.copiedMessage.set(''), 2500);
          }
      );
    }

    openLeaveConfirm(): void {
      this.actionError.set('');
      this.pendingAction.set('leave');
    }

    openDeleteConfirm(): void {
      this.actionError.set('');
      this.pendingAction.set('delete');
    }

    closeConfirm(): void {
      if (this.isProcessing()) return;
      this.pendingAction.set(null);
      this.actionError.set('');
    }

    confirmAction(): void {
      const action = this.pendingAction();
      if (action === 'leave') {
        this.runLeave();
      } else if (action == 'delete') {
        this.runDelete();
      }
    }

    private runLeave(): void {
      this.isProcessing.set(true);
      this.actionError.set('');

      this.leaguesApi
        .leaveLeague(this.leagueId())
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.isProcessing.set(false);
            this.pendingAction.set(null);
            this.router.navigate(['/leagues']);
          },
          error: (err) => {
            this.isProcessing.set(false);
            this.actionError.set(this.extractErrorMessage(err, 'Failed to leave league'));
          }
        });
    }

    private runDelete(): void {
      this.isProcessing.set(true);
      this.actionError.set('');

      this.leaguesApi
        .deleteLeague(this.leagueId())
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.isProcessing.set(false);
            this.pendingAction.set(null);
            this.router.navigate(['/leagues']);
          },
          error: (err) => {
            this.isProcessing.set(false);
            this.actionError.set(this.extractErrorMessage(err, 'Failed to delete league'));
          }
        })
    }


     // ============ Template helpers ============

    trackLeaderboardEntry(_index: number, entry: LeagueLeaderboardEntry): number {
      return entry.userId;
    }

    trackMember(_index: number, member: { userId: number }): number {
      return member.userId;
    }

    isCurrentUserEntry(entry: LeagueLeaderboardEntry): boolean {
      return entry.userId === this.currentUserId();
    }

    private isNotFoundError(err: unknown): boolean {
      return (err as { status?: number }).status === 404;
    }

    private extractErrorMessage(err: unknown, fallback: string): string {
      console.error('Leagues API error:', err);

      const httpError = err as {
        error?: {
          detail?: string;
          errors?: Record<string, string[]>;
          title?: string;
        };
      };

      if (httpError.error?.detail) return httpError.error.detail;

      const fieldErrors = httpError.error?.errors;
      if (fieldErrors) {
        const firstField = Object.keys(fieldErrors)[0];
        const firstMessage = fieldErrors[firstField]?.[0];
        if (firstMessage) return firstMessage;
      }

      return httpError.error?.title ?? fallback;
    }
}
