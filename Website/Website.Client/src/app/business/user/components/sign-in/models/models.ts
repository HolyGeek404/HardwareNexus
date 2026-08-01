import {FormControl} from "@angular/forms";

export type SignInFormModel = {
    email: FormControl<string | null>;
    password: FormControl<string | null>;
};