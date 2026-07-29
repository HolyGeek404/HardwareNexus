import {ChangeDetectionStrategy, Component, DestroyRef, OnInit, signal} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {ProductService} from '../../../../user/services/product.service';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {CoolerModel} from '../models/cooler.model';
import {ProductTypesEnum} from '../../../enums/product-types.enum';
import {NgOptimizedImage} from '@angular/common';
import {loadProduct} from '../../../../user/functions/product.functions';

@Component({
  selector: 'app-cooler',
  imports: [
    NgOptimizedImage
  ],
  templateUrl: './cooler.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './cooler.component.css',
})
export class CoolerComponent implements OnInit {
  protected coolerProduct = signal<CoolerModel | undefined>(undefined)

  constructor(
    private router: ActivatedRoute,
    private productService: ProductService,
    private destroyRef: DestroyRef
  ) {}

  ngOnInit() {
    loadProduct<CoolerModel>(ProductTypesEnum.COOLER, this.productService, this.router)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(product => this.coolerProduct.set(product));
  }
}
