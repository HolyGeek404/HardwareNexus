import {ChangeDetectionStrategy, Component, DestroyRef, OnInit, signal} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {ProductService} from '../../../services/product.service';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {CpuModel} from '../models/cpu.model';
import {ProductTypesEnum} from '../../../enums/product-types.enum';
import {NgOptimizedImage} from '@angular/common';
import {loadProduct} from '../../../functions/product.functions';

@Component({
  selector: 'app-cpu',
  imports: [
    NgOptimizedImage
  ],
  templateUrl: './cpu.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './cpu.component.css',
})
export class CpuComponent implements OnInit {
  protected cpuProduct = signal<CpuModel | undefined>(undefined)

  constructor(
    private router: ActivatedRoute,
    private productService: ProductService,
    private destroyRef: DestroyRef
  ) {}

  ngOnInit() {
    loadProduct<CpuModel>(ProductTypesEnum.CPU, this.productService, this.router)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(product => this.cpuProduct.set(product));
  }
}
