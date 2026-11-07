import {patchState, signalStore, withComputed, withMethods, withState} from '@ngrx/signals';
import {computed, inject} from '@angular/core';
import {firstValueFrom} from 'rxjs';
import {UserApi} from '../../business/user/api/user.api';
import {UserModel} from '../models/models';
import {environment} from '../../../environments/environment';

type SessionState = {
    user: UserModel | null;
    isInitialized: boolean;
    isLoading: boolean;
};

const initialState: SessionState = {
    user: null,
    isInitialized: false,
    isLoading: false,
};

export const SessionStore = signalStore(
    {providedIn: 'root'},
    withState(initialState),
    withComputed(({user, isInitialized}) => ({
        isAuthenticated: computed(() => user() !== null),
        isReady: computed(() => isInitialized()),
    })),
    withMethods((store, api = inject(UserApi)) => {
        let initialization: Promise<void> | undefined;

        return {
            initialize(): Promise<void> {
                if (store.isInitialized()) return Promise.resolve();
                if (initialization) return initialization;

                patchState(store, {isLoading: true});
                initialization = firstValueFrom(api.getUserData()).then(user => {
                    patchState(store, {user, isInitialized: true, isLoading: false});
                }).catch(() => {
                    patchState(store, {user: null, isInitialized: true, isLoading: false});
                }).finally(() => {
                    initialization = undefined;
                });
                return initialization;
            },

            login(returnUrl = '/user/dashboard'): void {
                const safeReturnUrl = returnUrl.startsWith('/') && !returnUrl.startsWith('//')
                    ? returnUrl
                    : '/user/dashboard';
                window.location.assign(
                    `${environment.api_gateway_url}/user/login?returnUrl=${encodeURIComponent(safeReturnUrl)}`
                );
            },

            logout(): void {
                window.location.assign(`${environment.api_gateway_url}/user/signout`);
            },
        };
    }),
);
