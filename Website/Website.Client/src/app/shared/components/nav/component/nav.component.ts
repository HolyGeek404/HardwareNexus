import {ChangeDetectionStrategy, Component, inject, OnInit, signal} from '@angular/core';
import {NgOptimizedImage} from '@angular/common';
import {RouterLink} from '@angular/router';
import {SessionStore} from "../../../store/session.store";

@Component({
    selector: 'app-nav',
    imports: [
        NgOptimizedImage,
        RouterLink
    ],
    providers: [SessionStore],
    templateUrl: './nav.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './nav.component.css'
})
export class Nav implements OnInit {
    session = inject(SessionStore);
    isLoggedIn = signal<boolean>(false);

    ngOnInit() {

    }
}
