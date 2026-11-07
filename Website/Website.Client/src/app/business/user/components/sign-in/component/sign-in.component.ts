import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {Router, RouterLink} from '@angular/router';
import {createSignInForm} from "../models/forms";
import {SessionStore} from "../../../../../shared/store/session.store";
import {UserApi} from "../../../api/user.api";

@Component({
    selector: 'app-sign-in',
    imports: [
        FormsModule,
        ReactiveFormsModule,
        RouterLink
    ],
    providers: [SessionStore],
    templateUrl: './sign-in.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './sign-in.component.css'
})
export class SignInComponent {
    api = inject(UserApi);
    session = inject(SessionStore);

    public stateMsg: string | null = null;
    signInForm = createSignInForm();

    errors: string[] = [];

    constructor(private router: Router) {
    }

    ngOnInit() {
        const state = history.state as { message?: string };

        if (state?.message) {
            this.stateMsg = state.message;
        }
    }

    onSubmit() {
        if (this.signInForm.invalid) {
            this.signInForm.markAllAsTouched();
            return;
        }

        const {email, password} = this.signInForm.getRawValue();

        this.session.login(email!, password!);
    }
}
