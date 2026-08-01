import {User} from "../../business/user/models/model";
import {signalStore, withComputed, withState} from '@ngrx/signals';
import {computed} from "@angular/core";

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
);