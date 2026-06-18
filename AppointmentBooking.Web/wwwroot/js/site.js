// ============================================
// APPOINTMENT BOOKING - DYNAMIC SITE JS
// ============================================

(function () {
      'use strict';

      const Translations = {
            ar: {
                  confirmDeleteTitle: 'هل أنت متأكد؟',
                  confirmDeleteText: 'لا يمكن التراجع عن هذا الإجراء!',
                  confirmButtonText: 'نعم، متأكد',
                  cancelButtonText: 'إلغاء',
                  loadingMessage: 'جاري التحميل...',
                  ajaxErrorMessage: 'حدث خطأ في الاتصال',
                  themeLightTitle: 'الوضع الفاتح',
                  themeDarkTitle: 'الوضع الداكن',
                  periodAM: 'ص',
                  periodPM: 'م'
            },
            en: {
                  confirmDeleteTitle: 'Are you sure?',
                  confirmDeleteText: 'This action cannot be undone!',
                  confirmButtonText: 'Yes, I\'m sure',
                  cancelButtonText: 'Cancel',
                  loadingMessage: 'Loading...',
                  ajaxErrorMessage: 'Connection error occurred',
                  themeLightTitle: 'Light Mode',
                  themeDarkTitle: 'Dark Mode',
                  periodAM: 'AM',
                  periodPM: 'PM'
            }
      };

      // Get current language from Config or default to 'ar'
      const getCurrentLang = () => {
            return (window.BizConfig && window.BizConfig.lang) ? window.BizConfig.lang : 'ar';
      };

      // Translation helper
      const t = (key) => {
            const lang = getCurrentLang();
            return Translations[lang] && Translations[lang][key] ? Translations[lang][key] : Translations.ar[key] || key;
      };

      // Global Config from Layout
      const Config = window.BizConfig || {
            lang: 'ar',
            dir: 'rtl',
            isRtl: true,
            currency: 'ج.م',
            slotDuration: 30,
            text: t('confirmDeleteText'),
      };

      // ============================================
      // THEME MANAGEMENT - FIXED
      // ============================================

      const ThemeManager = {
            init() {
                  this.toggleBtn = document.getElementById('themeToggle');
                  this.html = document.documentElement;
                  this.body = document.body;

                  // Load saved theme
                  const savedTheme = localStorage.getItem('biz-theme') || 'light';
                  this.setTheme(savedTheme);

                  // Listen for system theme changes
                  this.systemThemeListener();

                  if (this.toggleBtn) {
                        this.toggleBtn.addEventListener('click', (e) => {
                              e.preventDefault();
                              this.toggle();
                        });
                  }
            },

            setTheme(theme) {
                  // Set data attribute on html element (Bootstrap uses this)
                  this.html.setAttribute('data-bs-theme', theme);

                  // Also set on body for custom CSS
                  if (this.body) {
                        this.body.setAttribute('data-bs-theme', theme);
                  }

                  // Save to localStorage
                  localStorage.setItem('biz-theme', theme);

                  // Update button icon
                  this.updateButtonIcon(theme);

                  // Update meta theme-color
                  this.updateMetaColor(theme);

                  // Dispatch event for other components
                  window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
            },

            updateButtonIcon(theme) {
                  if (!this.toggleBtn) return;

                  const icon = this.toggleBtn.querySelector('i');
                  if (icon) {
                        if (theme === 'dark') {
                              icon.className = 'bi bi-sun-fill';
                        } else {
                              icon.className = 'bi bi-moon-stars';
                        }
                  }

                  // Update tooltip
                  const title = theme === 'dark' ? t('themeLightTitle') : t('themeDarkTitle');
                  this.toggleBtn.setAttribute('title', title);
            },

            updateMetaColor(theme) {
                  let metaTheme = document.querySelector('meta[name="theme-color"]');
                  if (!metaTheme) {
                        metaTheme = document.createElement('meta');
                        metaTheme.setAttribute('name', 'theme-color');
                        document.head.appendChild(metaTheme);
                  }

                  const color = theme === 'dark'
                        ? (Config.colors?.darkPrimary || '#1a1a2e')
                        : (Config.colors?.primary || '#2c6e7c');
                  metaTheme.setAttribute('content', color);
            },

            systemThemeListener() {
                  // Listen for system theme changes
                  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                        const savedTheme = localStorage.getItem('biz-theme');
                        // Only auto-switch if user hasn't manually set a preference
                        if (!savedTheme) {
                              this.setTheme(e.matches ? 'dark' : 'light');
                        }
                  });
            },

            toggle() {
                  const current = this.html.getAttribute('data-bs-theme');
                  const next = current === 'dark' ? 'light' : 'dark';
                  this.setTheme(next);
            },

            getCurrentTheme() {
                  return this.html.getAttribute('data-bs-theme') || 'light';
            },
      };

      // ============================================
      // LANGUAGE MANAGEMENT
      // ============================================
      const LanguageManager = {
            init() {
                  this.toggleBtn = document.getElementById('languageToggle');
                  if (!this.toggleBtn) return;

                  this.currentLang = this.getCurrentLanguage();
                  this.updateButtonUI();
                  this.updateDocumentDirection();

                  this.toggleBtn.addEventListener('click', (e) => {
                        e.preventDefault();
                        this.toggle();
                  });

                  window.addEventListener('languageChanged', (e) => {
                        this.currentLang = e.detail.language;
                        this.updateButtonUI();
                        this.updateDocumentDirection();
                  });
            },

            getCurrentLanguage() {
                  const htmlLang = document.documentElement.lang || '';
                  if (htmlLang) return htmlLang;

                  const savedLang = localStorage.getItem('biz-language');
                  if (savedLang) return savedLang;

                  const cookieLang = this.getCookie('UserLanguage');
                  if (cookieLang) return cookieLang;

                  return (Config && Config.lang) ? Config.lang : 'ar';
            },

            getCookie(name) {
                  const value = `; ${document.cookie}`;
                  const parts = value.split(`; ${name}=`);
                  if (parts.length === 2) return parts.pop().split(';').shift();
                  return null;
            },

            hasSeenWarning() {
                  return localStorage.getItem('biz-lang-warning') === 'true';
            },

            markWarningSeen() {
                  localStorage.setItem('biz-lang-warning', 'true');
            },
            showFirstTimeWarning(newLang) {
                  const isRtl = this.currentLang === 'ar';

                  const html = isRtl ? `
        <div class="text-end" dir="rtl" style="font-family: 'Segoe UI', 'Tahoma', sans-serif;">
            <div class="d-flex align-items-center gap-3 mb-4">
                <div class="flex-shrink-0">
                    <div style="
                        width: 48px; 
                        height: 48px; 
                        background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
                        border-radius: 12px;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        box-shadow: 0 4px 14px rgba(245, 158, 11, 0.35);
                    ">
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                            <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
                            <line x1="12" y1="9" x2="12" y2="13"></line>
                            <line x1="12" y1="17" x2="12.01" y2="17"></line>
                        </svg>
                    </div>
                </div>
                <div class="flex-grow-1">
                    <h5 class="mb-1 fw-bold" style="color: #1f2937; font-size: 1.15rem;">تنبيه قبل تغيير اللغة</h5>
                    <p class="mb-0 text-muted" style="font-size: 0.875rem;">يرجى قراءة المعلومات التالية بعناية</p>
                </div>
            </div>
            
            <div style="
                background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%);
                border: 1px solid #f59e0b;
                border-radius: 12px;
                padding: 1rem 1.25rem;
                margin-bottom: 1.25rem;
            ">
                <p class="mb-2" style="color: #5d920ec1; line-height: 1.7;">
                   عند تغيير لغة التطبيق، سيتم ترجمة جميع النصوص الافتراضية تلقائيًا.
                </p>
                <p class="mb-2" style="color: #92400e; line-height: 1.7;">
                   أما النصوص المخصصة التي قمت بتعديلها بنفسك <span style="background: #fde68a; padding: 2px 8px; border-radius: 6px; font-weight: 500;">(اسم الموقع، عناصر القائمة، العملة...وغيرها)</span> فستظل كما أدخلتها ولن تتغير تلقائيًا مع تغيير اللغة.
                </p>
                <p class="mb-0" style="color: #92400e; line-height: 1.7;">
                   إذا كنت ترغب في عرض هذه النصوص بلغة أخرى، يمكنك تعديلها من إعدادات الملف الشخصي.
                </p>
            </div>

            <div style="
                background: #f0f9ff;
                border: 1px solid #0ea5e9;
                border-radius: 12px;
                padding: 1rem 1.25rem;
            ">
                <p class="mb-2 fw-semibold" style="color: #0369a1; font-size: 0.9rem;">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#0ea5e9" stroke-width="2" style="vertical-align: middle; margin-left: 6px;">
                        <circle cx="12" cy="12" r="10"></circle>
                        <path d="M12 16v-4"></path>
                        <path d="M12 8h.01"></path>
                    </svg>
                    لتعديل النصوص، انتقل إلى:
                </p>
                <div class="d-flex align-items-center gap-2" style="color: #0284c7;">
                    <span class="badge" style="background: #e0f2fe; color: #0369a1; font-size: 0.8rem; padding: 6px 12px; border-radius: 8px;">الإعدادات</span>
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#0ea5e9" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"></polyline></svg>
                    <span class="badge" style="background: #e0f2fe; color: #0369a1; font-size: 0.8rem; padding: 6px 12px; border-radius: 8px;">الملف الشخصي</span>
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#0ea5e9" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"></polyline></svg>
                    <span class="badge" style="background: #bae6fd; color: #0369a1; font-size: 0.8rem; padding: 6px 12px; border-radius: 8px; font-weight: 700;">النصوص المخصصة</span>
                </div>
            </div>
        </div>
    ` : `
        <div class="text-start" dir="ltr" style="font-family: 'Segoe UI', 'Tahoma', sans-serif;">
            <div class="d-flex align-items-center gap-3 mb-4">
                <div class="flex-shrink-0">
                    <div style="
                        width: 48px; 
                        height: 48px; 
                        background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
                        border-radius: 12px;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        box-shadow: 0 4px 14px rgba(245, 158, 11, 0.35);
                    ">
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                            <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
                            <line x1="12" y1="9" x2="12" y2="13"></line>
                            <line x1="12" y1="17" x2="12.01" y2="17"></line>
                        </svg>
                    </div>
                </div>
                <div class="flex-grow-1">
                    <h5 class="mb-1 fw-bold" style="color: #1f2937; font-size: 1.15rem;">Language Switch Notice</h5>
                    <p class="mb-0 text-muted" style="font-size: 0.875rem;">Please read the following information carefully</p>
                </div>
            </div>
            
            <div style="
                background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%);
                border: 1px solid #f59e0b;
                border-radius: 12px;
                padding: 1rem 1.25rem;
                margin-bottom: 1.25rem;
            ">
                <p class="mb-2" style="color: #5d920ec1; line-height: 1.7;">
                  When changing the application language, all default system texts will be translated automatically.
                </p>
                <p class="mb-2" style="color: #92400e; line-height: 1.7;">
                  However, any custom texts that you have modified yourself <span style="background: #fde68a; padding: 2px 8px; border-radius: 6px; font-weight: 500;">(such as the website name, menu items, currency labels, ...and similar settings)</span> will remain unchanged and will continue to appear exactly as entered.
                </p>
                <p class="mb-0" style="color: #92400e; line-height: 1.7;">
                  If you want these texts to appear in another language, you can update them from the Profile Settings page.
                </p>
            </div>

            <div style="
                background: #f0f9ff;
                border: 1px solid #0ea5e9;
                border-radius: 12px;
                padding: 1rem 1.25rem;
            ">
                <p class="mb-2 fw-semibold" style="color: #0369a1; font-size: 0.9rem;">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#0ea5e9" stroke-width="2" style="vertical-align: middle; margin-right: 6px;">
                        <circle cx="12" cy="12" r="10"></circle>
                        <path d="M12 16v-4"></path>
                        <path d="M12 8h.01"></path>
                    </svg>
                    To edit texts, navigate to:
                </p>
                <div class="d-flex align-items-center gap-2" style="color: #0284c7;">
                    <span class="badge" style="background: #e0f2fe; color: #0369a1; font-size: 0.8rem; padding: 6px 12px; border-radius: 8px;">Settings</span>
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#0ea5e9" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"></polyline></svg>
                    <span class="badge" style="background: #e0f2fe; color: #0369a1; font-size: 0.8rem; padding: 6px 12px; border-radius: 8px;">Profile</span>
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#0ea5e9" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"></polyline></svg>
                    <span class="badge" style="background: #bae6fd; color: #0369a1; font-size: 0.8rem; padding: 6px 12px; border-radius: 8px; font-weight: 700;">Custom Labels</span>
                </div>
            </div>
        </div>
    `;

                  Swal.fire({
                        title: '',
                        html: html,
                        showCancelButton: true,
                        confirmButtonText: isRtl ? 'تغيير اللغة فقط' : 'Switch Language Only',
                        cancelButtonText: isRtl ? 'إلغاء' : 'Cancel',
                        confirmButtonColor: '#2c6e7c',
                        cancelButtonColor: '#9ca3af',
                        allowOutsideClick: false,
                        customClass: {
                              popup: 'language-warning-popup',
                              confirmButton: 'btn fw-semibold px-4 py-2',
                              cancelButton: 'btn fw-semibold px-4 py-2'
                        },
                        width: '32rem',
                        padding: '1.5rem',
                        showCloseButton: false,
                        backdrop: 'rgba(0, 0, 0, 0.4)',
                        didOpen: () => {
                              // Add subtle animation to the popup
                              const popup = Swal.getPopup();
                              if (popup) {
                                    popup.style.borderRadius = '16px';
                                    popup.style.boxShadow = '0 25px 50px -12px rgba(0, 0, 0, 0.25)';
                              }
                        }
                  }).then((result) => {
                        this.markWarningSeen();
                        if (result.isConfirmed) {
                              this.doSwitch(newLang);
                        }
                  });
            },

            doSwitch(lang) {
                  const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
                  window.location.href = `/Account/SetLanguage?culture=${lang}&returnUrl=${returnUrl}`;
            },

            async toggle() {
                  const newLang = this.currentLang === 'ar' ? 'en' : 'ar';

                  // Show warning first time switching to English
                  if (newLang === 'en' && !this.hasSeenWarning()) {
                        this.showFirstTimeWarning(newLang);
                        return;
                  }

                  if (window.BizLoading) {
                        window.BizLoading.show(t('loadingMessage'));
                  }

                  try {
                        this.doSwitch(newLang);
                  } catch (error) {
                        if (window.BizToast) {
                              window.BizToast('error', t('ajaxErrorMessage'));
                        }
                        if (window.BizLoading) {
                              window.BizLoading.hide();
                        }
                  }
            },

            updateButtonUI() {
                  if (!this.toggleBtn) return;

                  const icon = this.toggleBtn.querySelector('i');
                  const span = this.toggleBtn.querySelector('span');

                  if (icon) {
                        icon.className = 'bi bi-translate';
                  }

                  if (span) {
                        span.textContent = this.currentLang === 'ar' ? 'الإنجليزية' : 'Arabic';
                  }

                  const title = this.currentLang === 'ar' ? 'English' : 'العربية';
                  this.toggleBtn.setAttribute('title', title);
            },

            updateDocumentDirection() {
                  const isRtl = this.currentLang === 'ar';
                  document.documentElement.dir = isRtl ? 'rtl' : 'ltr';
                  document.documentElement.setAttribute('dir', isRtl ? 'rtl' : 'ltr');

                  if (isRtl) {
                        document.body.classList.add('rtl');
                        document.body.classList.remove('ltr');
                  } else {
                        document.body.classList.add('ltr');
                        document.body.classList.remove('rtl');
                  }
            },

            getAntiForgeryToken() {
                  const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                  return tokenInput ? tokenInput.value : '';
            }
      };

      // ============================================
      // NAVBAR SCROLL EFFECT
      // ============================================

      const NavbarManager = {
            init() {
                  this.navbar = document.querySelector('.biz-navbar');
                  if (!this.navbar) return;

                  window.addEventListener('scroll', () => this.handleScroll());
                  this.handleScroll(); // Initial check
            },

            handleScroll() {
                  if (window.scrollY > 50) {
                        this.navbar.style.boxShadow = '0 4px 20px rgba(0,0,0,0.15)';
                        this.navbar.style.padding = '0.5rem 0';
                  } else {
                        this.navbar.style.boxShadow = '';
                        this.navbar.style.padding = '0.75rem 0';
                  }
            }
      };

      // ============================================
      // ACTIVE NAV LINK
      // ============================================

      const NavigationManager = {
            init() {
                  const currentPath = window.location.pathname;
                  document.querySelectorAll('.biz-navbar .nav-link').forEach(link => {
                        const href = link.getAttribute('href');
                        if (href && href !== '/' && currentPath.startsWith(href)) {
                              link.classList.add('active');
                        } else if (href === '/' && currentPath === '/') {
                              link.classList.add('active');
                        }
                  });
            }
      };

      // ============================================
      // ANIMATIONS ON SCROLL
      // ============================================

      const AnimationManager = {
            init() {
                  this.destroy(true); // Clean up existing observer

                  this.observer = new IntersectionObserver((entries) => {
                        entries.forEach(entry => {
                              if (entry.isIntersecting) {
                                    entry.target.classList.add('biz-animate');
                                    this.observer.unobserve(entry.target);
                              }
                        });
                  }, { threshold: 0.1, rootMargin: '50px' }); // Added rootMargin for better performance

                  this.setupAnimations();
            },

            setupAnimations() {
                  const elements = document.querySelectorAll('.biz-card, .biz-stat-card');

                  elements.forEach(el => {
                        // Check if already being observed
                        if (!el.hasAttribute('data-animation-observer')) {
                              el.setAttribute('data-animation-observer', 'true');
                              this.observer.observe(el);
                        }
                  });
            },

            destroy(cleanAll = false) {
                  if (this.observer) {
                        this.observer.disconnect();
                        this.observer = null;
                  }

                  // Reset animation attributes
                  const elements = document.querySelectorAll('[data-animation-observer]');
                  elements.forEach(el => {
                        el.removeAttribute('data-animation-observer');
                  });

                  // If it's a complete cleaning, remove the classes as well.
                  if (cleanAll) {
                        const animatedElements = document.querySelectorAll('.biz-animate');
                        animatedElements.forEach(el => {
                              el.classList.remove('biz-animate');
                        });
                  }

                  this.isActive = false;
            },

            reinit(container = document) {
                  if (!this.observer || !this.isActive) {
                        this.init();
                        return;
                  }

                  const newElements = container.querySelectorAll('.biz-card, .biz-stat-card:not([data-animation-observer])');
                  newElements.forEach(el => {
                        if (!el.hasAttribute('data-animation-observer')) {
                              el.setAttribute('data-animation-observer', 'true');
                              this.observer?.observe(el);
                        }
                  });
            },

            cleanupOnUnload() {
                  if (this.observer) {
                        this.observer.disconnect();
                        this.observer = null;
                  }
                  this.isActive = false;
            }
      };

      // ============================================
      // CLEANUP MANAGER - PREVENT MEMORY LEAKS
      // ============================================

      const CleanupManager = {
            init() {
                  // Before page unload
                  window.addEventListener('beforeunload', () => this.cleanup());

                  // Page hide (for mobile)
                  window.addEventListener('pagehide', () => this.cleanup());

                  // For Turbolinks/Turbo
                  document.addEventListener('turbolinks:before-cache', () => this.cleanup());
                  document.addEventListener('turbo:before-cache', () => this.cleanup());

                  // For HTMX
                  document.addEventListener('htmx:beforeSwap', () => this.cleanup());

                  this.setupMutationObserver();
            },

            setupMutationObserver() {
                  const domObserver = new MutationObserver((mutations) => {
                        let needsReinit = false;

                        mutations.forEach(mutation => {
                              if (mutation.addedNodes.length) {
                                    mutation.addedNodes.forEach(node => {
                                          if (node.nodeType === 1 && // HTML item
                                                (node.multiple?.('.biz-card, .biz-stat-card') ||
                                                      node.querySelectorAll?.('.biz-card, .biz-stat-card').length > 0)) {
                                                needsReinit = true;
                                          }
                                    });
                              }
                        });

                        if (needsReinit && AnimationManager.reinit) {
                              AnimationManager.reinit();
                        }
                  });

                  domObserver.observe(document.body, {
                        childList: true,
                        subtree: true
                  });

                  this.domObserver = domObserver;
            },

            cleanup() {
                  if (AnimationManager.cleanupOnUnload) {
                        AnimationManager.cleanupOnUnload();
                  } else if (AnimationManager.destroy) {
                        AnimationManager.destroy(true);
                  }

                  if (this.domObserver) {
                        this.domObserver.disconnect();
                        this.domObserver = null;
                  }

                  // Add other managers that need cleanup
                  if (window.DynamicContentManager?.destroy) {
                        window.DynamicContentManager.destroy();
                  }
            }
      };

      // ============================================
      // CONFIRMATION DIALOGS (SweetAlert2)
      // ============================================

      window.BizConfirm = function (options = {}) {
            const defaults = {
                  title: t('confirmDeleteTitle'),
                  text: t('confirmDeleteText'),
                  icon: 'warning',
                  showCancelButton: true,
                  confirmButtonColor: Config.colors?.primary || '#2c6e7c',
                  cancelButtonColor: '#6c757d',
                  confirmButtonText: t('confirmButtonText'),
                  cancelButtonText: t('cancelButtonText'),
                  reverseButtons: Config.isRtl
            };

            return Swal.fire({ ...defaults, ...options });
      };

      window.BizToast = function (type, message) {
            const Toast = Swal.mixin({
                  toast: true,
                  position: Config.isRtl ? 'top-start' : 'top-end',
                  showConfirmButton: false,
                  timer: 3000,
                  timerProgressBar: true,
                  didOpen: (toast) => {
                        toast.addEventListener('mouseenter', Swal.stopTimer);
                        toast.addEventListener('mouseleave', Swal.resumeTimer);
                  }
            });

            Toast.fire({
                  icon: type,
                  title: message
            });
      };

      // ============================================
      // FORM VALIDATION HELPERS
      // ============================================

      window.BizValidation = {
            initForms() {
                  document.querySelectorAll('form[data-validate]').forEach(form => {
                        form.addEventListener('submit', (e) => this.handleSubmit(e));
                  });
            },

            handleSubmit(e) {
                  const form = e.target;
                  if (!form.checkValidity()) {
                        e.preventDefault();
                        e.stopPropagation();

                        // Focus first invalid field
                        const firstInvalid = form.querySelector(':invalid');
                        if (firstInvalid) {
                              firstInvalid.focus();
                              firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        }
                  }
                  form.classList.add('was-validated');
            },

            // Phone validation (Egyptian format)
            isValidPhone(phone) {
                  const regex = /^(01[0-2,5]{1}[0-9]{8})$/;
                  return regex.test(phone.replace(/\s/g, ''));
            },

            // Email validation
            isValidEmail(email) {
                  const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                  return regex.test(email);
            }
      };

      // ============================================
      // TIME SLOTS GENERATOR
      // ============================================

      window.TimeSlotGenerator = {
            generate(startTime, endTime, durationMinutes) {
                  const slots = [];
                  const [startHour, startMin] = startTime.split(':').map(Number);
                  const [endHour, endMin] = endTime.split(':').map(Number);

                  let current = new Date();
                  current.setHours(startHour, startMin, 0);

                  const end = new Date();
                  end.setHours(endHour, endMin, 0);

                  while (current < end) {
                        const timeStr = current.toTimeString().slice(0, 5);
                        slots.push({
                              value: timeStr,
                              label: this.formatTime(timeStr)
                        });
                        current.setMinutes(current.getMinutes() + durationMinutes);
                  }

                  return slots;
            },

            formatTime(time24) {
                  const [hours, minutes] = time24.split(':').map(Number);
                  const period = hours >= 12 ? t('periodPM') : t('periodAM');
                  const hours12 = hours % 12 || 12;
                  return `${hours12}:${minutes.toString().padStart(2, '0')} ${period}`;
            }
      };

      // ============================================
      // CURRENCY FORMATTER
      // ============================================

      window.BizCurrency = {
            format(amount, currency = Config.currency) {
                  return `${Number(amount).toLocaleString('ar-EG')} ${currency}`;
            },

            parse(formatted) {
                  return parseFloat(formatted.replace(/[^\d.]/g, ''));
            }
      };

      // ============================================
      // DATE HELPERS
      // ============================================

      window.BizDate = {
            format(date, format = 'DD/MM/YYYY') {
                  const d = new Date(date);
                  const day = d.getDate().toString().padStart(2, '0');
                  const month = (d.getMonth() + 1).toString().padStart(2, '0');
                  const year = d.getFullYear();

                  return format
                        .replace('DD', day)
                        .replace('MM', month)
                        .replace('YYYY', year);
            },

            isToday(date) {
                  const d = new Date(date);
                  const today = new Date();
                  return d.toDateString() === today.toDateString();
            },

            isPast(date) {
                  const d = new Date(date);
                  const today = new Date();
                  today.setHours(0, 0, 0, 0);
                  return d < today;
            },

            addDays(date, days) {
                  const result = new Date(date);
                  result.setDate(result.getDate() + days);
                  return result;
            }
      };

      // ============================================
      // LOADING SPINNER
      // ============================================

      window.BizLoading = {
            show(message = t('loadingMessage')) {
                  Swal.fire({
                        title: message,
                        allowOutsideClick: false,
                        allowEscapeKey: false,
                        didOpen: () => {
                              Swal.showLoading();
                        }
                  });
            },

            hide() {
                  Swal.close();
            }
      };

      // ============================================
      // AJAX HELPERS
      // ============================================

      window.BizAjax = {
            async get(url, options = {}) {
                  return this.request('GET', url, null, options);
            },

            async post(url, data, options = {}) {
                  return this.request('POST', url, data, options);
            },

            async request(method, url, data, options = {}) {
                  const defaults = {
                        headers: {
                              'X-Requested-With': 'XMLHttpRequest',
                              'X-CSRF-TOKEN': this.getCSRFToken(),
                              'Accept': 'application/json'
                        },
                        ...options
                  };

                  if (data && !(data instanceof FormData)) {
                        defaults.headers['Content-Type'] = 'application/json';
                        defaults.body = JSON.stringify(data);
                  } else if (data) {
                        defaults.body = data;
                  }

                  try {
                        const response = await fetch(url, {
                              method,
                              ...defaults
                        });

                        if (!response.ok) throw new Error(response.statusText);
                        return await response.json();
                  } catch (error) {
                        window.BizToast?.('error', t('ajaxErrorMessage'));
                        throw error;
                  }
            },

            getCSRFToken() {
                  const token = document.querySelector('input[name="__RequestVerificationToken"]');
                  return token ? token.value : '';
            }
      };



      // ============================================
      // INITIALIZATION
      // ============================================

      document.addEventListener('DOMContentLoaded', () => {
            ThemeManager.init();
            LanguageManager.init();

            if (window.requestIdleCallback) {
                  requestIdleCallback(() => {
                        NavbarManager.init();
                        NavigationManager.init();
                        AnimationManager.init();
                        BizValidation.initForms();
                        CleanupManager.init();
                  }, { timeout: 1000 });
            } else {
                  setTimeout(() => {
                        NavbarManager.init();
                        NavigationManager.init();
                        AnimationManager.init();
                        BizValidation.initForms();
                        CleanupManager.init();
                  }, 10);
            }
      });

})();