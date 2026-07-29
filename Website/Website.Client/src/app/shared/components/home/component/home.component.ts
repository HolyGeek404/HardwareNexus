import {ChangeDetectionStrategy, Component} from '@angular/core';
import {ProductTypes} from '../../models/product/ProductTypesEnum';
import {CategoryCardComponent} from '../categories/category-card/category-card.components';

@Component({
  selector: 'app-home',
  imports: [
    CategoryCardComponent

  ],
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './home.css'
})
export class HomeComponent {
  categories = Object.values(ProductTypes);
}
