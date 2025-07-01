import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ProductImage {
  id: number;
  title: string;
  url: string;
  position: number;
}

export interface ProductAttributes {
  avId: number;
  attributeId: number;
  variantId: number;
  attributeName: string;
  imageUrl: string;
  attributeValue: string;
  price: number;
}

export interface ProductDetail {
  key: string;
  value: string;
}

export interface ProductVariant {
  Productid: number;
  variantId: number;
  name: string;
  description: string;
  price: number;
  discountPrice?: number;
  stock: number;
  reservedStock: number;
  actualStock: number;
  soldQuantity: number;
  totalReviews: number;
  averageRating: number;
  isActived: number;
  images: ProductImage[];
  productsAttributes: ProductAttributes[];
  details: ProductDetail[];
  isInWishlist?: boolean;
  flashSaleStartDate?: string;
  flashSaleEndDate?: string;
}

export interface ProductResponse {
  isSuccess: boolean;
  statusCode: number;
  data: {
    totalItems: number;
    productVariants: ProductVariant[];
  };
}

export interface AttributeResponse {
  isSuccess: boolean;
  statusCode: number;
  data: ProductAttributes[];
}

export interface ProductDetailResponse {
  isSuccess: boolean;
  data: ProductVariant;
  message: string;
}

export interface AttributeValue {
  attributeId: number;
  value: string;
  id: number;
}

export interface AttributeValueResponse {
  isSuccess: boolean;
  statusCode: number;
  data: AttributeValue[];
}

export interface SimilarProductsResponse {
  isSuccess: boolean;
  data: ProductVariant[];
  message: string;
}

export interface RecommendResponse {
  variant_id: number;
  score: number;
  product_name: string;
  description: string;
  price: number;
  discount_price: number;
  stock: number;
  reserved_stock: number;
  actual_stock: number;
  average_rating: number;
  sold_quantity: number;
  total_reviews: number;
  images: ProductImage[];
  details: ProductDetail[];
}

export interface RecommendErrorResponse {
  detail: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrlV1 = `${environment.apiUrlV1}/product-variant`;
  private apiAI = `${environment.apiAI}`;
  constructor(private http: HttpClient) { }

  getProducts(options?: {
    categoryId?: number | null;
    color?: string | null;
    storage?: string | null;
    minPrice?: number | null;
    maxPrice?: number | null;
    isDescending?: boolean;
    isAscending?: boolean;
    skip?: number;
    take?: number;
  }): Observable<ProductResponse> {
    let params = new HttpParams().set('isActived', '1');
    if (options) {
      if (options.categoryId !== null && options.categoryId !== undefined) {
        params = params.set('categoryId', options.categoryId.toString());
      }
      if (options.color !== null && options.color !== undefined) {
        params = params.set('color', options.color.toString());
      }
      if (options.storage !== null && options.storage !== undefined) {
        params = params.set('storage', options.storage.toString());
      }
      if (options.minPrice !== null && options.minPrice !== undefined) {
        params = params.set('minPrice', options.minPrice.toString());
      }
      if (options.maxPrice !== null && options.maxPrice !== undefined) {
        params = params.set('maxPrice', options.maxPrice.toString());
      }
      if (options.isDescending) {
        params = params.set('isDescending', 'true');
      }
      if (options.isAscending) {
        params = params.set('isAscending', 'true');
      }
      if (options.skip !== undefined) {
        params = params.set('skip', options.skip.toString());
      }
      if (options.take !== undefined) {
        params = params.set('take', options.take.toString());
      }
    }
    return this.http.get<ProductResponse>(`${this.apiUrlV1}/get-all`, { params });
  }

  getProductById(id: number): Observable<ProductVariant> {
    return this.http.get<ProductVariant>(`${this.apiUrlV1}/get-detail/${id}`);
  }

  searchProducts(options: { name: string; skip?: number; take?: number }): Observable<ProductResponse> {
    let params = new HttpParams()
      .set('name', options.name)
      .set('isActived', '1');
    if (options.skip !== undefined) {
      params = params.set('skip', options.skip.toString());
    }
    if (options.take !== undefined) {
      params = params.set('take', options.take.toString());
    }
    return this.http.get<ProductResponse>(`${this.apiUrlV1}/search`, { params });
  }

  getProductDetail(id: number): Observable<ProductDetailResponse> {
    return this.http.get<ProductDetailResponse>(`${this.apiUrlV1}/get-detail/${id}`);
  }

  getColorOptions(id: number): Observable<AttributeResponse> {
    return this.http.get<AttributeResponse>(`${this.apiUrlV1}/color-options/${id}`);
  }

  searchProductByName(name: string): Observable<ProductResponse> {
    return this.http.get<ProductResponse>(`${this.apiUrlV1}/search`, { params: { name } });
  }

  getAllAttributeValues(): Observable<AttributeValueResponse> {
    return this.http.get<AttributeValueResponse>(`${environment.apiUrlV1}/attribute-value/get-all`);
  }

  getSimilarProducts(id: number): Observable<SimilarProductsResponse> {
    return this.http.get<SimilarProductsResponse>(`${this.apiUrlV1}/similar/${id}`);
  }

  getFlashSaleProducts(): Observable<ProductResponse> {
    return this.http.get<ProductResponse>(`${this.apiUrlV1}/flash-sale`);
  }

  getRecommendedProducts(nItems: number = 12): Observable<RecommendResponse[] | RecommendErrorResponse> {
    return this.http.post<RecommendResponse[] | RecommendErrorResponse>(`${this.apiAI}/recommend`, { n_items: nItems });
  }
}
