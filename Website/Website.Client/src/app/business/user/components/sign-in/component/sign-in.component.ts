import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import {RouterLink} from '@angular/router';
import {SessionStore} from "../../../../../shared/store/session.store";

@Component({
    selector: 'app-sign-in',
    imports: [RouterLink],
    templateUrl: './sign-in.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './sign-in.component.css'
})
export class SignInComponent {
    session = inject(SessionStore);

    public stateMsg: string | null = null;

    ngOnInit() {
        const state = history.state as { message?: string };

        if (state?.message) {
            this.stateMsg = state.message;
        }
    }

    onSubmit() {
        this.session.login();
    }
}
