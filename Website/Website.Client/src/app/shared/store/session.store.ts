import {User} from "../../business/user/models/model";
import {patchState, signalStore, withComputed, withMethods, withState} from '@ngrx/signals';
import {computed, inject} from "@angular/core";
import {UserApi} from "../../business/user/api/user.api";
import {Router} from "@angular/router";
import {switchMap} from "rxjs";

type SessionState = {
    user: User | null;
    isInitialized: boolean;
    isLoading: boolean;
    error: string | null;
};

const initialState: SessionState = {
    user: null,
    isInitialized: false,
    isLoading: false,
    error: null,
}

export const SessionStore = signalStore(
    {providedIn: 'root'},
    withState(initialState),

    withComputed(({user, isInitialized}) => ({
        isAuthenticated: computed(() => user() !== null),
        isReady: computed(() => isInitialized()),
    })),
    withMethods((store, api = inject(UserApi), router = inject(Router)) => ({
        
        login(email: string, password: string): void {
            api.signIn(email, password).pipe(
                switchMap(() => api.getUserData())
            ).subscribe({
                next: (user) => {
                    patchState(store, {
                        user,
                        isInitialized: true
                    });

                    router.navigate(['/user/dashboard']);
                },
                error: (err) => {
                    console.error('Error signing in.', err);
                }
            });
        }
    }))
);