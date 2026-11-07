import {ChangeDetectionStrategy, Component, inject, signal} from '@angular/core';
import {UserModel} from '../../../../../shared/models/models';
import {SessionStore} from "../../../../../shared/store/session.store";

@Component({
    selector: 'app-dashboard',
    imports: [],
    providers: [SessionStore],
    templateUrl: './dashboard.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
    session = inject(SessionStore);
    user = signal<UserModel | null>(null)
    panels = ['Overview', 'Orders', 'Settings', 'Security', 'Support'];
    activePanel = 'Overview';

    ngOnInit() {
        if (!this.session.isAuthenticated() || !this.session.isReady()) {

        }
    }

    selectPanel(panel: string): void {
        this.activePanel = panel;
    }

    signOut(): void {

    }
}
