import {ChangeDetectionStrategy, Component} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {Router, RouterLink} from '@angular/router';
import {SessionService} from '../../../../../shared/services/session.service';
import {createSignInForm} from "../models/forms";

@Component({
    selector: 'app-sign-in',
    imports: [
        FormsModule,
        ReactiveFormsModule,
        RouterLink
    ],
    templateUrl: './sign-in.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './sign-in.component.css'
})
export class SignInComponent {
    public stateMsg: string | null = null;
    signInForm = createSignInForm();

    errors: string[] = [];

    constructor(private router: Router, private userSessionService: SessionService) {
    }

    ngOnInit() {
        const state = history.state as { message?: string };

        if (state?.message) {
            this.stateMsg = state.message;
        }
    }

    onSubmit() {
        if (this.validate() && this.signInForm.valid) {
            this.userSessionService.signIn(this.signInForm.controls.email.value!,
                this.signInForm.controls.password.value!).subscribe({
                next: (result) => {
                    this.router.navigate(['/user/dashboard']);
                },
                error: (err) => console.error(`Error signing in.`, err)
            });

        }
    }
}
