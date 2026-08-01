import {ChangeDetectionStrategy, Component, computed, input} from '@angular/core';
import {NgOptimizedImage} from '@angular/common';
import {RouterLink} from '@angular/router';

@Component({
    selector: 'app-category-card',
    imports: [
        NgOptimizedImage,
        RouterLink
    ],
    templateUrl: './category-card.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './category-card.component.css'
})
export class CategoryCardComponent {
    category = input.required<string>();
    categoryImgPath = computed(() => {
        return "categories/" + this.category().toLowerCase() + ".svg";
    });
}
