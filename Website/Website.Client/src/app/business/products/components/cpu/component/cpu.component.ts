import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {ProductApi} from '../../../api/product.api';
import {toObservable, toSignal} from '@angular/core/rxjs-interop';
import {CpuModel} from '../models/cpu.model';
import {ProductTypes} from '../../../../../shared/models/enums';
import {NgOptimizedImage} from '@angular/common';
import {filter, map, switchMap} from "rxjs";

@Component({
    selector: 'app-cpu',
    imports: [
        NgOptimizedImage
    ],
    templateUrl: './cpu.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './cpu.component.css',
})
export class CpuComponent {
    readonly category = ProductTypes.CPU;
    api = inject(ProductApi);
    route = inject(ActivatedRoute);
    id = toSignal(this.route.paramMap.pipe(
        map(params => params.get('id'))), {initialValue: null}
    );
    product = toSignal(toObservable(this.id).pipe(
        filter((id): id is string => id !== null),
        switchMap((id) => this.api.getProduct<CpuModel>(this.category, id!))
    ));
}
