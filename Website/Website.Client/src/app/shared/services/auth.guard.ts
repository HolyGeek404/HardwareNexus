import {inject} from '@angular/core';
import {CanActivateFn, Router} from '@angular/router';
import {SessionStore} from "../store/session.store";

export const authGuard: CanActivateFn = () => {
    const session = inject(SessionStore);
    const router = inject(Router);

    if (!session.isReady()) {
        return router.createUrlTree(['/login']);
    }

    if (!session.isAuthenticated()) {
        return router.createUrlTree(['/login']);
    }

    return true;
};