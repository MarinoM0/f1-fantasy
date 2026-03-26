import { CanActivateFn, Router } from "@angular/router";
import { AuthStateService } from "../../features/auth/services/auth-state.service";
import { inject } from "@angular/core";

export const authGuard : CanActivateFn = () => {
    const authState = inject(AuthStateService);
    const router = inject(Router);

    if (authState.isAuthenticated()) {
        return true;
    }

    return router.createUrlTree(['/login']);
}