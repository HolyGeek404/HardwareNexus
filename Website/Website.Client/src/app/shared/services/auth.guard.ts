import {inject} from '@angular/core';
import {CanActivateFn, Router} from '@angular/router';
import {SessionStore} from "../store/session.store";

export const authGuard: CanActivateFn = async () => {
    const session = inject(SessionStore);
    const router = inject(Router);

    await session.initialize();

    if (!session.isAuthenticated()) {
        return router.createUrlTree(['/user/sign-in']);
    }

    return true;
};
