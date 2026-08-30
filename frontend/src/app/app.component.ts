import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';

@Component({ selector: 'app-root', standalone: true, imports: [RouterOutlet, RouterLink], template: `
  <header><a class="brand" routerLink="/dashboard">Minha Conta</a>
    @if (auth.isAuthenticated()) { <nav><a routerLink="/dashboard">Início</a><a routerLink="/perfil">Editar perfil</a><button class="link" (click)="logout()">Sair</button></nav> }
  </header><main><router-outlet /></main>` })
export class AppComponent {
  readonly auth = inject(AuthService); private readonly router = inject(Router);
  logout(): void { this.auth.logout(); void this.router.navigateByUrl('/login'); }
}
