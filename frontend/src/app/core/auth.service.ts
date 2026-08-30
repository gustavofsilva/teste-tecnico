import { Injectable, computed, signal } from '@angular/core';
import { AuthResponse, User } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'access_token'; private readonly userKey = 'current_user';
  private readonly tokenState = signal<string | null>(this.readToken());
  private readonly userState = signal<User | null>(this.readUser());
  readonly user = this.userState.asReadonly(); readonly isAuthenticated = computed(() => this.tokenState() !== null);
  setSession(response: AuthResponse): void { localStorage.setItem(this.tokenKey, response.token); localStorage.setItem(this.userKey, JSON.stringify(response.user)); this.tokenState.set(response.token); this.userState.set(response.user); }
  updateUser(user: User): void { localStorage.setItem(this.userKey, JSON.stringify(user)); this.userState.set(user); }
  token(): string | null { return this.tokenState(); }
  logout(): void { localStorage.removeItem(this.tokenKey); localStorage.removeItem(this.userKey); this.tokenState.set(null); this.userState.set(null); }
  private readToken(): string | null { try { return localStorage.getItem(this.tokenKey); } catch { return null; } }
  private readUser(): User | null { try { const value = localStorage.getItem(this.userKey); return value ? JSON.parse(value) as User : null; } catch { return null; } }
}
