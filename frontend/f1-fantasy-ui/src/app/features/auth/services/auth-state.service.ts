import { computed, inject, Injectable, signal } from "@angular/core";
import { AuthApiService } from "./auth-api.service";
import { AUTH_TOKEN_STORAGE_KEY } from "../auth.constant";
import { AuthResponse, LoginRequest, RegisterRequest, User } from "../models/auth.models";
import { catchError, map, Observable, of, tap } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class AuthStateService {
    private readonly authApi = inject(AuthApiService);
    private readonly storage = sessionStorage;

    private readonly tokenSignal = signal<string |null>(this.getStoredToken());
    private readonly currentUserSignal = signal<User | null>(null);
    private readonly initializedSignal = signal(false);

    readonly token = computed(() => this.tokenSignal());
    readonly currentUser = computed(() => this.currentUserSignal());
    readonly isAuthenticated = computed(() => !!this.tokenSignal());
    readonly isInitialized = computed(() => this.initializedSignal());

    login(request: LoginRequest) :Observable<User> {
        return this.authApi.login(request).pipe(
            tap((response) => this.applyAuthResponse(response)),
            map((response) => response.user)
        );
    }

    register(request: RegisterRequest): Observable<User> {
        return this.authApi.register(request).pipe(
            tap((response) => this.applyAuthResponse(response)),
            map((response) => response.user)
        );
    }

    initialize(): Observable<User | null> {
        const token = this.tokenSignal();

        if(!token) {
            this.initializedSignal.set(true);
            return of(null);
        }

        return this.authApi.getMe().pipe(
            tap((user) => {
                this.currentUserSignal.set(user);
                this.initializedSignal.set(true);
            }),
            catchError(() => {
                this.clearAuthState();
                this.initializedSignal.set(true);
                return of(null);
            })
        );
    }

    private getStoredToken(): string | null {
        return this.storage.getItem(AUTH_TOKEN_STORAGE_KEY);
    }

    private applyAuthResponse(response: AuthResponse): void {
        this.tokenSignal.set(response.token);
        this.currentUserSignal.set(response.user);
        this.storage.setItem(AUTH_TOKEN_STORAGE_KEY, response.token);
        this.initializedSignal.set(true);
    }

    private clearAuthState() : void {
        this.tokenSignal.set(null);
        this.currentUserSignal.set(null);
        this.storage.removeItem(AUTH_TOKEN_STORAGE_KEY);
    }
}

