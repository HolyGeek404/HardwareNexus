import {ChangeDetectionStrategy, Component, effect, inject, input, output, signal} from '@angular/core';
import {BaseHttpService} from '../../../../../shared/services/base-http.service';
import {ProductTypes} from '../../../../../shared/models/enums';
import type {FilterModel} from '../models/filter.model';

export interface ProductFilterSelection {
    team: string[];
    socket: string[];
    cores: string[];
    architecture: string[];
    priceMin: string;
    priceMax: string;
}

@Component({
    selector: 'app-product-filter',
    standalone: true,
    templateUrl: './product-filter.components.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './product-filter.components.css'
})
export class FilterComponent {
    productType = input.required<ProductTypes>();
    filtersApplied = output<ProductFilterSelection>();
    filters = signal<FilterModel | null>(null);
    isLoading = signal(false);
    selection = signal<ProductFilterSelection>({
        team: [],
        socket: [],
        cores: [],
        architecture: [],
        priceMin: '',
        priceMax: ''
    });
    private functionsService = inject(BaseHttpService);

    constructor() {
        effect((onCleanup) => {
            const type = this.productType();
            this.isLoading.set(true);
            this.filters.set(null);
            this.selection.set({
                team: [],
                socket: [],
                cores: [],
                architecture: [],
                priceMin: '',
                priceMax: ''
            });
            const sub = this.functionsService.getProductFilters(type).subscribe({
                next: (data) => {
                    this.filters.set(data);
                    this.isLoading.set(false);
                },
                error: (err) => {
                    console.error(`Error loading filters for ${type}:`, err);
                    this.filters.set(null);
                    this.isLoading.set(false);
                }
            });

            onCleanup(() => sub.unsubscribe());
        }, {allowSignalWrites: true});
    }

    toggleSelection(group: keyof ProductFilterSelection, option: string, checked: boolean): void {
        this.selection.update((current) => {
            const list = current[group];
            if (!Array.isArray(list)) {
                return current;
            }
            const nextList = checked
                ? Array.from(new Set([...list, option]))
                : list.filter((item) => item !== option);
            return {...current, [group]: nextList};
        });
    }

    updatePrice(field: 'priceMin' | 'priceMax', value: string): void {
        this.selection.update((current) => ({...current, [field]: value}));
    }

    onClear(): void {
        this.selection.set({
            team: [],
            socket: [],
            cores: [],
            architecture: [],
            priceMin: '',
            priceMax: ''
        });
        this.filtersApplied.emit(this.selection());
    }

    onApply(): void {
        this.filtersApplied.emit(this.selection());
    }
}
