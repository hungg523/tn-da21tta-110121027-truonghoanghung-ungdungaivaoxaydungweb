// home.component.ts
import { Component, OnInit, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CategoryService, Category } from '../../services/category.service';
import { BannerService, Banner } from '../../services/banner.service';
import { ProductService, ProductVariant, ProductResponse, RecommendResponse, RecommendErrorResponse } from '../../services/product.service';
import { LuckyWheelComponent } from '../../components/lucky-wheel/lucky-wheel.component';
import { gsap } from 'gsap';
import { ScrollTrigger } from 'gsap/ScrollTrigger';
import * as THREE from 'three';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';
import type { GLTF } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { VndCurrencyPipe } from '../../pipes/vnd-currency.pipe';
import { ChatBoxComponent } from '../../components/chat-box/chat-box.component';
gsap.registerPlugin(ScrollTrigger);

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, LuckyWheelComponent, VndCurrencyPipe, ChatBoxComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, AfterViewInit {
  @ViewChild('parallaxSection') parallaxSection!: ElementRef;
  @ViewChild('model3dContainer') model3dContainer!: ElementRef;
  @ViewChild('flashSaleSlider') flashSaleSlider!: ElementRef;
  @ViewChild('recommendedSlider') recommendedSlider!: ElementRef;
  
  categories: Category[] = [];
  banners: Banner[] = [];
  currentBannerIndex: number = 0;
  private bannerInterval: any;
  flashSaleProducts: ProductVariant[] = [];
  recommendedProducts: RecommendResponse[] = [];
  showRecommendedSection = true;
  recommendedCurrentPage = 0;
  private scene!: THREE.Scene;
  private camera!: THREE.PerspectiveCamera;
  private renderer!: THREE.WebGLRenderer;
  private model!: THREE.Group;
  private controls!: OrbitControls;
  modelFiles = [
    '/assets/3dmodels/iphone_15_pro_max.glb',
    '/assets/3dmodels/mackbook_air_15_m2.glb',
  ];
  modelScales = [2.5, 2.2]; // scale mới cho từng model
  modelPositionsY = [-0.5, -0.5]; // căn chỉnh vị trí Y
  modelPositionsZ = [-8, -5]; // căn chỉnh vị trí Z nếu cần
  currentModelIndex = 0;
  private modelInterval: any;
  private modelFade = 1;
  private isUserInteracting = false;
  private autoRotateTimeout: any;
  private lastPointerX = 0;
  private lastPointerY = 0;
  private lastRotationX = 0;
  private lastRotationY = 0;
  private isPointerDown = false;
  private initRotation = { x: 0, y: 0, z: 0 };
  flashSaleCountdown: string = '';
  private flashSaleTimer: any;
  flashSaleCurrentPage = 0;

  constructor(
    private categoryService: CategoryService,
    private bannerService: BannerService,
    private productService: ProductService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadCategories();
    this.loadBanners();
    this.loadFlashSaleProducts();
    this.loadRecommendedProducts();
    this.startBannerAutoSlide();
  }

  ngAfterViewInit() {
    this.init3DModel();
    this.initAnimations();
    this.startModelAutoSwitch();
    // Lắng nghe scroll để cập nhật dot
    this.flashSaleSlider.nativeElement.addEventListener('scroll', () => this.updateFlashSaleDot());
  }

  ngOnDestroy() {
    if (this.modelInterval) clearInterval(this.modelInterval);
    if (this.bannerInterval) clearInterval(this.bannerInterval);
  }

  private loadCategories() {
    this.categoryService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  private loadBanners() {
    this.bannerService.getAllBanners(1, 0).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.banners = response.data.map(banner => {
            let link = banner.link;
            if (link) {
              // Chuyển link tuyệt đối về relative path nếu cần
              link = link.replace(/^https?:\/\/[^\/]+/, '');
              // Giải mã nhiều lần nếu bị encode
              try {
                while (/%[0-9A-Fa-f]{2}/.test(link)) {
                  const decoded = decodeURIComponent(link);
                  if (decoded === link) break;
                  link = decoded;
                }
              } catch {}
            }
            return {
              ...banner,
              link
            };
          });
          this.startBannerAutoSlide();
        } else {
          console.error('Không có banner để hiển thị');
        }
      },
      error: (error) => {
        console.error('Error loading banners:', error);
      }
    });
  }

  private loadFlashSaleProducts() {
    this.productService.getFlashSaleProducts().subscribe({
      next: (response: ProductResponse) => {
        if (response.isSuccess) {
          this.flashSaleProducts = response.data.productVariants;
          this.startFlashSaleTimer();
        }
      },
      error: (error) => {
        console.error('Error loading flash sale products:', error);
      }
    });
  }

  private loadRecommendedProducts() {
    this.productService.getRecommendedProducts().subscribe({
      next: (response) => {
        if (Array.isArray(response)) {
          this.recommendedProducts = response;
          this.showRecommendedSection = true;
        } else {
          this.showRecommendedSection = false;
        }
      },
      error: (error) => {
        console.error('Error loading recommended products:', error);
        this.showRecommendedSection = false;
      }
    });
  }

  private initParallax() {
    // Đã loại bỏ hiệu ứng parallax
  }

  private init3DModel() {
    try {
      this.scene = new THREE.Scene();
      const container = this.model3dContainer.nativeElement;
      this.camera = new THREE.PerspectiveCamera(60, container.clientWidth / container.clientHeight, 0.1, 1000);
      this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
      this.renderer.setClearColor(0x000000, 0);
      this.renderer.setSize(container.clientWidth, container.clientHeight);
      container.appendChild(this.renderer.domElement);
      // Custom mouse/touch event để xoay model tại chỗ
      container.addEventListener('mousedown', (e: MouseEvent) => this.onPointerDown(e));
      container.addEventListener('mousemove', (e: MouseEvent) => this.onPointerMove(e));
      container.addEventListener('mouseup', (e: MouseEvent) => this.onPointerUp(e));
      container.addEventListener('mouseleave', (e: MouseEvent) => this.onPointerUp(e));
      container.addEventListener('touchstart', (e: TouchEvent) => this.onPointerDown(e));
      container.addEventListener('touchmove', (e: TouchEvent) => this.onPointerMove(e));
      container.addEventListener('touchend', (e: TouchEvent) => this.onPointerUp(e));
      const ambientLight = new THREE.AmbientLight(0xffffff, 1.2);
      this.scene.add(ambientLight);
      const directionalLight = new THREE.DirectionalLight(0xffffff, 1.2);
      directionalLight.position.set(5, 10, 7.5);
      this.scene.add(directionalLight);
      const pointLight = new THREE.PointLight(0xffffff, 0.8);
      pointLight.position.set(-5, -5, 5);
      this.scene.add(pointLight);
      this.camera.position.set(0, 0, 4);
      this.controls = new OrbitControls(this.camera, this.renderer.domElement);
      this.controls.enableDamping = true;
      this.controls.dampingFactor = 0.1;
      this.controls.enableZoom = true;
      this.controls.minDistance = 2;
      this.controls.maxDistance = 8;
      this.controls.enablePan = false;
      this.controls.minPolarAngle = Math.PI / 3; // Giới hạn nhìn lên
      this.controls.maxPolarAngle = Math.PI * 2 / 3; // Giới hạn nhìn xuống
      this.load3DModel();
      window.addEventListener('resize', () => {
        const width = container.clientWidth;
        const height = container.clientHeight;
        this.camera.aspect = width / height;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(width, height);
      });
      let autoRotateY = 0;
      const animate = () => {
        requestAnimationFrame(animate);
        if (this.model) {
          const modelSection = document.querySelector('.model-showcase');
          let inView = false;
          if (modelSection) {
            const rect = modelSection.getBoundingClientRect();
            inView = rect.top < window.innerHeight && rect.bottom > 0;
          }
          if (inView && !this.isUserInteracting) {
            autoRotateY += 0.01;
            this.model.rotation.y = autoRotateY;
          }
        }
        this.renderer.render(this.scene, this.camera);
      };
      animate();
    } catch (e) {
      console.error('Lỗi khởi tạo 3D model:', e);
    }
  }

  private initAnimations() {
    // Animation cho các phần tử khi scroll
    gsap.fromTo('.hero-banner .hero-content h1',
      {y: 30, opacity: 0},
      {y: 0, opacity: 1, duration: 0.8, ease: 'power2.out',
        scrollTrigger: {
          trigger: '.hero-banner',
          start: 'top 80%',
          toggleActions: 'play none none reverse'
        }
      }
    );
    gsap.fromTo('.hero-banner .hero-content p',
      {y: 30, opacity: 0},
      {y: 0, opacity: 1, duration: 0.8, delay: 0.2, ease: 'power2.out',
        scrollTrigger: {
          trigger: '.hero-banner',
          start: 'top 80%',
          toggleActions: 'play none none reverse'
        }
      }
    );
    gsap.fromTo('.flash-sale-section',
      {y: 80, opacity: 0},
      {y: 0, opacity: 1, duration: 1, ease: 'power2.out',
        scrollTrigger: {
          trigger: '.flash-sale-section',
          start: 'top 80%',
          toggleActions: 'play none none reverse'
        }
      }
    );
    gsap.fromTo('.recommended-products',
      {y: 80, opacity: 0},
      {y: 0, opacity: 1, duration: 1, ease: 'power2.out',
        scrollTrigger: {
          trigger: '.recommended-products',
          start: 'top 80%',
          toggleActions: 'play none none reverse'
        }
      }
    );
    gsap.fromTo('.features-section',
      {y: 80, opacity: 0},
      {y: 0, opacity: 1, duration: 1, ease: 'power2.out',
        scrollTrigger: {
          trigger: '.features-section',
          start: 'top 80%',
          toggleActions: 'play none none reverse'
        }
      }
    );
  }

  changeModel(index: number) {
    this.currentModelIndex = index;
    this.load3DModel();
    this.restartModelAutoSwitch();
  }

  startModelAutoSwitch() {
    if (this.modelInterval) clearInterval(this.modelInterval);
    this.modelInterval = setInterval(() => {
      this.fadeOutModel(() => {
        this.currentModelIndex = (this.currentModelIndex + 1) % this.modelFiles.length;
        this.load3DModel(true);
      });
    }, 20000); // 9 giây
  }

  restartModelAutoSwitch() {
    this.startModelAutoSwitch();
  }

  private fadeOutModel(callback: () => void) {
    if (!this.model) return callback();
    gsap.to(this.model.scale, {x: 0.1, y: 0.1, z: 0.1, duration: 0.5, ease: 'power2.in', onComplete: callback});
    gsap.to(this.model, {visible: false, delay: 0.5});
  }

  private fadeInModel() {
    if (!this.model) return;
    this.model.visible = true;
    gsap.fromTo(this.model.scale, {x: 0.1, y: 0.1, z: 0.1}, {
      x: this.modelScales[this.currentModelIndex],
      y: this.modelScales[this.currentModelIndex],
      z: this.modelScales[this.currentModelIndex],
      duration: 0.7,
      ease: 'back.out(1.7)'
    });
    gsap.fromTo(this.model, {opacity: 0}, {opacity: 1, duration: 0.7});
  }

  private load3DModel(fadeIn = false) {
    try {
      if (this.model) {
        this.scene.remove(this.model);
      }
      const loader = new GLTFLoader();
      loader.load(this.modelFiles[this.currentModelIndex], (gltf: GLTF) => {
        this.model = gltf.scene;
        const scale = this.modelScales[this.currentModelIndex] || 2.2;
        const posY = this.modelPositionsY[this.currentModelIndex] || -0.5;
        const posZ = this.modelPositionsZ[this.currentModelIndex] || 0;
        this.model.scale.set(scale, scale, scale);
        this.model.position.set(0, posY, posZ);
        this.model.visible = !fadeIn;
        this.scene.add(this.model);
        // Lưu lại rotation ban đầu
        this.initRotation = {
          x: this.model.rotation.x,
          y: this.model.rotation.y,
          z: this.model.rotation.z
        };
        if (fadeIn) {
          setTimeout(() => this.fadeInModel(), 100);
        }
      });
    } catch (e) {
      console.error('Lỗi load 3D model:', e);
    }
  }

  private onPointerDown(e: MouseEvent | TouchEvent) {
    this.isPointerDown = true;
    this.isUserInteracting = true;
    if (this.autoRotateTimeout) clearTimeout(this.autoRotateTimeout);
    if (e instanceof MouseEvent) {
      this.lastPointerX = e.clientX;
      this.lastPointerY = e.clientY;
    } else {
      this.lastPointerX = e.touches[0].clientX;
      this.lastPointerY = e.touches[0].clientY;
    }
    if (this.model) {
      this.lastRotationX = this.model.rotation.x;
      this.lastRotationY = this.model.rotation.y;
    }
  }

  private onPointerMove(e: MouseEvent | TouchEvent) {
    if (!this.isPointerDown || !this.model) return;
    let clientX, clientY;
    if (e instanceof MouseEvent) {
      clientX = e.clientX;
      clientY = e.clientY;
    } else {
      clientX = e.touches[0].clientX;
      clientY = e.touches[0].clientY;
    }
    const deltaX = clientX - this.lastPointerX;
    const deltaY = clientY - this.lastPointerY;
    this.model.rotation.y = this.lastRotationY + deltaX * 0.01;
    this.model.rotation.x = this.lastRotationX + deltaY * 0.1;
    // Giới hạn góc X để model không bị lật ngược
    this.model.rotation.x = Math.max(-Math.PI / 2, Math.min(Math.PI / 2, this.model.rotation.x));
  }

  private onPointerUp(e: MouseEvent | TouchEvent) {
    this.isPointerDown = false;
    if (this.autoRotateTimeout) clearTimeout(this.autoRotateTimeout);
    this.autoRotateTimeout = setTimeout(() => {
      this.isUserInteracting = false;
      // Animate model về vị trí ban đầu (rotation ban đầu khi load model)
      if (this.model) {
        gsap.to(this.model.rotation, {
          x: this.initRotation.x,
          y: this.initRotation.y,
          z: this.initRotation.z,
          duration: 0.7,
          ease: 'power2.out',
        });
      }
    }, 2000);
  }

  private startBannerAutoSlide() {
    if (this.bannerInterval) {
      clearInterval(this.bannerInterval);
    }
    this.bannerInterval = setInterval(() => {
      this.nextBanner();
    }, 5000); // Chuyển banner mỗi 5 giây
  }

  nextBanner() {
    this.currentBannerIndex = (this.currentBannerIndex + 1) % this.banners.length;
  }

  previousBanner() {
    this.currentBannerIndex = (this.currentBannerIndex - 1 + this.banners.length) % this.banners.length;
  }

  goToBanner(index: number) {
    this.currentBannerIndex = index;
    this.startBannerAutoSlide(); // Reset timer khi người dùng chọn banner
  }

  private updateFlashSaleCountdown() {
    if (!this.flashSaleProducts || this.flashSaleProducts.length === 0) {
      this.flashSaleCountdown = '';
      return;
    }
    const endDateStr = this.flashSaleProducts[0].flashSaleEndDate;
    if (!endDateStr) {
      this.flashSaleCountdown = '';
      return;
    }
    const endDate = new Date(endDateStr).getTime();
    const now = Date.now();
    let diff = endDate - now;
    if (diff <= 0) {
      this.flashSaleCountdown = 'Đã kết thúc';
      return;
    }
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    diff -= days * 1000 * 60 * 60 * 24;
    const hours = Math.floor(diff / (1000 * 60 * 60));
    diff -= hours * 1000 * 60 * 60;
    const minutes = Math.floor(diff / (1000 * 60));
    diff -= minutes * 1000 * 60;
    const seconds = Math.floor(diff / 1000);
    let result = '';
    if (days > 0) result += days + ' ngày ';
    if (hours > 0 || days > 0) result += hours + ' giờ ';
    if (minutes > 0 || hours > 0 || days > 0) result += minutes + ' phút ';
    result += seconds + ' giây';
    this.flashSaleCountdown = 'Còn lại: ' + result.trim();
  }

  private startFlashSaleTimer() {
    if (this.flashSaleTimer) clearInterval(this.flashSaleTimer);
    this.updateFlashSaleCountdown();
    this.flashSaleTimer = setInterval(() => {
      this.updateFlashSaleCountdown();
    }, 1000);
  }

  navigateToDetail(variantId: number) {
    this.router.navigate(['/product-detail', variantId]);
  }

  slideFlashSale(direction: 'prev' | 'next') {
    const slider = this.flashSaleSlider.nativeElement;
    const card = slider.querySelector('.product-card');
    if (!card) return;
    const perPage = Math.min(6, Math.max(1, Math.floor(slider.offsetWidth / card.offsetWidth)));
    if (direction === 'prev') {
      slider.scrollBy({ left: -card.offsetWidth * perPage, behavior: 'smooth' });
    } else {
      slider.scrollBy({ left: card.offsetWidth * perPage, behavior: 'smooth' });
    }
    setTimeout(() => this.updateFlashSaleDot(), 400);
  }

  updateFlashSaleDot() {
    const slider = this.flashSaleSlider.nativeElement;
    const card = slider.querySelector('.product-card');
    if (!card) return;
    const perPage = Math.min(6, Math.max(1, Math.floor(slider.offsetWidth / card.offsetWidth)));
    const totalPage = Math.ceil(this.flashSaleProducts.length / perPage);
    const scrollLeft = slider.scrollLeft;
    const maxScroll = slider.scrollWidth - slider.clientWidth;
    if (maxScroll > 0 && scrollLeft >= maxScroll - 1) {
      this.flashSaleCurrentPage = totalPage - 1;
    } else {
      const page = Math.round(scrollLeft / (card.offsetWidth * perPage));
      this.flashSaleCurrentPage = Math.min(page, totalPage - 1);
    }
  }

  getFlashSaleDots(): any[] {
    const slider = this.flashSaleSlider?.nativeElement;
    const card = slider?.querySelector('.product-card');
    if (!slider || !card) return [];
    const perPage = Math.min(6, Math.max(1, Math.floor(slider.offsetWidth / card.offsetWidth)));
    return Array(Math.ceil(this.flashSaleProducts.length / perPage)).fill(0);
  }

  slideRecommended(direction: 'prev' | 'next') {
    const slider = this.recommendedSlider.nativeElement;
    const card = slider.querySelector('.product-card');
    if (!card) return;
    const perPage = Math.min(6, Math.max(1, Math.floor(slider.offsetWidth / card.offsetWidth)));
    if (direction === 'prev') {
      slider.scrollBy({ left: -card.offsetWidth * perPage, behavior: 'smooth' });
    } else {
      slider.scrollBy({ left: card.offsetWidth * perPage, behavior: 'smooth' });
    }
    setTimeout(() => this.updateRecommendedDot(), 400);
  }

  updateRecommendedDot() {
    const slider = this.recommendedSlider.nativeElement;
    const card = slider.querySelector('.product-card');
    if (!card) return;
    const perPage = Math.min(6, Math.max(1, Math.floor(slider.offsetWidth / card.offsetWidth)));
    const totalPage = Math.ceil(this.recommendedProducts.length / perPage);
    const scrollLeft = slider.scrollLeft;
    const maxScroll = slider.scrollWidth - slider.clientWidth;
    if (maxScroll > 0 && scrollLeft >= maxScroll - 1) {
      this.recommendedCurrentPage = totalPage - 1;
    } else {
      const page = Math.round(scrollLeft / (card.offsetWidth * perPage));
      this.recommendedCurrentPage = Math.min(page, totalPage - 1);
    }
  }

  getRecommendedDots(): any[] {
    const slider = this.recommendedSlider?.nativeElement;
    const card = slider?.querySelector('.product-card');
    if (!slider || !card) return [];
    const perPage = Math.min(6, Math.max(1, Math.floor(slider.offsetWidth / card.offsetWidth)));
    return Array(Math.ceil(this.recommendedProducts.length / perPage)).fill(0);
  }
}
