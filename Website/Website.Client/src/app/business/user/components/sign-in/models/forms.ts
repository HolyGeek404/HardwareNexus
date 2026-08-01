import {FormControl, FormGroup, Validators} from "@angular/forms";
import {SignInFormModel} from "./models";

export function createSignInForm(): FormGroup<SignInFormModel> {
    return new FormGroup<SignInFormModel>({
        email: new FormControl('', [
            Validators.required,
            Validators.email,
        ]),
        password: new FormControl('', [
            Validators.required,
            Validators.pattern(
                /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/
            ),
        ]),
    });
}