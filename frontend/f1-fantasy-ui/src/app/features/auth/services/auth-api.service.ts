import {HttpClient} from '@angular/common/http';
import {Injectable, inject} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from '../../../../environments/environment';
import {
    AuthResponse,
    LoginRequest,
    RegisterRequest,
    User
} from '../models/auth.models';

@Injectable({
    providedIn: 'root'
})

export class AuthApiService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = `${environment.apiBaseUrl}/auth`;

    login(request: LoginRequest): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request);
    }

    register(request: RegisterRequest): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.baseUrl}/register`, request);
    }

    getMe(): Observable<User> {
        return this.http.get<User>(`${this.baseUrl}/me`);
    }
}