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
            <div class="text-end" dir="rtl">
                <h5 class="mb-3">⚠️ تنبيه قبل تغيير اللغة</h5>
                <p>النصوص الحالية (اسم الموقع , القوائم مثل: الخدمات/العملاء/الحجوزات , العملة...إلخ) مُدخلة يدوياً بالعربية.</p>
                <p>عند التبديل للإنجليزية، ستظل النصوص "<strong>بالعربية</strong>" حتى تقوم بتغييرها من:</p>
                <p class="fw-bold text-primary mb-0">الإعدادات ← الملف الشخصي ← النصوص المخصصة</p>
            </div>
        ` : `
            <div class="text-start" dir="ltr">
                <h5 class="mb-3">⚠️ Language Switch Notice</h5>
                <p>Current texts (Website name, menus such as: Services/Customers/Appointments, Currency...etc.) are manually set in <strong>Arabic</strong>.</p>
                <p>Switching to English will keep Arabic texts until you change them from:</p>
                <p class="fw-bold text-primary mb-0">Settings → Profile → Custom Labels</p>
            </div>
        `;

                  Swal.fire({
                        title: '',
                        html: html,
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonText: isRtl ? 'تغيير اللغة فقط' : 'Switch Language Only',
                        cancelButtonText: isRtl ? 'إلغاء' : 'Cancel',
                        confirmButtonColor: '#2c6e7c',
                        allowOutsideClick: false
                  }).then((result) => {
                        this.markWarningSeen();

                        if (result.isConfirmed) {
                              this.doSwitch(newLang);
                        }
                        // Cancel: do nothing, stay on current page
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