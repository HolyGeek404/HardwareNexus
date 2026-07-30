import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import {BaseHttpService} from '../../../shared/services/base-http.service';
import {ActivatedRoute, Router} from '@angular/router';
import type {AccountVerificationRequest} from '../models/account-verification.model';

@Component({
  selector: 'app-services',
  template: ``,
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true
})
export class AccountVerificationService {
  private functionsService = inject(BaseHttpService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  ngOnInit() {
    const userEmail = this.route.snapshot.queryParamMap.get('userEmail');
    const key = this.route.snapshot.queryParamMap.get('key');

    if (!userEmail || !key) {
      console.error("Missing query params");
      return;
    }

    // Map into interface
    const payload: AccountVerificationRequest = {
      userEmail: userEmail,
      key: key
    };

    // Call API
    this.functionsService.accountVerification(payload).subscribe({
      next: (result) => {
        if (result) {
          this.router.navigate(['/user/sign-in'], {
            state: { message: 'Account activated! You can now sign-in.' }
          })
        } else {
          this.router.navigate(['/user/sign-in'], {
            state: { message: 'Sorry! Something went wrong.' }
          })
        }
      }
    });
  }
}
