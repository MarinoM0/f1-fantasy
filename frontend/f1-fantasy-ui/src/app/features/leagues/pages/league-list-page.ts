import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LeaguesApiService } from '../services/leagues-api.service';
import { LeagueSummary } from '../models/league.models';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { takeUntil } from 'rxjs';

@Component({
  standalone:true,
  selector: 'app-league-list-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './league-list-page.html',
  styleUrl: './league-list-page.css',
})

export class LeagueListPage implements OnInit{
  private readonly leaguesApi = inject(LeaguesApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);

  //------------page state-----------------------
  readonly leagues = signal<LeagueSummary[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');


  //------------modal state---------------------
  readonly isCreateModalOpen = signal(false);
  readonly isJoinModalOpen = signal(false);


  //----------------forms--------------------------------------------------
  readonly createForm: FormGroup = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]]
  });

  readonly joinForm: FormGroup = this.formBuilder.nonNullable.group({
    inviteCode: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
  });

  readonly isSubmitting = signal(false);
  readonly submitError = signal('');

  //--------------------data loading---------
  ngOnInit(): void {
    this.loadLeagues();
  }

  private loadLeagues(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.leaguesApi
      .getMyLeagues()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (leagues) => {
          this.leagues.set(leagues);
          this.isLoading.set(false);
        },
        error: () => {
          this.leagues.set([]);
          this.errorMessage.set('Failed to load your leagues');
          this.isLoading.set(false);
        },
      });
  }

  //--------------modal open/close--------------------------
  openCreateModal(): void {
    this.createForm.reset({ name: ''});
    this.submitError.set('');
    this.isCreateModalOpen.set(true);
  }

  openJoinModal(): void {
    this.joinForm.reset({ inviteCode: ''});
    this.submitError.set('');
    this.isJoinModalOpen.set(true);
  }

  closeModals(): void {
    this.isCreateModalOpen.set(false);
    this.isJoinModalOpen.set(false);
    this.submitError.set('');
  }

  //---------------submit handlers-------------------------

  onSubmitCreate(): void {
    if(this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const name = this.createForm.controls['name'].value.trim();

    this.isSubmitting.set(true);
    this.submitError.set('');

    this.leaguesApi
      .createLeague({ name })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (league) => {
          this.isSubmitting.set(false);
          this.closeModals();
          this.router.navigate(['/leagues', league.id]);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.submitError.set(this.extractErrorMessage(err, 'Failed to create league'));
        },
      });
  }

  onSubmitJoin() : void {
    if (this.joinForm.invalid) {
      this.joinForm.markAllAsTouched();
      return;
    }

    const code = this.joinForm.controls['inviteCode'].value.trim().toUpperCase();

    this.isSubmitting.set(true);
    this.submitError.set('');

    this.leaguesApi
      .joinLeague({ inviteCode: code })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (league) => {
          this.isSubmitting.set(false);
          this.closeModals();
          this.router.navigate(['/leagues', league.id]); 
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.submitError.set(this.extractErrorMessage(err, 'Failed to join league'));
        }
      });
  }


  //------------------------------helpers---------------------------------

  nameError(): string {
    const control = this.createForm.controls['name'];
    if (!control.touched || control.valid) return '';
    if (control.errors?.['required']) return 'League name is required.';
    if (control.errors?.['minlength']) return 'Must be at least 3 characters.';
    if (control.errors?.['maxlength']) return 'Must be 50 characters or fewer.';
    return '';
  }

  inviteCodeError(): string {
    const control = this.joinForm.controls['inviteCode'];
    if (!control.touched || control.valid) return '';
    if (control.errors?.['required']) return 'Invite code is required.';
    if (control.errors?.['minlength'] || control.errors?.['maxlength']) {
      return 'Invite code must be exactly 6 characters.';
    }
    return '';
  }

  trackLeague(_index: number, league: LeagueSummary): number {
    return league.id;
  }

  private extractErrorMessage(err: unknown, fallback: string): string {
    const httpError = err as { error?: { detail?: string } };
    return httpError.error?.detail ?? fallback;
  }
}
