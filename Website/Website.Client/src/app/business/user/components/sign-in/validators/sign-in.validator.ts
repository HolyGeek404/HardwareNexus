import {SignInFormModel} from "../models/models";
import {FormGroup} from "@angular/forms";

export class SignInValidator {
    errorMessages = {
        email: {
            required: 'Email is required',
            email: 'Please enter a valid email address'
        },
        password: {
            required: 'Password is required',
            pattern: 'Password must contain at least 8 characters, one uppercase letter, one lowercase letter, one number, and one special character (@$!%*?&)'
        }
    };

    validate(signInForm: FormGroup<SignInFormModel>, errors: string[]): boolean {
        let isValid = true;

        // validate email
        if (!signInForm.controls.email.valid) {
            if (signInForm.controls.email.errors!['required'] != null) {
                errors.push(this.errorMessages.email.required);
                isValid = false;
            }
            if (signInForm.controls.email.errors!['email'] != null) {
                errors.push(this.errorMessages.email.email);
                isValid = false;
            }
        }

        //validate password
        if (!signInForm.controls.password.valid) {
            if (signInForm.controls.password.errors!['required'] != null) {
                errors.push(this.errorMessages.password.required);
                isValid = false;
            }
            if (signInForm.controls.password.errors!['pattern'] != null) {
                errors.push(this.errorMessages.password.pattern);
                isValid = false;
            }
        }

        return isValid;
    }
}